using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Tasharen;
using System.Collections;

public class TradeAI : MonoBehaviour
{
    public static bool isRunning = false;
    IEnumerator Start()
    {
        if (!isRunning)
        {
            isRunning = true;
            for (; ; )
            {
                if (MyPlayer.ship != null)
                {
                    List<GameShip> followers = DiscoverNewTraders();

                    foreach (GameShip follower in followers)
                    {
                        TradeAIShips.Create(follower);
                        UIStatusBar.Show("Found a new trader");
                        
                    }
                    AILogic.Update();
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }
    }


    private static List<GameShip> DiscoverNewTraders()
    {
        GameShip[] ships = GameWorld.FindObjectsOfType<GameShip>();
        List<GameShip> followers = new List<GameShip>();
        if (ships != null && ships.Count() > 0)
        {
            foreach (GameShip ship in ships)
            {
                if(ship.ai != null && ship.ai.followTarget != null && ship.ai.followTarget.id == MyPlayer.ship.id)
                {
                    followers.Add(ship);
                }
            }
        }
        return followers;
    }
}

