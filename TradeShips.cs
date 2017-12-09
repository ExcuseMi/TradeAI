using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class TradeShips
{
    //Clean this up so it doesn't store this information forever
    private static Dictionary<int, TradeShip> REGISTRY = new Dictionary<int, TradeShip>();

    public static TradeShip Find(GameShip gameShip)
    {
        if (!REGISTRY.ContainsKey(gameShip.id))
        {
            gameShip.onDeath += Remove;
            return new TradeShip(gameShip);
        }
        else
        {
            return REGISTRY[gameShip.id] ?? new TradeShip(gameShip);
        }
    }


    public static TradeShip Create(GameShip gameShip)
    {
        if (!REGISTRY.ContainsKey(gameShip.id))
        {
            gameShip.name = "<" + TNManager.playerName + ">" + gameShip.name;
            TradeShip tradeShip = new TradeShip(gameShip);
            REGISTRY [gameShip.id] = tradeShip;
        }
        return Find(gameShip);
    }

    public static List<TradeShip> FindAll()
    {
        return REGISTRY.Values.ToList();
    }

    public static void Purge()
    {


    }


    public static void Store(TradeShip tradeShip)
    {
        REGISTRY[tradeShip.id] = tradeShip;
    }

    internal static void Remove(GameShip tradeShip)
    {
        if(REGISTRY.ContainsKey(tradeShip.id))
        {
            REGISTRY.Remove(tradeShip.id);
        }
    }

    public static void Remove(TradeShip tradeShip)
    {
        if (REGISTRY.ContainsKey(tradeShip.id))
        {
            REGISTRY.Remove(tradeShip.id);
        }
    }

    public static void RemoveAll(List<TradeShip> tradeShips)
    {
        tradeShips.ForEach(x=>Remove(x));
    }
}
