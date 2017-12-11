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
    public static bool ENABLED = false;

    IEnumerator Start()
    {
        if (!isRunning)
        {
            isRunning = true;
            UIGameChat.onCommand += ChatInput;
            GameZone.onDataChanged += OnGameZoneChanged;
            //TNManager.client.OnJoinChannel += OnJoinChannel;
            //StartCoroutine(ToggleOnCombatCoRoutine());
            EnableMod();
        }
        yield return new WaitForEndOfFrame();
    }

    private void OnGameZoneChanged(TNet.DataNode data)
    {
        ToggleOnCombatInstance();
    }

    private void ToggleOnCombatInstance()
    {
        switch(GameZone.regionType)
        {
            case GameZone.RegionType.Duel:
            case GameZone.RegionType.Combat:
            case GameZone.RegionType.Heroic:
            case GameZone.RegionType.Battle:
            case GameZone.RegionType.Raid:
                DisableMod();
                break;
            default:
                EnableMod();
                break;
        }
    }
    private void DisableMod()
    {
        if (ENABLED)
        {
            TradeChat.Warn("All trading has been suspended until the peace returns in the zone.");
            StopCoroutine(DoStatusUpdates());
            StopCoroutine(UpdateNavigation());
            StopCoroutine(UpdateTradeMissions());
            StopCoroutine(UpdateSales());
            StartCoroutine(DiscoverTraders());
            ENABLED = false;
        }
    }

    private void EnableMod()
    {
        if (!ENABLED)
        {
            TradeChat.Warn("Trading is back on!");
            StartCoroutine(DoStatusUpdates());
            StartCoroutine(UpdateNavigation());
            StartCoroutine(UpdateTradeMissions());
            StartCoroutine(UpdateSales());
            StartCoroutine(DiscoverTraders());
            ENABLED = true;
        }
    }

    IEnumerator DiscoverTraders()
    {
        for (; ; )
        {
            try
            {
                DiscoverNewTraders();

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator UpdateTradeMissions()
    {
        for (; ; )
        {
            try
            {
                AILogic.UpdateTradeMissions();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            yield return new WaitForSeconds(5f);
        }
    }


    IEnumerator UpdateNavigation()
    {
        for (; ; )
        {
            try
            {
                AILogic.UpdateNavigation();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator DoStatusUpdates()
    {
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
                yield return new WaitForSeconds(4f);
            }
            else
            {
                yield return new WaitForSeconds(5f);
            }
        }
    }

    IEnumerator UpdateSales()
    {
        for (; ; )
        {
            if (MyPlayer.ship != null)
            {
                try
                {
                    AILogic.UpdateSales();
                }
                catch (Exception e)
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


    private static void DiscoverNewTraders()
    {
        GameShip[] ships = GameWorld.FindObjectsOfType<GameShip>();
        List<GameShip> followers = new List<GameShip>();
        if (ships != null && ships.Count() > 0)
        {
            foreach (GameShip ship in ships)
            {
                if(ship.ai != null && ship.ai.followTarget != null && ship.ai.followTarget.id == MyPlayer.ship.id)
                {
                    int? currentTraderOwner = ship.Get<int?>(TradeShip.TRADER_OWNER);
                    if (currentTraderOwner.HasValue && currentTraderOwner.Value == TNManager.playerID)
                    {
                        TradeChat.Chat("You fired " + ship.name + "!");
                        ship.Set(TradeShip.TRADER_OWNER, null);
                    }
                    else
                    {
                        if(currentTraderOwner.HasValue)
                        {
                            var player = GamePlayer.Find(currentTraderOwner.Value);
                            if(player != null)
                            {
                                TradeChat.Chat("You stole trader " + ship.name + " from " + player.name + "!");

                            } else
                            {
                                TradeChat.Chat("You hired " + ship.name + " as a trader!");
                            }
                        } else
                        {
                            TradeChat.Chat("You hired " + ship.name + " as a trader!");

                        }
                        ship.Set(TradeShip.TRADER_OWNER, TNManager.playerID);
                    }
                    ship.ai.NavigateTo(ship.position);
                }
            }
        };
    }

    private void ChatInput(string msg, ref bool handled)
    {
        handled = false;
    }

}

