namespace Fish.Models;

public class FishEntry
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal MinWeightKg { get; init; }
    public decimal MaxWeightKg { get; init; }
    public int Rarity { get; init; }
    public int IsSpecial { get; init; }
}