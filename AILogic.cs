using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AILogic
{
    public static void Update()
    {
        List<TradeShip> tradeShipsToRemove = new List<TradeShip>();
        List<TradeShip> tradeShips = TradeShips.FindAll();
        foreach (TradeShip tradeShip in tradeShips)
        {
            if(tradeShip.GameShip.isActive) { 

                tradeShip.UpdateShip();
            } else
            {
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
            if (tradeShip.GameShip.isActive)
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
            if (tradeShip.GameShip.isActive)
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
            if (tradeShip.GameShip.isActive)
            {
                tradeShip.UpdateTradeMissions();
            }
        }
    }
}
