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
            return null;
        }
        else
        {
            return REGISTRY[gameShip.id] ?? new TradeShip(gameShip);
        }
    }


    public static void Create(GameShip gameShip)
    {
        if (gameShip.ai != null && gameShip.position != null)
        {
            gameShip.ai.NavigateTo(gameShip.position);
        }
        if (!REGISTRY.ContainsKey(gameShip.id))
        {
            TradeShip tradeShip = new TradeShip(gameShip);
            TradeChat.Chat("Added new trader " + gameShip.name + " with " + tradeShip.CargoSlots + " cargo slots");
            REGISTRY [gameShip.id] = tradeShip;
        } else
        {
            TradeShip tradeShip = REGISTRY[gameShip.id];
            GamePlayer owner = tradeShip.GetOwner();
            TradeChat.Warn(gameShip.name + " is owned by " + owner.name);
        }
    }

    public static List<TradeShip> FindAll()
    {
        return REGISTRY.Values.ToList();
    }

    public static void Store(TradeShip tradeShip)
    {
        REGISTRY[tradeShip.Id] = tradeShip;
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
        if (REGISTRY.ContainsKey(tradeShip.Id))
        {
            REGISTRY.Remove(tradeShip.Id);
        }
    }

    public static void RemoveAll(List<TradeShip> tradeShips)
    {
        tradeShips.ForEach(x=>Remove(x));
    }
}
