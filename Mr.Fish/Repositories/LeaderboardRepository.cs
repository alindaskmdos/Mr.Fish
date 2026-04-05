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

    public async Task<List<FishCatch>> GetTop10BestByPoints()
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
                Points = (decimal)(double)reader["points"],
                Rarity = (int)(long)reader["rarity"],
                IsSpecial = (long)reader["is_special"] == 1,
                CaughtAt = DateTime.Parse((string)reader["caught_at"])
            };

            result.Add(fish);
        }

        return result;
    }

    public async Task<List<FishCatch>> GetTop10WorstByPoints()
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
                Points = (decimal)(double)reader["points"],
                Rarity = (int)(long)reader["rarity"],
                IsSpecial = (long)reader["is_special"] == 1,
                CaughtAt = DateTime.Parse((string)reader["caught_at"])
            };

            result.Add(fish);
        }

        return result;
    }

    public async Task<List<FishCatch>> GetTop10BestByWeight()
    {
        var result = new List<FishCatch>();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM leaderboard ORDER BY weight_kg DESC LIMIT 10";

        await using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var fish = new FishCatch()
            {
                UserId = (ulong)(long)reader["user_id"],
                FishName = (string)reader["fish_name"],
                AdjectiveName = (string)reader["adjective"],
                WeightKg = (decimal)(double)reader["weight_kg"],
                Points = (decimal)(double)reader["points"],
                Rarity = (int)(long)reader["rarity"],
                IsSpecial = (long)reader["is_special"] == 1,
                CaughtAt = DateTime.Parse((string)reader["caught_at"])
            };

            result.Add(fish);
        }

        return result;
    }

    public async Task<List<FishCatch>> GetTop10WorstByWeight()
    {
        var result = new List<FishCatch>();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM leaderboard ORDER BY weight_kg ASC LIMIT 10";

        await using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var fish = new FishCatch()
            {
                UserId = (ulong)(long)reader["user_id"],
                FishName = (string)reader["fish_name"],
                AdjectiveName = (string)reader["adjective"],
                WeightKg = (decimal)(double)reader["weight_kg"],
                Points = (decimal)(double)reader["points"],
                Rarity = (int)(long)reader["rarity"],
                IsSpecial = (long)reader["is_special"] == 1,
                CaughtAt = DateTime.Parse((string)reader["caught_at"])
            };

            result.Add(fish);
        }

        return result;
    }

    public async Task<List<FishCatch>> GetTop3RarestByUser(ulong userId)
    {
        var result = new List<FishCatch>();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
                SELECT * FROM leaderboard
                WHERE user_id = $userId
                ORDER BY rarity DESC, points DESC, caught_at DESC
                LIMIT 3
                """;
        command.Parameters.AddWithValue("$userId", (long)userId);

        await using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var fish = new FishCatch()
            {
                UserId = (ulong)(long)reader["user_id"],
                FishName = (string)reader["fish_name"],
                AdjectiveName = (string)reader["adjective"],
                WeightKg = (decimal)(double)reader["weight_kg"],
                Points = (decimal)(double)reader["points"],
                Rarity = (int)(long)reader["rarity"],
                IsSpecial = (long)reader["is_special"] == 1,
                CaughtAt = DateTime.Parse((string)reader["caught_at"])
            };

            result.Add(fish);
        }

        return result;
    }

    public async Task<UserFishingStats?> GetUserStats(ulong userId)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
                SELECT
                    COUNT(*) AS total_catches,
                    COALESCE(SUM(weight_kg), 0) AS total_weight_kg,
                    COALESCE(AVG(weight_kg), 0) AS avg_weight_kg,
                    COALESCE(SUM(points), 0) AS total_points,
                    COALESCE(MAX(points), 0) AS best_points,
                    COALESCE(MAX(rarity), 0) AS max_rarity,
                    COALESCE((
                        SELECT fish_name
                        FROM leaderboard
                        WHERE user_id = $userId
                        ORDER BY points DESC, caught_at DESC
                        LIMIT 1
                    ), '') AS best_fish_name,
                    COALESCE((
                        SELECT fish_name
                        FROM leaderboard
                        WHERE user_id = $userId
                        ORDER BY rarity DESC, points DESC, caught_at DESC
                        LIMIT 1
                    ), '') AS rarest_fish_name
                FROM leaderboard
                WHERE user_id = $userId
                """;
        command.Parameters.AddWithValue("$userId", (long)userId);

        await using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        int totalCatches = (int)(long)reader["total_catches"];
        if (totalCatches == 0)
            return null;

        return new UserFishingStats
        {
            TotalCatches = totalCatches,
            TotalWeightKg = (decimal)(double)reader["total_weight_kg"],
            AverageWeightKg = (decimal)(double)reader["avg_weight_kg"],
            TotalPoints = (decimal)(double)reader["total_points"],
            BestPoints = (decimal)(double)reader["best_points"],
            MaxRarity = (int)(long)reader["max_rarity"],
            BestFishName = (string)reader["best_fish_name"],
            RarestFishName = (string)reader["rarest_fish_name"]
        };
    }
}
