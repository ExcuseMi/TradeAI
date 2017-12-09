using System;

public class TradeMission
{
    public GameTown Departure { get; set; }
    public GameTown Destination { get; set; }
    public String ResourceName { get; set; }
    public int salePrice { get; set; }
    public PlayerItem playerItem { get; set; }
    public Boolean valid { get; set; } = true;
    public Boolean stockedUp()
    {
        return playerItem != null;
    }

    public void Update()
    {
        if (stockedUp())
        {
            if (!Destination.NeedsResource(ResourceName))
            {
                valid = false;
            }
        } else if(!Departure.HasResource(ResourceName))
        {
            valid = false;
        }
    }

    public int GetProfit()
    {
        return salePrice - playerItem.gold;
    }

    public override string ToString()
    {
        string translation = Localization.Get(ResourceName);
        return Departure.name + " wants to ship " + translation + " to " + Destination.name;
    }

}
