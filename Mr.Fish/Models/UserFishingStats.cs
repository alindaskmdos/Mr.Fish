namespace Fish.Models;

public class UserFishingStats
{
    public int TotalCatches { get; init; }
    public decimal TotalWeightKg { get; init; }
    public decimal AverageWeightKg { get; init; }
    public decimal TotalPoints { get; init; }
    public decimal BestPoints { get; init; }
    public int MaxRarity { get; init; }
    public string BestFishName { get; init; } = "";
    public string RarestFishName { get; init; } = "";
}
