using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AILogic
{
    public static bool isRunning = false;


    public static void Update()
    {
        List<TradeShip> tradeShips = TradeShips.FindAll();
        if(tradeShips.Count() == 0)
        {
            UIStatusBar.Show("Hire a friendly ship to become your trader!", 5f);

        }
        MyPlayer.ship.scaleSpeed = 10f;

        List<TradeShip> tradeShipsToRemove = new List<TradeShip>();
        foreach (TradeShip tradeShip in tradeShips)
        {
            if(tradeShip.gameShip.isActive) { 

                tradeShip.gameShip.scaleSpeed = 5f;
                tradeShip.Update();
                TradeShips.Store(tradeShip);
            } else
            {
                tradeShipsToRemove.Add(tradeShip);
            }
        }
        TradeShips.RemoveAll(tradeShipsToRemove);
    }


}
