using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeAIShip
{
    public GameShip gameShip { get; set; }
    public List<TradeMission> tradeMissions { get; set; }
    const float RANGE = 1f;

    public TradeAIShip(GameShip gameShip)
    {
        this.gameShip = gameShip;
        this.tradeMissions = new List<TradeMission>();
    }

    public Boolean HasResource(string resource)
    {
        return tradeMissions.Where(x => x.stockedUp && x.ResourceName.Equals(resource)).Any();
    }

    public void Update()
    {
        if (gameShip.isActive)
        {
            UpdateTradePaths();
            TradeMission currentTradeMission = tradeMissions.FirstOrDefault();
            if (currentTradeMission != null)
            {
                Boolean selling = HasResource(currentTradeMission.ResourceName) ? true : false;
                GameTown currentDestinationTown = selling ? currentTradeMission.Destination : currentTradeMission.Departure;
                if (selling && InRange(currentTradeMission.Destination, gameShip))
                {
                    Sell(currentTradeMission.Destination);
                }
                else if (!selling && InRange(currentTradeMission.Departure, gameShip))
                {
                    Buy(currentTradeMission.Departure);
                }
                else
                {
                    AIShip aiShip = gameShip.GetComponent<AIShip>();
                    if (aiShip != null)
                    {
                        aiShip.NavigateTo(currentDestinationTown.position, 2);
                    }
                }
            }
        }
    }

    private void UpdateTradePaths()
    {
        if (tradeMissions.Count() == 0)
        {
            TradeMission newTradePath = TradeMissionCalculator.FindATradeMission();
            if (newTradePath != null)
            {
                tradeMissions.Add(newTradePath);
                gameShip.AddHudText(newTradePath.ToString(), Color.green, 5f, false);
            }
        }
    }

    private Boolean InRange(GameTown gameTown, GameShip gameShip)
    {
        Boolean inProximity = gameTown.InProximityTo(gameShip);
        return inProximity;
    }

    private void Sell(GameTown gameTown)
    {
        gameShip.AddHudText("Trying to sell", Color.green, 2f);
        List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();
        foreach (TradeMission tradeMission in tradeMissions)
        {
            if(tradeMission.Destination.id.Equals(gameTown.id))
            {
                int salePrice = gameTown.Sell(tradeMission.playerItem);
                if (salePrice != 0)
                {

                    tradeMission.salePrice = salePrice;
                    UIStatusBar.Show(Localization.Format("Sold Item", tradeMission.playerItem.name, GameTools.FormatGold(salePrice, true, true)), 20f);

                    tradeMissionsToRemove.Add(tradeMission);
                } else
                {
                    TradeMission newTradeMission = TradeMissionCalculator.FindATradeMissionForResource(tradeMission.ResourceName);
                    if (newTradeMission != null)
                    {
                        newTradeMission.playerItem = tradeMission.playerItem;
                        newTradeMission.stockedUp = true;
                        ReplaceTradeMission(tradeMission, newTradeMission);
                    }
                    else
                    {
                        UIStatusBar.Show("This town doesn't let me sell my stuff! Destroying cargo worth " + GameTools.FormatGold(tradeMission.playerItem.gold, true, true), 5f);
                        tradeMissionsToRemove.Add(tradeMission);
                    }
                }
            }
        }
        RemoveTradeMissions(tradeMissionsToRemove);
    }

    private void RemoveTradeMissions(List<TradeMission> tradeMissionsToRemove)
    {
        foreach (TradeMission tradeMissionToRemove in tradeMissionsToRemove)
        {
            tradeMissions.Remove(tradeMissionToRemove);
        }
    }

    private void ReplaceTradeMission(TradeMission tradeMissionToRemove, TradeMission tradeMissionToAdd)
    {
        tradeMissions.Remove(tradeMissionToRemove);
        tradeMissions.Add(tradeMissionToAdd);
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
                    PlayerItem playerItem = tradeMission.createPlayerItem();
                    if (!MyPlayer.Buy(playerItem.gold))
                    {
                        return;
                    }
                    tradeMission.Departure.ReduceProduction(tradeMission.ResourceName);
                    UIStatusBar.Show(Localization.Format("Bought Item", playerItem.name, GameTools.FormatGold(playerItem.gold, true, true)), 20f);
                    tradeMission.stockedUp = true;
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
}
