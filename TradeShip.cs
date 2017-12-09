using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeShip
{
    public GameShip gameShip { get; set; }
    public int id { get; set; }
    public List<TradeMission> tradeMissions { get; set; }
    const float RANGE = 8f;

    public TradeShip(GameShip gameShip)
    {
        this.gameShip = gameShip;
        this.id = gameShip.id;
        this.tradeMissions = new List<TradeMission>();
    }

    public Boolean HasResource(string resource)
    {
        return tradeMissions.Where(x => x.stockedUp() && x.ResourceName.Equals(resource)).Any();
    }

    public void Update()
    {
        if (gameShip.isActive)
        {
            UpdateTradeMissions();
            TradeMission currentTradeMission = tradeMissions.FirstOrDefault();
            if (currentTradeMission != null)
            {

                Boolean selling = currentTradeMission.stockedUp() ? true : false;
                GameTown currentDestinationTown = selling ? currentTradeMission.Destination : currentTradeMission.Departure;
                if (selling && InRange(currentTradeMission.Destination))
                {
                    Sell(currentTradeMission.Destination);
                }
                else if (!selling && InRange(currentTradeMission.Departure))
                {
                    Buy(currentTradeMission.Departure);
                }
                else
                {
                    AIShip aiShip = gameShip.GetComponent<AIShip>();
                    if (aiShip != null)
                    {
                        aiShip.NavigateTo(currentDestinationTown.dockPos, 2);
                        if (selling)
                        {
                            gameShip.AddHudText("Delivering " + Localization.Get(currentTradeMission.ResourceName) + " to " + currentDestinationTown.name, Color.gray, 1f);
                        } else
                        {
                            gameShip.AddHudText("Buying " + Localization.Get(currentTradeMission.ResourceName) + " from " + currentDestinationTown.name, Color.gray, 1f);

                        }
                    }
                }
            }
        }
    }

    private void UpdateTradeMissions()
    {
        tradeMissions.ForEach(x => x.Update());
        tradeMissions.RemoveAll(x => !x.valid); 
        if (tradeMissions.Count() == 0)
        {
            TradeMission newTradeMission = TradeMissionCalculator.FindATradeMission(gameShip.position);
            if (newTradeMission != null)
            {
                tradeMissions.Add(newTradeMission);
            } else
            {
                gameShip.AddHudText("Can't find trade missions", Color.red, 1f);
            }
        }
    }

    private Boolean InRange(GameTown gameTown)
    {
        float range = Math.Max(Math.Max(
            Math.Abs(gameTown.dockPos.x - gameShip.position.x),
            Math.Abs(gameTown.dockPos.y - gameShip.position.y)), 
            Math.Abs(gameTown.dockPos.z - gameShip.position.z));
        return range <= RANGE; ;
    }

    private void Sell(GameTown gameTown)
    {
        List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();
        List<TradeMission> tradeMissionsToAdd = new List<TradeMission>();
        foreach (TradeMission tradeMission in tradeMissions)
        {
            if(tradeMission.Destination.id.Equals(gameTown.id))
            {
                int salePrice = gameTown.Sell(tradeMission.playerItem);
                if (salePrice != 0)
                {

                    tradeMission.salePrice = salePrice;
                    UIStatusBar.Show(gameShip.name + "@" + gameTown.name +  ": " + "Sold item: " + Localization.Get(tradeMission.ResourceName) + ". Profit: " + GameTools.FormatGold(tradeMission.GetProfit(), true, true), 10f);

                    tradeMissionsToRemove.Add(tradeMission);
                } else
                {
                    TradeMission newTradeMission = TradeMissionCalculator.FindATradeMissionForResource(tradeMission.ResourceName, tradeMission.Departure, gameShip.position);
                    if (newTradeMission != null)
                    {
                        newTradeMission.playerItem = tradeMission.playerItem;
                        tradeMissionsToAdd.Add(newTradeMission);
                        tradeMissionsToRemove.Add(tradeMission);
                    }
                    else
                    {
                        UIStatusBar.Show("No towns in this region allow me to sell " + tradeMission.playerItem.name + ". Cargo destroyed, cost: " + GameTools.FormatGold(tradeMission.playerItem.gold, true, true), 5f);
                        tradeMissionsToRemove.Add(tradeMission);
                    }
                }
            }
        }
        RemoveTradeMissions(tradeMissionsToRemove);
        tradeMissions.AddRange(tradeMissionsToAdd);
    }

    private void RemoveTradeMissions(List<TradeMission> tradeMissionsToRemove)
    {
        foreach (TradeMission tradeMissionToRemove in tradeMissionsToRemove)
        {
            tradeMissions.Remove(tradeMissionToRemove);
        }
    }

    private void Buy(GameTown gameTown)
    {
        List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();

        foreach (TradeMission tradeMission in tradeMissions)
        {
            if (tradeMission.Departure.id.Equals(gameTown.id))
            {
                if (tradeMission.Departure.HasResource(tradeMission.ResourceName))
                {
                    PlayerItem playerItem = createPlayerItemForResource(gameTown, tradeMission.ResourceName);
                    if (!MyPlayer.Buy(playerItem.gold))
                    {
                        return;
                    }
                    tradeMission.Departure.ReduceProduction(tradeMission.ResourceName);
                    tradeMission.playerItem = playerItem;
                    //UIStatusBar.Show(gameShip.name + "@" + gameTown.name + ": " + Localization.Format("Bought Item", playerItem.name, GameTools.FormatGold(playerItem.gold, true, true)), 10f);
                    if (GameAudio.instance != null)
                        NGUITools.PlaySound(GameAudio.instance.buy);
                    MyPlayer.saveNeeded = true;
                    MyPlayer.Sync();
                } else
                {
                    tradeMissionsToRemove.Add(tradeMission);
                }
            }
        }
        RemoveTradeMissions(tradeMissionsToRemove);
    }

    private static PlayerItem createPlayerItemForResource(GameTown gameTown, string resourceName)
    {
        float multi = GameConfig.GetRewardMultiplier(MyPlayer.level, true);
        int mPrice = gameTown.GetFinalProductionOffer(resourceName, multi);
        PlayerItem playerItem = new PlayerItem();
        playerItem.name = Localization.Format("Shipment of", Localization.Get(resourceName));
        playerItem.info = Localization.Format("Loaded in", gameTown.name);
        playerItem.gold = mPrice;
        playerItem.type = "Shipment";
        playerItem.SetStat(resourceName, 1);
        playerItem.SetStat("Level", GameZone.challengeLevel);
        return playerItem;
    }
}
