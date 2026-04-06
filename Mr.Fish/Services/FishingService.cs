using Fish.Data;
using Fish.Models;

namespace Fish.Services;

public class FishingService(FishingData data)
{
    public FishEntry GetFishOfTheDay(DateTime? now = null)
    {
        var currentDate = (now ?? DateTime.UtcNow).Date;
        int seed = currentDate.Year * 10000 + currentDate.Month * 100 + currentDate.Day;
        var random = new Random(seed);
        int index = random.Next(data.Fish.Length);

        return data.Fish[index];
    }

    public (FishCatch, string, bool) RollFish(ulong userId, int luckBonus = 0)
    {
        const double lambda = 0.025;

        double u = Random.Shared.NextDouble();
        double maxExp = Math.Exp(-lambda * 100);
        double roll = -Math.Log(1 - u * (1 - maxExp)) / lambda;

        int safeLuckBonus = Math.Clamp(luckBonus, 0, 25);
        int result = (int)roll + safeLuckBonus;
        result = Math.Clamp(result, 0, 100);

        var minDiff = data.Fish
                        .Where(f => result >= f.Rarity)
                        .Select(f => result - f.Rarity)
                        .DefaultIfEmpty(int.MaxValue)
                        .Min();

        var candidates = data.Fish
                        .Where(f => result >= f.Rarity && (result - f.Rarity) == minDiff)
                        .ToList();

        if (candidates.Count == 0)
            candidates = data.Fish.OrderBy(f => f.Rarity).Take(1).ToList();

        var fish = candidates[Random.Shared.Next(candidates.Count)];
        var description = fish.Description;
        var adjective = data.Adjectives[Random.Shared.Next(0, data.Adjectives.Length)];
        var weight = data.GetWeight(fish);
        var isSpecial = Random.Shared.Next(2) == 1;
        var points = data.GetPoints(fish, adjective, weight);
        if (isSpecial) points = (int)(points * 1.5m);

        var fishOfTheDay = GetFishOfTheDay();
        bool isFishOfTheDay = string.Equals(fish.Name, fishOfTheDay.Name, StringComparison.Ordinal);
        if (isFishOfTheDay) points *= 1.5m;

        return (new FishCatch
        {
            UserId = userId,
            FishName = fish.Name,
            AdjectiveName = adjective.Name,
            WeightKg = weight,
            Points = points,
            Rarity = fish.Rarity,
            IsSpecial = isSpecial,
            CaughtAt = DateTime.UtcNow
        }, description, isFishOfTheDay);
    }
}