using Microsoft.Data.Sqlite;
using Fish.Models;

namespace Fish.Repositories;

public class LeaderboardRepository(string connectionString)
{
    public async Task Save(FishCatch fish)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
                INSERT INTO leaderboard (user_id, fish_name, adjective, weight_kg, points, rarity, is_special, caught_at)
                VALUES ($userId, $fishName, $adjective, $weightKg, $points, $rarity, $isSpecial, $caughtAt)
                """;

        command.Parameters.AddWithValue("$userId", (long)fish.UserId);
        command.Parameters.AddWithValue("$fishName", fish.FishName);
        command.Parameters.AddWithValue("$adjective", fish.AdjectiveName);
        command.Parameters.AddWithValue("$weightKg", (double)fish.WeightKg);
        command.Parameters.AddWithValue("$points", fish.Points);
        command.Parameters.AddWithValue("$rarity", fish.Rarity);
        command.Parameters.AddWithValue("$isSpecial", fish.IsSpecial ? 1 : 0);
        command.Parameters.AddWithValue("$caughtAt", fish.CaughtAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<FishCatch>> GetTop10Best()
    {
        var result = new List<FishCatch>();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM leaderboard ORDER BY points DESC LIMIT 10";

        await using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var fish = new FishCatch()
            {
                UserId = (ulong)(long)reader["user_id"],
                FishName = (string)reader["fish_name"],
                AdjectiveName = (string)reader["adjective"],
                WeightKg = (decimal)(double)reader["weight_kg"],
                Points = (int)(long)reader["points"],
                Rarity = (int)(long)reader["rarity"],
                IsSpecial = (long)reader["is_special"] == 1,
                CaughtAt = DateTime.Parse((string)reader["caught_at"])
            };

            result.Add(fish);
        }

        return result;
    }

    public async Task<List<FishCatch>> GetTop10Worse()
    {
        var result = new List<FishCatch>();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM leaderboard ORDER BY points ASC LIMIT 10";

        await using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var fish = new FishCatch()
            {
                UserId = (ulong)(long)reader["user_id"],
                FishName = (string)reader["fish_name"],
                AdjectiveName = (string)reader["adjective"],
                WeightKg = (decimal)(double)reader["weight_kg"],
                Points = (int)(long)reader["points"],
                Rarity = (int)(long)reader["rarity"],
                IsSpecial = (long)reader["is_special"] == 1,
                CaughtAt = DateTime.Parse((string)reader["caught_at"])
            };

            result.Add(fish);
        }

        return result;
    }
}
