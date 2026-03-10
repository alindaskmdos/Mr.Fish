using Fish.Data;
using Fish.Models;

namespace Fish.Services;

public class FishingService(FishingData data)
{
    public (FishCatch, string) RollFish(ulong userId)
    {
        const double lambda = 0.025;

        double u = Random.Shared.NextDouble();
        double maxExp = Math.Exp(-lambda * 100);
        double roll = -Math.Log(1 - u * (1 - maxExp)) / lambda;
        int result = (int)roll;

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
        }, description);
    }
}