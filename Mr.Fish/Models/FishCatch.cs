namespace Fish.Models;

public class FishCatch
{
    public ulong UserId { get; init; }
    public string FishName { get; init; } = "";
    public string AdjectiveName { get; init; } = "";
    public decimal WeightKg { get; init; }
    public decimal Points { get; init; }
    public int Rarity { get; init; }
    public bool IsSpecial { get; init; }
    public DateTime CaughtAt { get; init; } = DateTime.UtcNow;
}
