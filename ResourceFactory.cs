using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class ResourceFactory
{
    internal static PlayerItem CreatePlayerItemForResource(GameTown gameTown, string resourceName)
    {
        float multi = GameZone.rewardMultiplier;
        int mPrice = gameTown.GetFinalProductionOffer(resourceName, multi);
        PlayerItem playerItem = new PlayerItem();
        playerItem.name = Localization.Format("Shipment of", Localization.Get(resourceName));
        playerItem.info = Localization.Format("Loaded in", gameTown.name);
        playerItem.gold = mPrice;
        playerItem.SetStat(resourceName, 1);
        playerItem.SetStat("Level", GameZone.challengeLevel);
        playerItem.type = "Shipment";
        return playerItem;
    }


    internal static PlayerItem CreatePlayerItemForResource(GameTown gameTown, string resourceName, int purchasePrice)
    {
        PlayerItem playerItem = new PlayerItem();
        playerItem.name = Localization.Format("Shipment of", Localization.Get(resourceName));
        playerItem.info = Localization.Format("Loaded in", gameTown.name);
        playerItem.gold = purchasePrice;
        playerItem.type = "Shipment";
        playerItem.SetStat(resourceName, 1);
        playerItem.SetStat("Level", GameZone.challengeLevel);
        return playerItem;
    }
}
