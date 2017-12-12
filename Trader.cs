using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Tasharen;
using System.Collections;

public class Trader : MonoBehaviour
{
    public static bool isRunning = false;
    public static bool ENABLED = false;

    IEnumerator Start()
    {
        if (!isRunning)
        {
            isRunning = true;
            UIGameChat.onCommand += ChatInput;
            
            for (; ; )
            {
                if (MyPlayer.ship != null)
                {
                    MyPlayer.ship.speedLimiter = 5f;

                }
                yield return new WaitForSeconds(5);

            }
        }
        yield return new WaitForEndOfFrame();
    }


    public static TradeShip FindTradeShip(int id)
    {
        return GameWorld.FindObjectsOfType<TradeShip>().Where(x => x.gameShip.id == id).FirstOrDefault(); ;
    }

    private void ChatInput(string msg, ref bool handled)
    {
        handled = false;
    }

}

