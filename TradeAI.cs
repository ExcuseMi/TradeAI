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
            StartCoroutine(DoStatusUpdates());
            StartCoroutine(DoNavigation());
            StartCoroutine(UpdateTradeMissions());

            isRunning = true;
            for (; ; )
            {
                    if (MyPlayer.ship != null)
                    {
                        try
                        {
                            List<GameShip> followers = DiscoverNewTraders();

                            foreach (GameShip follower in followers)
                            {
                                TradeShips.Create(follower);
                            }
                            AILogic.Update();
                        } catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                        yield return new WaitForSeconds(1f);
                }
                    else
                    {
                        yield return new WaitForSeconds(5f);
                    }

            }
        }
    }

    IEnumerator DoNavigation()
    {
        for (; ; )
        {
            if (MyPlayer.ship != null)
            {
                try
                {
                    AILogic.UpdateNavigation();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    IEnumerator UpdateTradeMissions()
    {
        for (; ; )
        {
            if (MyPlayer.ship != null)
            {
                try
                {
                    AILogic.UpdateTradeMissions();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
                yield return new WaitForSeconds(2f);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
    IEnumerator DoStatusUpdates()
    {
        if (!isRunning)
        {
            isRunning = true;
            for (; ; )
            {
                if (MyPlayer.ship != null)
                {
                    try
                    {
                        AILogic.UpdateStatus();
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                    yield return new WaitForSeconds(3f);
                }
                else
                {
                    yield return new WaitForSeconds(5f);
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

