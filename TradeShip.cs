using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeShip
{
    public GameShip GameShip { get; set; }
    const float RANGE = 8f;
    const int DEFAULT_CARGO_SLOTS = 2;
    public const string TRADER_OWNER = "traderOwner";
    public const string ORIGINAL_SHIP_PROPERTIES = "OriginalShipProperties";
    public const string TRADE_MISSIONS = "tradeMissions";
    public const string COMPLETED_TRADE_MISSIONS = "completedTradeMissions";
    public const string STOP_TRADE = "stopTrade";
    public const string CARGO_SLOTS = "cargoSlots";
    public const string REQUEST_TRADE_MISSION = "requestTradeMission";

    public TradeShip(GameShip gameShip)
    {
        this.GameShip = gameShip;
        if(gameShip.Get<TNet.DataNode>(ORIGINAL_SHIP_PROPERTIES) == null)
        {
            gameShip.Set(ORIGINAL_SHIP_PROPERTIES, OriginalShipProperties.Create(gameShip).Convert());
        }
    }

    public OriginalShipProperties ReadOriginalShipProperties()
    {
        TNet.DataNode data = GameShip.Get<TNet.DataNode>(ORIGINAL_SHIP_PROPERTIES);
        if (data != null) {
            return OriginalShipProperties.FromData(data);
        }
        return null;
    }

    public void SaveTradeMissions(List<TradeMission> tradeMissions)
    {
        GameShip.Set(TRADE_MISSIONS, ListToTNetList<TradeMission, TNet.DataNode>.To(tradeMissions.Where(x => !x.Completed).ToList(), m => m.ToDataNode()));
        AddCompletedMissions(tradeMissions.Where(x => x.Completed).ToList());
    }

    public void AddCompletedMissions(List<TradeMission> tradeMissions)
    {
        if (GetOwner() != null)
        {
            List<TradeMission> completedMissions = ReadCompletedTradeMissions();
            completedMissions.AddRange(tradeMissions);
            GameShip.Set(COMPLETED_TRADE_MISSIONS, ListToTNetList<TradeMission, TNet.DataNode>.To(completedMissions.Where(x => x.Completed).ToList(), m => m.ToDataNode()));
        } else
        {
            ClearCompletedTradeMissions();
        }
    }

    public List<TradeMission> ReadTradeMissions()
    {
        TNet.List<TNet.DataNode> tradeMissions = GameShip.Get<TNet.List<TNet.DataNode>>(TRADE_MISSIONS);
        if(tradeMissions == null)
        {
            return new List<TradeMission>();
        }
        return ListToTNetList<TNet.DataNode, TradeMission>.From(tradeMissions, m => TradeMission.FromDataNode(m));
    }

    internal void UpdateTownInteraction()
    {
        throw new NotImplementedException();
    }

    public void ClearCompletedTradeMissions()
    {
        GameShip.Set(COMPLETED_TRADE_MISSIONS, new TNet.List<TNet.DataNode>());
    }


    public List<TradeMission> ReadCompletedTradeMissions()
    {
        var tradeMissions = GameShip.Get<TNet.List<TNet.DataNode>>(COMPLETED_TRADE_MISSIONS);
        if (tradeMissions == null)
        {
            return new List<TradeMission>();
        }
        return ListToTNetList<TNet.DataNode, TradeMission>.From(tradeMissions, m => TradeMission.FromDataNode(m));
    }

    public Boolean HasResource(string resource)
    {
        return ReadTradeMissions().Where(x => x.StockedUp() && x.ResourceName.Equals(resource)).Any();
    }

    public void UpdateNavigation()
    {
        if (GameShip != null && GameShip.position != null && GameShip.isActive)
        {
            TradeMission currentTradeMission = ReadTradeMissions().FirstOrDefault();

            if (currentTradeMission != null)
            {
                Boolean selling = currentTradeMission.StockedUp();
                var destination = currentTradeMission.GetDestination();
                var departure = currentTradeMission.GetDeparture();

                GameTown currentDestinationTown = selling ? currentTradeMission.GetDestination() : currentTradeMission.GetDeparture() ;
                if (selling && InRange(destination))
                {
                    Sell(destination);
                }
                else if (!selling && InRange(departure))
                {
                    Buy(departure);
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
        if(IsValid()) {
            var tradeMissions = ReadTradeMissions();
            TradeMission currentTradeMission = tradeMissions.FirstOrDefault();

            if (currentTradeMission != null)
            {
                Boolean selling = currentTradeMission.StockedUp();
                GameTown currentDestinationTown = selling ? currentTradeMission.GetDestination() : currentTradeMission.GetDeparture();
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
                if (!GetRequestTradeMission())
                {
                    GameShip.AddHudText("Where are the good deals ?", Color.gray, 1f);
                }
            }
        }
    }

    public void UpdateShip()
    {
        GamePlayer gamePlayer = GetOwner();
        if (gamePlayer != null)
        {
            GameShip.sailColor0 = gamePlayer.ship.sailColor0;
            GameShip.sailColor1 = gamePlayer.ship.sailColor1;
            GameShip.symbolTex = gamePlayer.ship.symbolTex;
        } else
        {
            OriginalShipProperties originalShipProperties = ReadOriginalShipProperties();
            if (originalShipProperties != null)
            {
                GameShip.sailColor0 = originalShipProperties.sailColor0;
                GameShip.sailColor1 = originalShipProperties.sailColor1;
                GameShip.symbolTex = originalShipProperties.symbolTex;
            }
        }
    }


    public void UpdateTradeMissions()
    {
        if (IsValid())
        {
            var tradeMissions = ReadTradeMissions();
            tradeMissions.ForEach(x => UpdateTradeMission(x));
            tradeMissions.RemoveAll(x => !x.Valid);

            tradeMissions.RemoveAll(x => !x.StockedUp());
            var cargoSlots = GetCargoSlots();
            if (!GetStopTrade() && tradeMissions.Count() < cargoSlots)
            {
                List<TradeMission> newTradeMissions = TradeMissionCalculator.FindTradeMissions(GameShip, cargoSlots - tradeMissions.Count());
                tradeMissions.AddRange(newTradeMissions);

                RequestTradeMission(false);
            }
            SaveTradeMissions(tradeMissions);
        }
    }

    public Boolean GetStopTrade()
    {
        Boolean? stopTrade = GameShip.Get<Boolean?>("stopTrade");
        return stopTrade.HasValue && stopTrade.Value;
    }

    public void SetStopTrade(Boolean value)
    {
        GameShip.Set("stopTrade", value);
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
        if (gameTown != null && GameShip.position != null)
        {
            List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();

            var tradeMissions = ReadTradeMissions();
            foreach (TradeMission tradeMission in tradeMissions.Where(m=>m.Valid))
            {
                if (tradeMission.Destination.Equals(gameTown.id))
                {
                    PlayerItem playerItem = CreatePlayerItemForResource(tradeMission.GetDeparture(), tradeMission.ResourceName, tradeMission.PurchasePrice);
                    int demand = gameTown.GetDemand(tradeMission.ResourceName);
                    if (demand > 0)
                    {
                        tradeMission.Completed = true;
                    }
                    else if(tradeMission.Destination == tradeMission.Departure)
                    {
                        //BuyBack scenario
                        tradeMission.Completed = true;
                    }
                    else
                    {
                        TradeMission newTradeMission = TradeMissionCalculator.FindATradeMissionForResource(tradeMission.ResourceName, tradeMission.GetDeparture(), GameShip.position);
                        if (newTradeMission != null)
                        {
                            tradeMission.Destination = newTradeMission.Destination;
                        }
                        else
                        {
                            //BuyBack
                            tradeMission.Destination = tradeMission.Departure;
                        }
                    }
                }
            }

            RemoveTradeMissions(tradeMissions, tradeMissionsToRemove);
            MarkRequestNewTradeMissionsIfNeeded(tradeMissions);
            tradeMissions.Sort(new TradeMissionComparator());
            SaveTradeMissions(tradeMissions);
        }
    }

    public void PlayerSell()
    {
        var completedTradeMissions = ReadCompletedTradeMissions();
        if (completedTradeMissions.Count() > 0)
        {
            foreach (TradeMission tradeMission in completedTradeMissions)
            {
                if (tradeMission.Departure == tradeMission.Destination)
                {
                    float multi = GameZone.rewardMultiplier;
                    int price = tradeMission.GetDeparture().GetBuyBackOffer(tradeMission.ResourceName, tradeMission.PurchasePrice, multi);
                    tradeMission.GetDeparture().IncreaseProduction(tradeMission.ResourceName);
                    MyPlayer.ModifyResource("gold", -tradeMission.PurchasePrice, true);
                    MyPlayer.ModifyResource("gold", price, true);
                    int profit = price - tradeMission.PurchasePrice;
                    TradeChat.Bad(GameShip.name + " sold " + Localization.Get(tradeMission.ResourceName) + " back to " + tradeMission.GetDestination().name + " as a buy back. " + (profit >= 0 ? "Profit" : "Loss") + " : " + Math.Abs(profit) + "g");
                }
                else
                {
                    var playerItem = CreatePlayerItemForResource(tradeMission.GetDestination(), tradeMission.ResourceName, tradeMission.PurchasePrice);
                    int salePrice = tradeMission.GetDestination().Sell(playerItem);
                    if (salePrice != 0)
                    {
                        MyPlayer.ModifyResource("gold", -tradeMission.PurchasePrice, true);
                        int profit = salePrice - tradeMission.PurchasePrice;
                        var text = GameShip.name + " sold " + Localization.Get(tradeMission.ResourceName) + " at " + tradeMission.GetDestination().name + ". " + (profit >= 0 ? "Profit" : "Loss") + " : " + Math.Abs(profit) + "g";
                        if (profit > 0)
                        {
                            TradeChat.Chat(text);
                        } else
                        {
                            TradeChat.Bad(text);

                        }
                    }
                }
            }
            MyPlayer.saveNeeded = true;
            MyPlayer.Sync();
            ClearCompletedTradeMissions();
        } else
        {
        }
    }


    private void Buy(GameTown gameTown)
    {
        List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();
        List<TradeMission> tradeMissionsStockedUp = new List<TradeMission>();
        var tradeMissions = ReadTradeMissions();
        foreach (TradeMission tradeMission in tradeMissions.Where(m => m.Valid))
        {
            var departure = tradeMission.GetDeparture();
            if (departure.id.Equals(gameTown.id))
            {

                var production = departure.GetProduction(tradeMission.ResourceName);
                if (production > 1)
                {
                    PlayerItem playerItem = CreatePlayerItemForResource(gameTown, tradeMission.ResourceName);
                    TradeChat.Chat(GameShip.name + " buying " + playerItem.name + " at " + gameTown.name + " for " + playerItem.gold + "g");
                    tradeMission.GetDeparture().ReduceProduction(tradeMission.ResourceName);
                    tradeMission.PurchasePrice = playerItem.gold;
                    tradeMissionsStockedUp.Add(tradeMission);

                } else
                {
                    tradeMissionsToRemove.Add(tradeMission);
                }
            }
        }

        RemoveTradeMissions(tradeMissions, tradeMissionsToRemove);
        SaveTradeMissions(tradeMissions);
    }

    private void MarkRequestNewTradeMissionsIfNeeded(List<TradeMission> allTradeMissions)
    {
        if (allTradeMissions.Where(x=>x.Valid && !x.Completed).Count() == 0)
        {
            RequestTradeMission( true);
        }
    }

    private void RequestTradeMission(Boolean boolean)
    {
        GameShip.Set(REQUEST_TRADE_MISSION, boolean ? 1 : 0);
    }

    private Boolean GetRequestTradeMission()
    {
        int? requestTradeMission = GameShip.Get<int?>(REQUEST_TRADE_MISSION);
        return requestTradeMission.HasValue && requestTradeMission.Value == 1 ;
    }

    private void RemoveTradeMissions(List<TradeMission> allTradeMissions, List<TradeMission> tradeMissionsToRemove)
    {
        foreach (TradeMission tradeMissionToRemove in tradeMissionsToRemove)
        {
            allTradeMissions.Remove(tradeMissionToRemove);
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
        playerItem.SetStat(resourceName, 1);
        playerItem.SetStat("Level", GameZone.challengeLevel);
        playerItem.type = "Shipment";
        playerItem.SetStat(resourceName, 1);
        playerItem.SetStat("Level", GameZone.challengeLevel);
        return playerItem;

    }


    private static PlayerItem CreatePlayerItemForResource(GameTown gameTown, string resourceName, int purchasePrice)
    {
        PlayerItem playerItem = new PlayerItem();
        playerItem.name = Localization.Format("Shipment of", Localization.Get(resourceName));
        playerItem.info = Localization.Format("Loaded in", gameTown.name);
        playerItem.gold = purchasePrice;
        playerItem.type = "Shipment";
        playerItem.SetStat(resourceName, 1);
        playerItem.SetStat("Level", GameZone.challengeLevel);
        return playerItem;
    }

    public int GetCargoSlots()
    {
        int? cargoSlots = GameShip.Get<int?>(CARGO_SLOTS);
        if (!cargoSlots.HasValue)
        {
            cargoSlots = GetCargoSlots(GameShip.prefabName);
            GameShip.Set(CARGO_SLOTS, cargoSlots);
        }
        return cargoSlots.Value;
    }

    private static int GetCargoSlots(string name)
    {
        TNet.DataNode ships = GameConfig.ships;
        if (ships == null || ships.children.size == 0)
        {
            return DEFAULT_CARGO_SLOTS;
        }
        TNet.DataNode child1 = ships.GetChild(name);
        if (child1 == null)
            return DEFAULT_CARGO_SLOTS;
        return child1.GetChild<int>("cargo");
    }

    public GamePlayer GetOwner()
    {
        int? traderOwnerId = GetTraderOwnerId();
        if (traderOwnerId.HasValue)
        {
            GamePlayer gamePlayer = GamePlayer.Find(traderOwnerId.Value);
            return gamePlayer;
        }
        return null;
    }

    public int? GetTraderOwnerId()
    {
        return GameShip.Get<int?>(TRADER_OWNER);
    }

    public void SetTraderOwnerId(int? traderOwnerId)
    {
        GameShip.Set(TRADER_OWNER, traderOwnerId);
    }

    public Boolean IsValid()
    {
        if(GameShip == null || !GameShip.isActive)
        {
            return false;
        }
        if (GetStopTrade() && ReadTradeMissions().Count() == 0)
        {
            return false;
        }
        return true;
    }


    public void UpdateTradeMission(TradeMission tradeMission)
    {
        if (tradeMission.StockedUp())
        {
            var destinationTown = tradeMission.GetDestination();
            if (destinationTown == null || !destinationTown.NeedsResource(tradeMission.ResourceName) || !GameTownIsAlly(GameShip, destinationTown))
            {
                tradeMission.Valid = false;
            }
        }
        else
        {
            var departure = tradeMission.GetDeparture();

            if (!departure.HasResource(tradeMission.ResourceName) || !GameTownIsAlly(GameShip, departure))
            {
                tradeMission.Valid = false;
            }
        }
    }


    public static Boolean GameTownIsAlly(GameShip gameShip, GameTown gameTown)
    {
        return (gameTown.factionID == 0 && gameShip.factionID == 0) || (gameShip.factionID > 0 && gameTown.factionID > 0);
    }
}
