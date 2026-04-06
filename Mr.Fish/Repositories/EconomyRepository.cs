using Fish.Models;
using Microsoft.Data.Sqlite;

namespace Fish.Repositories;

public class EconomyRepository(string connectionString)
{
    public async Task<UserEconomy> GetOrCreate(ulong userId)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var ensure = connection.CreateCommand();
        ensure.CommandText = """
                INSERT OR IGNORE INTO user_economy (user_id, scales, rod_tier)
                VALUES ($userId, 0, 0)
                """;
        ensure.Parameters.AddWithValue("$userId", (long)userId);
        await ensure.ExecuteNonQueryAsync();

        var select = connection.CreateCommand();
        select.CommandText = """
                SELECT scales, rod_tier
                FROM user_economy
                WHERE user_id = $userId
                """;
        select.Parameters.AddWithValue("$userId", (long)userId);

        await using var reader = await select.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return new UserEconomy { UserId = userId, Scales = 0, RodTier = 0 };
        }

        return new UserEconomy
        {
            UserId = userId,
            Scales = (int)(long)reader["scales"],
            RodTier = (int)(long)reader["rod_tier"]
        };
    }

    public async Task<UserEconomy> AddScales(ulong userId, int amount)
    {
        if (amount <= 0)
            return await GetOrCreate(userId);

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var ensure = connection.CreateCommand();
        ensure.CommandText = """
                INSERT OR IGNORE INTO user_economy (user_id, scales, rod_tier)
                VALUES ($userId, 0, 0)
                """;
        ensure.Parameters.AddWithValue("$userId", (long)userId);
        await ensure.ExecuteNonQueryAsync();

        var update = connection.CreateCommand();
        update.CommandText = """
                UPDATE user_economy
                SET scales = scales + $amount
                WHERE user_id = $userId
                """;
        update.Parameters.AddWithValue("$userId", (long)userId);
        update.Parameters.AddWithValue("$amount", amount);
        await update.ExecuteNonQueryAsync();

        return await GetOrCreate(userId);
    }

    public async Task<(bool Success, string Message, UserEconomy Profile)> TryBuyRod(ulong userId, RodOffer rod)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var ensure = connection.CreateCommand();
        ensure.CommandText = """
                INSERT OR IGNORE INTO user_economy (user_id, scales, rod_tier)
                VALUES ($userId, 0, 0)
                """;
        ensure.Parameters.AddWithValue("$userId", (long)userId);
        await ensure.ExecuteNonQueryAsync();

        var update = connection.CreateCommand();
        update.CommandText = """
                UPDATE user_economy
                SET scales = scales - $price,
                    rod_tier = $tier
                WHERE user_id = $userId
                  AND scales >= $price
                  AND rod_tier < $tier
                """;
        update.Parameters.AddWithValue("$price", rod.Price);
        update.Parameters.AddWithValue("$tier", rod.Tier);
        update.Parameters.AddWithValue("$userId", (long)userId);
        int affectedRows = await update.ExecuteNonQueryAsync();

        var profile = await GetOrCreate(userId);

        if (affectedRows > 0)
            return (true, "Покупка прошла успешно.", profile);

        if (rod.Tier <= profile.RodTier)
            return (false, "Эта удочка уже куплена или у тебя есть лучше.", profile);

        if (profile.Scales < rod.Price)
            return (false, "Не хватает чешуек на покупку.", profile);

        return (false, "Покупка не прошла, попробуй еще раз.", profile);
    }
}
