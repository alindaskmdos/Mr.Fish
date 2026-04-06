using Fish.Data;
using Fish.Models;

namespace Fish.Services;

public class EconomyService(FishingData data)
{
    private const string BuyRodPrefix = "shop_buy";
    private const string ShopPagePrefix = "shop_page";

    public IReadOnlyList<RodOffer> GetAllRods()
    {
        return data.Rods;
    }

    public RodOffer GetDefaultRod()
    {
        return data.Rods[0];
    }

    public RodOffer? GetRodByTier(int tier)
    {
        return data.Rods.FirstOrDefault(r => r.Tier == tier);
    }

    public RodOffer GetCurrentRodOrDefault(int tier)
    {
        return GetRodByTier(tier) ?? GetDefaultRod();
    }

    public int CalculateScalesReward(decimal points)
    {
        var reward = (int)Math.Floor(points / 10m);
        return Math.Max(1, reward);
    }

    public string BuildBuyRodCustomId(int tier, int page)
    {
        return $"{BuyRodPrefix}:{tier}:{page}";
    }

    public bool TryParseBuyRodCustomId(string customId, out int tier, out int page)
    {
        tier = 0;
        page = 0;

        var parts = customId.Split(':');
        if (parts.Length != 3)
            return false;

        if (!string.Equals(parts[0], BuyRodPrefix, StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[1], out tier))
            return false;

        if (!int.TryParse(parts[2], out page))
            return false;

        return true;
    }

    public string BuildShopPageCustomId(int page)
    {
        return $"{ShopPagePrefix}:{page}";
    }

    public bool TryParseShopPageCustomId(string customId, out int page)
    {
        page = 0;

        var parts = customId.Split(':');
        if (parts.Length != 2)
            return false;

        if (!string.Equals(parts[0], ShopPagePrefix, StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[1], out page))
            return false;

        return true;
    }
}
