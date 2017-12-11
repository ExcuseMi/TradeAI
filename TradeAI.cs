﻿using System;
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
            UIGameChat.onCommand += ChatInput;
            GameWorld.onMapChanged += OnMapChanged;
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
                                AILogic.Create(follower);
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

    private void OnMapChanged()
    {
        AILogic.MapChanged();
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
                yield return new WaitForSeconds(1f);
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
                    yield return new WaitForSeconds(5f);
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

    private void ChatInput(string msg, ref bool handled)
    {
        switch (msg)
        {
            case "stopTrading": StopTrading(); handled = true; break;

        }

    }

    private void StopTrading()
    {
        AILogic.StopTrading();
    }
}

