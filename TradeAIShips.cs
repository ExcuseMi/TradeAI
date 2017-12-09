using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class TradeAIShips
{
    //Clean this up so it doesn't store this information forever
    private static Dictionary<int, TradeAIShip> REGISTRY = new Dictionary<int, TradeAIShip>();

    public static TradeAIShip Find(GameShip gameShip)
    {
        if (!REGISTRY.ContainsKey(gameShip.id))
        {
            gameShip.onDeath += Remove;
            return new TradeAIShip(gameShip);
        }
        else
        {
            return REGISTRY[gameShip.id] ?? new TradeAIShip(gameShip);
        }
    }


    public static TradeAIShip Create(GameShip gameShip)
    {
        if (!REGISTRY.ContainsKey(gameShip.id))
        {
            gameShip.name = "<" + TNManager.playerName + ">" + gameShip.name;
            TradeAIShip tradeAIShip = new TradeAIShip(gameShip);
            REGISTRY [gameShip.id] = tradeAIShip;
        }
        return Find(gameShip);
    }

    public static List<TradeAIShip> FindAll()
    {
        return REGISTRY.Values.ToList();
    }

    public static void Purge()
    {


    }


    public static void Store(TradeAIShip tradeAIShip)
    {
        REGISTRY[tradeAIShip.gameShip.id] = tradeAIShip;
    }

    internal static void Remove(GameShip gameShip)
    {
        if(REGISTRY.ContainsKey(gameShip.id))
        {
            REGISTRY.Remove(gameShip.id);
        }
    }
}
