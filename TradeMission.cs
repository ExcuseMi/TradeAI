using System;

public class TradeMission
{
    public GameTown Departure { get; set; }
    public GameTown Destination { get; set; }
    public String ResourceName { get; set; }
    public int SalePrice { get; set; }
    public PlayerItem PlayerItem { get; set; }
    public Boolean Valid { get; set; } = true;
    public Boolean NotEnoughFunds { get; set; }
    public Boolean StockedUp()
    {
        return PlayerItem != null;
    }

    public void Update()
    {
        if (StockedUp())
        {
            if (!Destination.NeedsResource(ResourceName) && !GameTownIsAlly(Destination))
            {
                Valid = false;
            }
        } else if(!Departure.HasResource(ResourceName) && !GameTownIsAlly(Destination))
        {
            Valid = false;
        }
    }

    public int GetProfit()
    {
        if(PlayerItem == null)
        {
            return 0;
        }
        return SalePrice - PlayerItem.gold;
    }

    public void ReturnCargo()
    {
        if(StockedUp())
        {
            Departure.IncreaseProduction(ResourceName);
        }
        Valid = false;
    }

    public override string ToString()
    {
        string translation = Localization.Get(ResourceName);
        return Departure.name + " wants to ship " + translation + " to " + Destination.name;
    }

    public static Boolean GameTownIsAlly(GameTown gameTown)
    {
        return (gameTown.factionID == 0 && MyPlayer.factionID == 0) || (MyPlayer.factionID > 0 && gameTown.factionID > 0);
    }

}
