using System;
using TNet;

public class TradeMission
{
    public int Departure { get; set; }
    public int Destination { get; set; }
    public String ResourceName { get; set; }
    public int PurchasePrice { get; set; } = -1;
    public Boolean Valid { get; set; } = true;
    public Boolean Completed { get; set; } = false;

    public Boolean StockedUp()
    {
        return PurchasePrice != -1;
    }

    public GameTown GetDestination()
    {
        return FindGameTown(Destination);
    }
    public GameTown GetDeparture()
    {
        return FindGameTown(Departure);
    }
    private GameTown FindGameTown(int id)
    {
        return GameTown.Find(id);
    }
}
