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

    public TNet.DataNode ToDataNode()
    {
        var parent = new TNet.DataNode("TradeMission");
        parent.AddChild("Departure", Departure);
        parent.AddChild("Destination", Destination);
        parent.AddChild("ResourceName", ResourceName);
        parent.AddChild("PurchasePrice", PurchasePrice);
        parent.AddChild("Valid", Valid);
        parent.AddChild("Completed", Completed);
        return parent;
    }

    public static TradeMission FromDataNode(TNet.DataNode dataNode)
    {
        return new TradeMission()
        {
            Departure = dataNode.GetChild<int>("Departure"),
            Destination = dataNode.GetChild<int>("Destination"),
            ResourceName = dataNode.GetChild<string>("ResourceName"),
            PurchasePrice = dataNode.GetChild<int>("PurchasePrice"),
            Valid = dataNode.GetChild<Boolean>("Valid"),
            Completed = dataNode.GetChild<Boolean>("Completed")
        };
    }

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
