using System;

public class TradeMission
{
    public GameTown Departure { get; set; }
    public GameTown Destination { get; set; }
    public String ResourceName { get; set; }
    public int intakePrice { get; set; }
    public int salePrice { get; set; }
    public PlayerItem playerItem { get; set; }

    public Boolean stockedUp()
    {
        return playerItem != null;
    }
    public override string ToString()
    {
        string translation = Localization.Get(ResourceName);
        return Departure.name + " wants to ship " + translation + " to " + Destination.name;
    }

}
