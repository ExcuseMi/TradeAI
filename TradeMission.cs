using System;

public class TradeMission
{
    public GameTown Departure { get; set; }
    public GameTown Destination { get; set; }
    public String ResourceName { get; set; }
    public Boolean stockedUp { get; set; } = false;
    public int intakePrice { get; set; }
    public int salePrice { get; set; }
    public PlayerItem playerItem { get; set; }

    public override string ToString()
    {
        string translation = Localization.Get(ResourceName);
        return Departure.name + " wants to ship " + translation + " to " + Destination.name;
    }

    public PlayerItem createPlayerItem()
    {
        if (playerItem == null) {
            float multi = GameConfig.GetRewardMultiplier(MyPlayer.level, true);
            int mPrice = Departure.GetFinalProductionOffer(ResourceName, multi);
            PlayerItem playerItem = new PlayerItem();
            playerItem.name = Localization.Format("Shipment of", Localization.Get(ResourceName));
            playerItem.info = Localization.Format("Loaded in", Departure.name);
            playerItem.gold = mPrice;
            playerItem.type = "Shipment";
            playerItem.SetStat(ResourceName, 1);
            playerItem.SetStat("Level", GameZone.challengeLevel);
            this.playerItem = playerItem;
        }
        return this.playerItem;
    }
}
