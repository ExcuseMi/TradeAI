using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Tasharen;
using System.Collections;
using TNet;
using static TNet.GameClient;

public class Trader : MonoBehaviour
{
    public static bool isRunning = false;
    public static bool ENABLED = false;
    private System.Collections.Generic.List<ShipConfiguration> tradeShips = new System.Collections.Generic.List<ShipConfiguration>();

    void Start()
    {
        if (!isRunning)
        {
            isRunning = true;
            UIGameChat.onCommand += ChatInput;
            TNManager.onObjectCreated += UpdateToTradeShip;
            TNManager.client.onPlayerLeft += RemovePlayer;
            StartCoroutine(DoStatusUpdates());
            StartCoroutine(DoUpdateNavigation());
            StartCoroutine(DoUpdateTradeMissions());
        }
    }

    private void RemovePlayer(Player p)
    {
        foreach (TradeShip tradeShip in GameWorld.FindObjectsOfType<TradeShip>().Where(x => x.Owner != null && x.Owner.id == p.id))
        {
            if (tradeShip.gameObject.GetComponent<GameShip>().tno.isMine)
            {
                tradeShip.UpdateTraderOwner(null);
            }
        }
    }

    IEnumerator DoUpdateTradeMissions()
    {
        for (; ; )
        {
            try
            {
                foreach (TradeShip tradeShip in GameWorld.FindObjectsOfType<TradeShip>().Where(x => x.Owner != null && x.Owner.id == MyPlayer.id))
                {
                    tradeShip.UpdateTradeMissions();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            yield return new WaitForSeconds(4f);
        }
    }


    IEnumerator DoUpdateNavigation()
    {
        for (; ; )
        {
            try
            {
                foreach (TradeShip tradeShip in GameWorld.FindObjectsOfType<TradeShip>().Where(x => x.Owner != null && x.Owner.id == MyPlayer.id))
                {
                    tradeShip.UpdateNavigation();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            yield return new WaitForSeconds(1f);

        }
    }

    IEnumerator DoStatusUpdates()
    {
        for (; ; )
        {
            try
            {

                foreach (TradeShip tradeShip in GameWorld.FindObjectsOfType<TradeShip>().Where(x => x.Owner != null && x.Owner.id == MyPlayer.id))
                {
                    tradeShip.UpdateStatus();
                }

            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            yield return new WaitForSeconds(3f);

        }
    }


    private void UpdateToTradeShip(GameObject go)
    {
        var gameMap = FindObjectOfType<UIGameMap>();
        if(gameMap != null)
        {
            gameMap.overrideSprite = null;
        }
        if (GameZone.regionType == GameZone.RegionType.World || GameZone.regionType == GameZone.RegionType.Questing)
        {
            GameShip gameShip = go.GetComponent<GameShip>();

            if (gameShip != null && gameShip.player == null)
            {
                go.AddMissingComponent<TradeShip>().Activate();
                go.AddMissingComponent<TraderClickableScript>();
                //Destroy(GetComponent<UIGameMapIcon>());

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

    public void Create(System.Collections.Generic.List<ShipConfiguration> shipConfigurations)
    {
        if (tradeShips.Count > 0)
        {
            foreach (ShipConfiguration shipConfiguration in shipConfigurations)
            {
                TradeChat.Log("Trying to create " + shipConfiguration.name);
                GameShip.Create(shipConfiguration.name, shipConfiguration.prefab, MyPlayer.factionID, MyPlayer.ship.position, 0f, 0, 1.5f, 0.75f, 0);
            }
            StartCoroutine(FindPlayerShip(shipConfigurations));
        }
    }

    IEnumerator FindPlayerShip(System.Collections.Generic.List<ShipConfiguration> shipConfigurations)
    {
        yield return new WaitForSeconds(1f);

        for (; ; )
        {
            if (MyPlayer.ship != null && MyPlayer.ship.position != null)
            {
                TradeChat.Log("Player ship found");
                StartCoroutine(FindTrader(shipConfigurations));
                StopCoroutine(FindPlayerShip(shipConfigurations));
            }
            yield return new WaitForSeconds(0.5f);
        }

    }

    IEnumerator FindTrader(System.Collections.Generic.List<ShipConfiguration> shipConfigurations)
    {
        int instancesFound = 0;
        for (; ; )
        {
            foreach (ShipConfiguration shipConfiguration in shipConfigurations)
            {
                foreach(GameShip g in GameWorld.FindObjectsOfType<GameShip>())
                {
                    TradeChat.Log(g.name);

                }
                var possibleTrader = GameWorld.FindObjectsOfType<GameShip>().Where(x => x.name.Equals(shipConfiguration.name)).FirstOrDefault();

                if (possibleTrader != null)
                {
                    TradeChat.Log("Possible trader found: " + shipConfiguration.name);

                    TradeShip traderShip = possibleTrader.GetComponent<TradeShip>();
                    if (traderShip == null)
                    {
                        traderShip = possibleTrader.gameObject.AddMissingComponent<TradeShip>();
                        traderShip.Activate();
                    }
                    traderShip.UpdateTraderOwner(MyPlayer.id);
                    traderShip.SetTradeMissions(shipConfiguration.tradeMissions);
                    instancesFound++;
                }
                if (instancesFound == shipConfigurations.Count)
                {
                    StopCoroutine(FindTrader(shipConfigurations));
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }

}

