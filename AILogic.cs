using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AILogic
{
    public static bool isRunning = false;

    public IEnumerator Start()
    {
        if (!isRunning)
        {
            isRunning = true;
            for (; ; )
            {

                yield return new WaitForSeconds(2f);
            }
        }
        isRunning = false;
    }


    public static void Update()
    {
        List<TradeAIShip> tradeAIShips = TradeAIShips.FindAll();
        if(tradeAIShips.Count() == 0)
        {
            UIStatusBar.Show("Hire a friendly ship to become your trader!", 5f);

        }
        foreach (TradeAIShip tradeAIShip in tradeAIShips)
        {
            tradeAIShip.Update();

            TradeAIShips.Store(tradeAIShip);

        }
    }


}
