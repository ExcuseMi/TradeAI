using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Tasharen;
using System.Collections;

public class TradeAI : MonoBehaviour
{
    private int count = 0;
       
    void Start()
    {
        UIGameChat.onCommand += ChatInput;
    }

    void LateUpdate()
    {

    }

    private void ChatInput(string msg, ref bool handled)
    {
        switch (msg)
        {
            case "spawnTrader": SpawnTrader(); handled = true; break;
            case "cheathelp": CheatsList(); handled = true; break;
        }
    }

    private void SpawnTrader()
    {
        count++;
        string name = "<" + TNManager.playerName + ">Trader";
        GameShip.CreateRandom(name, MyPlayer.ship.position, 1f, MyPlayer.factionID);
        foreach (GameShip ship in GameWorld.FindObjectsOfType<GameShip>())
        {
            if(!ship.playerControlled && ship.name.Equals(name))
            {
                //ship.ai.ownerID = TNManager.playerID;
            }
        }
        UIStatusBar.Show("count " + count, 5f);

    }

    private void CheatsList()
    {
  
        UIGameChat.AddCurrent("spawntrader --- Spawns trader", Color.yellow);
    }


}

