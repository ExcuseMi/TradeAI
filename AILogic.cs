using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AILogic
{
    public static void UpdateSales()
    {
        var myTraders = FindMyTraders();
        foreach (TradeShip tradeShip in FindMyTraders())
        {
            if (tradeShip.IsValid())
            {
                tradeShip.PlayerSell();
            }
        }
    }

    public static void UpdateTradeMissions()
    {
        foreach (TradeShip tradeShip in FindMyNPCs())
        {
            if (tradeShip.IsValid())
            {
                tradeShip.UpdateShip();
                tradeShip.UpdateTradeMissions();
            }
        }
    }

    public static void UpdateNavigation()
    {
        foreach (TradeShip tradeShip in FindMyNPCs())
        {
            if (tradeShip.IsValid())
            {
                tradeShip.UpdateNavigation();
            }
        }
    }
    internal static void StopTrading()
    {
        foreach (TradeShip tradeShip in FindMyTraders()) {
            if (tradeShip.IsValid())
            {
                tradeShip.SetStopTrade(true);
            }
        }
    }

    internal static void UpdateStatus()
    {
        foreach (TradeShip tradeShip in FindMyTraders())
        {
            if (tradeShip.IsValid())
            {
                tradeShip.UpdateStatus();
            }
        }
    }


    internal static void UpdateTownInteraction()
    {
        foreach (TradeShip tradeShip in FindMyNPCs())
        {
            if (tradeShip.IsValid())
            {
                tradeShip.UpdateTownInteraction();
            }
        }
    }

    internal static TradeShip[] FindMyTraders()
    {
        GameShip[] ships = GameWorld.FindObjectsOfType<GameShip>();
        int playerId = TNManager.playerID;
        return ships
            .Where(s => s.player == null && s.factionID != 0)
            .Select(s => new TradeShip(s))
            .Where(t => t.GetTraderOwnerId().HasValue && t.GetTraderOwnerId().Value == playerId).ToArray();
    }

    internal static TradeShip[] FindMyNPCs()
    {
        GameShip[] ships = GameWorld.FindObjectsOfType<GameShip>();
        int playerId = TNManager.playerID;
        return ships
            .Where(s => s.player == null &&  s.ownerID == playerId && s.factionID != 0)
            .Select(s => new TradeShip(s))
            .Where(t => t.GetOwner() != null)
            .ToArray();
    }

    internal static TradeShip[] FindAllNPCs()
    {
        GameShip[] ships = GameWorld.FindObjectsOfType<GameShip>();
        int playerId = TNManager.playerID;
        return ships
            .Where(s => s.player == null && s.factionID != 0)
            .Select(s => new TradeShip(s)).ToArray();
    }

    internal static TradeShip[] FindAllTraders()
    {
        GameShip[] ships = GameWorld.FindObjectsOfType<GameShip>();
        int playerId = TNManager.playerID;
        return ships
            .Where(s => s.player == null && s.factionID != 0)
            .Select(s => new TradeShip(s))
            .Where(t => t.GetOwner() != null).ToArray();
    }

    internal static TradeShip[] FindAllNPCsWherePlayerNearby()
    {
        GameShip[] ships = GameWorld.FindObjectsOfType<GameShip>();
        int playerId = TNManager.playerID;
        return ships
            .Where(s => s.player == null && !s.playerControlled && s.factionID != 0 && s.isPlayerClose)
            .Select(s => new TradeShip(s))
            .Where(t => t.GetOwner() != null)
            .ToArray();
    }
}
