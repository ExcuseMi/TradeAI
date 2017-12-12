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
            StartCoroutine(DoDiscoverTraders());
            EnableMod();
        }
        yield return new WaitForEndOfFrame();
    }

    IEnumerator DoDiscoverTraders()
    {
        for (; ; )
        {
            if(TNManager.isHosting) { 
                DiscoverNewTraders();
            }
            yield return new WaitForSeconds(5f);
        }
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
            StartCoroutine(DoDiscoverTraders());
            ENABLED = false;
        }
    }

    private void EnableMod()
    {
        if (!ENABLED)
        {
            TradeChat.Warn("Trading is back on!");
            StartCoroutine(DoDiscoverTraders());
            ENABLED = true;
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
                    var tradeShip = FindTradeShip(ship.id);
                    if(tradeShip == null)
                    {
                        ship.gameObject.AddMissingComponent<TradeShip>().Activate(TNManager.playerID, ship.sailColor0, ship.sailColor1, ship.symbolTex);
                        TradeChat.Chat("You hired " + ship.name + " as a trader!");

                    }
                    else if (tradeShip.GetTraderOwnerId() == TNManager.playerID)
                    {
                        //ztradeShip.Dettach();

                        TradeChat.Chat("You fired " + ship.name + "!");
                    }
                    else
                    {
                        var player = GamePlayer.Find(tradeShip.GetTraderOwnerId());
                        if(player != null)
                        {
                            tradeShip.UpdateTradeOwner(TNManager.playerID);
                            TradeChat.Chat("You stole trader " + ship.name + " from " + player.name + "!");

                        }
                    }
                    ship.ai.NavigateTo(ship.position);
                }
            }
        };
    }

    public static TradeShip FindTradeShip(int id)
    {
        return GameWorld.FindObjectsOfType<TradeShip>().Where(x => x.GameShip.id == id).FirstOrDefault(); ;
    }

    private void ChatInput(string msg, ref bool handled)
    {
        handled = false;
    }

}

