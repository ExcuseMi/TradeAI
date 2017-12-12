using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNet;

public static class MyTradePlayer
{
    const int MAX_TRADE_SHIPS = 3;
    const string TRADE_SHIPS = "tradeShips";
    const string NAME = "name";

    public static void AddTradeShip(TradeShip tradeShip)
    {
        var dataNodes = MyPlayer.data.GetChild<TNet.List<TNet.DataNode>>(TRADE_SHIPS);
        if( dataNodes == null)
        {
            dataNodes = new TNet.List<TNet.DataNode>();
        }
        dataNodes.Add(tradeShip);
        MyPlayer.syncNeeded = true;
        MyPlayer.Sync();
    }

    public static void RemoveTradeShip(string name)
    {
        var dataNodes = MyPlayer.data.GetChild<TNet.List<TNet.DataNode>>(TRADE_SHIPS);
        if (dataNodes != null)
        {
            DataNode dataNodeToRemove = null;
            foreach (var dataNode in dataNodes)
            {
                if(name.Equals(dataNode.Get<string>(NAME)))
                {
                    dataNodeToRemove = dataNode;
                }
            }
            if (dataNodeToRemove != null)
            {
                dataNodes.Remove(dataNodeToRemove);
                MyPlayer.syncNeeded = true;
                MyPlayer.Sync();
            }
        }
    }

    public static void UpdateTradeShip(TradeShip tradeShip)
    {
        var name = tradeShip.gameShip.name;
        var dataNodes = MyPlayer.data.GetChild<TNet.List<TNet.DataNode>>(TRADE_SHIPS);
        if (dataNodes != null)
        {
            foreach (var dataNode in dataNodes)
            {
                if (name.Equals(dataNode.Get<string>(NAME)))
                {
                    dataNode.SetChild("sailColor0", tradeShip.sailColor0);
                    dataNode.SetChild("sailColor1", tradeShip.sailColor1);
                    dataNode.SetChild("symbolTex", tradeShip.symbolTex);
                    dataNode.SetChild("prefabShipName", tradeShip.gameShip.prefabName);
                    MyPlayer.syncNeeded = true;
                    MyPlayer.Sync();
                    return;
                }
            }
        }
    }

    public static TNet.DataNode ToData(TradeShip tradeShip)
    {
        var dataNode = new TNet.DataNode();
        dataNode.AddChild("name", tradeShip.gameShip.name);
        dataNode.AddChild("sailColor0", tradeShip.sailColor0);
        dataNode.AddChild("sailColor1", tradeShip.sailColor1);
        dataNode.AddChild("symbolTex", tradeShip.symbolTex);

        dataNode.AddChild("prefabShipName", tradeShip.gameShip.prefabName);
        return dataNode;
    }

}
