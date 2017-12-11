using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AILogic
{
    const int MAX_TRADERS = 3;

    public static Boolean Create(GameShip gameShip)
    {

        TradeShip tradeShip = TradeShips.Find(gameShip);
        if (gameShip.ai != null && gameShip.position != null)
        {
            gameShip.ai.NavigateTo(gameShip.position, 5);
        }
        if (tradeShip == null)
        {
            if (TradeShips.FindAll().Count() < MAX_TRADERS)
            {
                TradeShip newTradeShip = new TradeShip(gameShip);
                TradeChat.Chat("Added new trader " + gameShip.name + " with " + newTradeShip.CargoSlots + " cargo slots");
                TradeShips.Store(newTradeShip);
                    return true;
            }
            else
            {
                TradeChat.Warn("Trader limit of " + MAX_TRADERS + " has been reached.");
            }
        }
        else
        {
            if (tradeShip.OwnerId != TNManager.playerID)
            {
                GamePlayer owner = tradeShip.GetOwner();
                TradeChat.Warn(gameShip.name + " is owned by " + owner.name);
            }
            else
            {
                tradeShip.Fired = true;
                //TradeChat.Chat("You fired trader " + tradeShip.GameShip.name + "!");
                //TradeShips.Remove(tradeShip);
            }
        }
        return false;
    }

    public static void Update()
    {
        List<TradeShip> tradeShipsToRemove = new List<TradeShip>();
        List<TradeShip> tradeShips = TradeShips.FindAll();
        foreach (TradeShip tradeShip in tradeShips)
        {
            if (tradeShip.IsValid())
            {
                tradeShip.UpdateShip();
            }
            else
            {
                int cost = tradeShip.Fire();
                InvalidReason invalidReason = tradeShip.InvalidReason;
                switch (invalidReason)
                {
                    case InvalidReason.NON_ACTIVE:
                        TradeChat.Chat("Removed trader " + tradeShip.GameShip.name + " because of inactivty");
                        break;
                    case InvalidReason.FIRED:
                        TradeChat.Chat("You fired trader " + tradeShip.GameShip.name + "! Cargo lost: " + cost + "g");
                        break;
                    case InvalidReason.STOP_TRADING:
                        TradeChat.Chat("You fired trader " + tradeShip.GameShip.name + " because of /stopTrading command!");
                        break;

                }
                tradeShipsToRemove.Add(tradeShip);
            }
        }
        TradeShips.RemoveAll(tradeShipsToRemove);
    }

    public static void UpdateStatus()
    {
        List<TradeShip> tradeShips = TradeShips.FindAll();
        foreach (TradeShip tradeShip in tradeShips)
        {
            if (tradeShip.IsValid())
            {
                tradeShip.UpdateStatus();
            }
        }
    }

    public static void UpdateNavigation()
    {
        List<TradeShip> tradeShips = TradeShips.FindAll();
        foreach (TradeShip tradeShip in tradeShips)
        {
            if (tradeShip.IsValid())
            {
                tradeShip.UpdateNavigation();
            }
        }
    }

    public static void UpdateTradeMissions()
    {
        List<TradeShip> tradeShips = TradeShips.FindAll();
        foreach (TradeShip tradeShip in tradeShips)
        {
            if (tradeShip.IsValid())
            {
                tradeShip.UpdateTradeMissions();
            }
        }
    }

    internal static void StopTrading()
    {
        List<TradeShip> tradeShips = TradeShips.FindAll();
        foreach (TradeShip tradeShip in tradeShips) {
            if (tradeShip.IsValid())
            {
                tradeShip.StopTrade = true;
            }
        }
    }

    internal static void MapChanged()
    {
        List<TradeShip> tradeShips = TradeShips.FindAll();
        int cargoWorth = tradeShips.Select(s => s.Fire()).Sum();
        tradeShips.ForEach(s => s.ReturnCargo());
        if (cargoWorth > 0)
        {
            TradeChat.Chat("All traders have been removed because of a region change, their cargo has been refunded to you.");
            MyPlayer.ModifyResource("gold", cargoWorth, true);
            MyPlayer.syncNeeded = true;
        }
        TradeShips.RemoveAll();
    }
}
