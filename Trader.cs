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
            TNManager.onObjectCreated += UpdateToTradeShip;

            for (; ; )
            {
                if (MyPlayer.ship != null)
                {
                    if (MyPlayer.ownedShip.gameObject.GetComponent<QuestHireTrader>() == null)
                    {
                        //MyPlayer.ownedShip.gameObject.AddMissingComponent<QuestHireTrader>();
                    }
                    MyPlayer.ship.speedLimiter = 5f;
                }
                yield return new WaitForSeconds(1);

            }
        }
        yield return new WaitForEndOfFrame();
    }

    private void UpdateToTradeShip(GameObject go)
    {
        GameShip gameShip = go.GetComponent<GameShip>();

        if (gameShip != null)
        {
            if (gameShip.tno.isMine && gameShip.factionID != 0 && gameShip.player == null)
            {
               go.AddMissingComponent<TradeShip>().Activate();
            }

        }
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

