using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeShip
{
    public GameShip GameShip { get; set; }
    public int Id { get; set; }
    public List<TradeMission> tradeMissions { get; set; }
    const float RANGE = 8f;
    const int DEFAULT_CARGO_SLOTS = 2;
    public int OwnerId { get; set; }
    private Boolean RequestTradeMisison = true;
    public int CargoSlots { get; set; }

    public TradeShip(GameShip gameShip)
    {
        this.GameShip = gameShip;
        this.Id = gameShip.id;
        this.tradeMissions = new List<TradeMission>();
        this.OwnerId = TNManager.playerID;
        this.CargoSlots = GetCargoSlots(GameShip.unitType);
    }

    public Boolean HasResource(string resource)
    {
        return tradeMissions.Where(x => x.StockedUp() && x.ResourceName.Equals(resource)).Any();
    }

    public void UpdateNavigation()
    {
        if (GameShip != null && GameShip.position != null && GameShip.isActive)
        {
            TradeMission currentTradeMission = tradeMissions.FirstOrDefault();

            if (currentTradeMission != null)
            {
                Boolean selling = currentTradeMission.StockedUp();
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
                    AIShip aiShip = GameShip.GetComponent<AIShip>();
                    if (aiShip != null)
                    {
                        aiShip.NavigateTo(currentDestinationTown.dockPos, 2);
                    }
                }
            }
        }
    }

    public void UpdateStatus()
    {
        if(GameShip != null) { 
            TradeMission currentTradeMission = tradeMissions.FirstOrDefault();

            if (currentTradeMission != null)
            {
                Boolean selling = currentTradeMission.StockedUp();
                GameTown currentDestinationTown = selling ? currentTradeMission.Destination : currentTradeMission.Departure;
                AIShip aiShip = GameShip.GetComponent<AIShip>();
                if (aiShip != null)
                {
                    if (selling)
                    {
                        string[] resources = tradeMissions.Where(m => m.Destination == currentTradeMission.Destination && m.StockedUp())
                            .Select(m => Localization.Get(m.ResourceName))
                            .GroupBy(x => x)
                            .Select(g => g.Key + (g.Count() > 1 ? " (x" + g.Count() + ")": "")).ToArray();
                        string resourcesCS = string.Join(", ", resources);
                        GameShip.AddHudText("Delivering " + resourcesCS + " to " + currentDestinationTown.name, Color.gray, 1f);
                    }
                    else
                    {
                        string[] resources = tradeMissions.Where(m => m.Departure == currentTradeMission.Departure && !m.StockedUp()).Select(m => Localization.Get(m.ResourceName))
                            .GroupBy(x => x)
                            .Select(g => g.Key + (g.Count() > 1 ? " (x" + g.Count() + ")" : "")).ToArray();
                        string resourcesCS = string.Join(", ", resources);
                        GameShip.AddHudText("Buying " + resourcesCS + " from " + currentDestinationTown.name, Color.gray, 1f);
                    }
                }
            } else
            {
                if (!RequestTradeMisison)
                {
                    GameShip.AddHudText("Can't find trade missions", Color.red, 1f);
                }
            }
        }
    }

    public void UpdateShip()
    {
        GamePlayer gamePlayer = GetGamePlayer();
        if (gamePlayer != null)
        {
            GameShip.hullColor0 = gamePlayer.ship.hullColor0;
            GameShip.hullColor1 = gamePlayer.ship.hullColor1;
            GameShip.sailColor0 = gamePlayer.ship.sailColor0;
            GameShip.sailColor1 = gamePlayer.ship.sailColor1;
            GameShip.symbolTex = gamePlayer.ship.symbolTex;
        }
    }

    private GamePlayer GetGamePlayer()
    {
        return GamePlayer.Find(this.OwnerId);
    }

    public void UpdateTradeMissions()
    {
        if (GameShip != null && GameShip.position != null && GameShip.isActive)
        {
            tradeMissions.ForEach(x => x.Update());
            tradeMissions.RemoveAll(x => !x.Valid);
            if (tradeMissions.Count() < CargoSlots)
            {
                List<TradeMission> newTradeMissions = TradeMissionCalculator.FindTradeMissions(GameShip.position, CargoSlots - tradeMissions.Count());
                tradeMissions.AddRange(newTradeMissions);
                RequestTradeMisison = false;
            }
        }
    }

    private Boolean InRange(GameTown gameTown)
    {
        if(gameTown == null || gameTown.dockPos == null || GameShip == null || GameShip.position == null)
        {
            return false;
        }
        float range = Math.Max(Math.Max(
            Math.Abs(gameTown.dockPos.x - GameShip.position.x),
            Math.Abs(gameTown.dockPos.y - GameShip.position.y)), 
            Math.Abs(gameTown.dockPos.z - GameShip.position.z));
        return range <= RANGE; ;
    }

    private void Sell(GameTown gameTown)
    {
        List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();
    
        List<TradeMission> tradeMissionsCompleted = new List<TradeMission>();
        foreach (TradeMission tradeMission in tradeMissions)
        {
            if(tradeMission.Destination.id.Equals(gameTown.id))
            {
                int salePrice = gameTown.Sell(tradeMission.PlayerItem);
                if (salePrice != 0)
                {
                    tradeMission.SalePrice = salePrice;
                    tradeMissionsCompleted.Add(tradeMission);
                } else
                {
                    TradeMission newTradeMission = TradeMissionCalculator.FindATradeMissionForResource(tradeMission.ResourceName, tradeMission.Departure, GameShip.position);
                    if (newTradeMission != null)
                    {
                        tradeMission.Destination = newTradeMission.Destination;
                    }
                    else
                    {
                        TradeChat.Chat(GameShip.name + "No towns in this region allow me to sell " + tradeMission.PlayerItem.name + ". Cargo destroyed, cost: " + tradeMission.PlayerItem.gold + "g");
                        tradeMissionsToRemove.Add(tradeMission);
                    }
                }
            }
        }

        if (tradeMissionsCompleted.Count() > 0)
        {
            string[] resources = tradeMissionsCompleted
                .Select(m => Localization.Get(m.ResourceName))
                .GroupBy(x => x)
                .Select(g => g.Key + (g.Count() > 1 ? " (x" + g.Count() + ")" : "")).ToArray();
            string resourcesCS = string.Join(", ", resources);
            int profit = tradeMissionsCompleted
                .Select(m => m.GetProfit()).Sum();

            TradeChat.Chat(GameShip.name + " sold " + resourcesCS + " at " + gameTown.name + ". " + (profit >= 0 ? "Profit" : "Loss") + ": " + profit + "g");
            RemoveTradeMissions(tradeMissionsCompleted);

        }
        RemoveTradeMissions(tradeMissionsToRemove);
        MarkRequestNewTradeMissionsIfNeeded();
    }



    private void Buy(GameTown gameTown)
    {
        List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();
        List<TradeMission> tradeMissionsStockedUp = new List<TradeMission>();
        foreach (TradeMission tradeMission in tradeMissions)
        {
            if (tradeMission.Departure.id.Equals(gameTown.id))
            {
                if (tradeMission.Departure.HasResource(tradeMission.ResourceName))
                {
                    PlayerItem playerItem = CreatePlayerItemForResource(gameTown, tradeMission.ResourceName);
                    if (MyPlayer.GetResource("gold") > playerItem.gold && MyPlayer.Buy(playerItem.gold))
                    {
                            tradeMission.NotEnoughFunds = true;
                            tradeMission.Departure.ReduceProduction(tradeMission.ResourceName);
                            tradeMission.PlayerItem = playerItem;
                            tradeMissionsStockedUp.Add(tradeMission);
                    } else if (!tradeMission.NotEnoughFunds)
                    {
                        tradeMission.NotEnoughFunds = true;
                        TradeChat.Chat(GameShip.name + ": Not enough funds to buy " + playerItem.name + ", need " + playerItem.gold + "g");
                    }

                } else
                {
                    tradeMissionsToRemove.Add(tradeMission);
                }
            }
        }


        RemoveTradeMissions(tradeMissionsToRemove);
        if(tradeMissionsStockedUp.Count() > 0)
        {
            string[] resources = tradeMissionsStockedUp
                .Select(m => Localization.Get(m.ResourceName))
                .GroupBy(x => x)
                .Select(g => g.Key + (g.Count() > 1 ? " (x" + g.Count() + ")" : "")).ToArray();
            string resourcesCS = string.Join(", ", resources);
            int cost = tradeMissionsStockedUp
                .Select(m => m.PlayerItem.gold).Sum();
            TradeChat.Chat(GameShip.name + " bought " + resourcesCS + " at " + gameTown.name + " for " + cost + "g");

            if (GameAudio.instance != null)
                NGUITools.PlaySound(GameAudio.instance.buy);
            MyPlayer.saveNeeded = true;
            MyPlayer.Sync();
        }
    }

    private void MarkRequestNewTradeMissionsIfNeeded()
    {
        if (tradeMissions.Count() == 0)
        {
            this.RequestTradeMisison = true;
        }
    }

    private void RemoveTradeMissions(List<TradeMission> tradeMissionsToRemove)
    {
        foreach (TradeMission tradeMissionToRemove in tradeMissionsToRemove)
        {
            tradeMissions.Remove(tradeMissionToRemove);
        }
    }

    private static PlayerItem CreatePlayerItemForResource(GameTown gameTown, string resourceName)
    {
        float multi = GameZone.rewardMultiplier;
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

    private static int GetCargoSlots(string name)
    {
        TNet.DataNode ships = GameConfig.ships;
        if (ships == null || ships.children.size == 0)
            return DEFAULT_CARGO_SLOTS;
        TNet.DataNode child1 = ships.GetChild(name);
        if (child1 == null)
            return DEFAULT_CARGO_SLOTS;
        return child1.GetChild<int>("cargo");
    }
}
