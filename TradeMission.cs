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
            if (!Destination.NeedsResource(ResourceName))
            {
                Valid = false;
            }
        } else if(!Departure.HasResource(ResourceName))
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

    public override string ToString()
    {
        string translation = Localization.Get(ResourceName);
        return Departure.name + " wants to ship " + translation + " to " + Destination.name;
    }

}
