using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Tasharen;
using System.Collections;

[RequireComponent(typeof(GameShip))]
public class TradeShip : TNBehaviour
{
    public GameShip gameShip { get; set; }
    const float RANGE = 8f;
    const int DEFAULT_CARGO_SLOTS = 2;
    public TNet.Player Owner;
    public Color sailColor0;
    public Color sailColor1;
    public string symbolTex;
    [NonSerialized]
    public List<TradeMission> TradeMissions = new List<TradeMission>();




    protected override void OnEnable()
    {
        base.OnEnable();
        this.gameShip = this.GetComponent<GameShip>();
        this.gameShip.onDeath += new GameShip.DeathCallback(this.Died);
        StartCoroutine(DoUpdateTradeMissions());
        StartCoroutine(DoStatusUpdates());
        StartCoroutine(DoUpdateNavigation());
        gameShip.spawnOnDeath = "yes";
    }

    protected virtual void OnDisable()
    {
        StopCoroutine(DoUpdateTradeMissions());
        StopCoroutine(DoStatusUpdates());
        StopCoroutine(DoUpdateNavigation());
        tno.Send("OnUpdateShip", TNet.Target.All, sailColor0, sailColor1, symbolTex);
    }

    IEnumerator DoUpdateTradeMissions()
    {
        for (; ; )
        {
            try
            {
                if (TNManager.isHosting)
                {
                    UpdateTradeMissions();
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
                if (TNManager.isHosting)
                {
                    UpdateNavigation();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            yield return new WaitForSeconds(0.5f);

        }
    }

    IEnumerator DoStatusUpdates()
    {
        for (; ; )
        {
            try
            {
                if (TNManager.isHosting)
                {
                    UpdateShip();
                    UpdateStatus();
                }
            } catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            yield return new WaitForSeconds(3f);

        }
    }

    private void Died(GameShip ship)
    {
        this.gameShip.onDeath -= new GameShip.DeathCallback(this.Died);
        Dettach();
    }

    public GameShip GetGameShip()
    {
        return gameShip;
    }

    public void SaveTradeMissions()
    {
        UpdateTradeMissions(TradeMissions);
    }

    [TNet.RFC]
    protected void OnUpdateTradeMissions(TNet.List<TNet.DataNode> tradeMissions)
    {
        this.TradeMissions = ListToTNetList<TNet.DataNode, TradeMission>.From(tradeMissions, m => TradeMission.FromDataNode(m));
    }

    public void UpdateTradeMissions(List<TradeMission> tradeMissions)
    {
        tno.Send("OnUpdateTradeMissions", TNet.Target.All, ListToTNetList<TradeMission, TNet.DataNode>.To(tradeMissions, m => m.ToDataNode()));
    }

    public Boolean HasResource(string resource)
    {
        return TradeMissions.Where(x => x.StockedUp() && x.ResourceName.Equals(resource)).Any();
    }

    public void UpdateNavigation()
    {
        var GameShip = GetGameShip();

        if (GameShip != null && GameShip.position != null && GameShip.isActive)
        {
            TradeMission currentTradeMission = TradeMissions.FirstOrDefault();

            if (currentTradeMission != null)
            {
                Boolean selling = currentTradeMission.StockedUp();
                var destination = currentTradeMission.GetDestination();
                var departure = currentTradeMission.GetDeparture();

                GameTown currentDestinationTown = selling ? currentTradeMission.GetDestination() : currentTradeMission.GetDeparture();
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
        var GameShip = GetGameShip();

        var tradeMissions = TradeMissions;
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
                        .Select(g => g.Key + (g.Count() > 1 ? " (x" + g.Count() + ")" : "")).ToArray();
                    string resourcesCS = string.Join(", ", resources);
                    AddHudText("Delivering " + resourcesCS + " to " + currentDestinationTown.name, 1f);
                }
                else
                {
                    string[] resources = tradeMissions.Where(m => m.Departure == currentTradeMission.Departure && !m.StockedUp()).Select(m => Localization.Get(m.ResourceName))
                        .GroupBy(x => x)
                        .Select(g => g.Key + (g.Count() > 1 ? " (x" + g.Count() + ")" : "")).ToArray();
                    string resourcesCS = string.Join(", ", resources);
                    AddHudText("Buying " + resourcesCS + " from " + currentDestinationTown.name, 1f);
                }
            }
        } else
        {
            AddHudText("Where are the good deals ?", 1f);
        }
    }

    internal void Activate()
    {
        this.Owner = null;
        var gameShip = GetGameShip();
        tno.onDestroy += Dettach;
        this.TradeMissions = new List<TradeMission>();
    }

    internal void Activate(TNet.Player owner)
    {
        this.Owner = owner;
        Activate();
    }

    public void AddHudText(string message, float duration, bool localized = false)
    {

        tno.Send("OnAddHudText", TNet.Target.All, message, duration, localized);
    }

    [TNet.RFC]
    protected void OnAddHudText(string message, float duration, bool localized)
    {
        if (Owner != null && Owner.id == MyPlayer.id)
        {
            Color GREEN = new Color(111, 206, 101);
            GetGameShip().AddHudText(message, GREEN, duration, localized);
        } else
        {
            GetGameShip().AddHudText(message, Color.gray, duration, localized);
        }
    }

    public void UpdateShip()
    {
        var GameShip = GetGameShip();

        if (Owner != null)
        {
            var gamePlayer = GamePlayer.Find(Owner.id);
            if (GameShip.sailColor0 != gamePlayer.ship.sailColor0 || GameShip.sailColor1 != gamePlayer.ship.sailColor1 || GameShip.symbolTex != gamePlayer.ship.symbolTex)
            {
                tno.Send("OnUpdateShip", TNet.Target.All, gamePlayer.ship.sailColor0, gamePlayer.ship.sailColor1, gamePlayer.ship.symbolTex);
            }
        }
    }

    [TNet.RFC]
    protected void OnUpdateShip(Color sailColor0, Color sailColor1, string symbolTex)
    {
        var GameShip = GetGameShip();

        GameShip.sailColor0 = sailColor0;
        GameShip.sailColor1 = sailColor1;
        GameShip.symbolTex = symbolTex;
    }


    public void UpdateTradeMissions()
    {
        TradeMissions.ForEach(x => UpdateTradeMission(x));
        TradeMissions.RemoveAll(x => !x.Valid);

        var cargoSlots = GetCargoSlots();
        if (TradeMissions.Count() < cargoSlots)
        {
            List<TradeMission> newTradeMissions = TradeMissionCalculator.FindTradeMissions(GetGameShip(), cargoSlots - TradeMissions.Count());
            TradeMissions.AddRange(newTradeMissions);
        }
        SaveTradeMissions();
    }


    private Boolean InRange(GameTown gameTown)
    {
        var GameShip = GetGameShip();

        if (gameTown == null || gameTown.dockPos == null || GameShip == null || GameShip.position == null)
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
        var GameShip = GetGameShip();

        if (gameTown != null && GameShip.position != null)
        {

            List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();
            foreach (TradeMission tradeMission in TradeMissions.Where(m => m.Valid))
            {
                if (tradeMission.Destination.Equals(gameTown.id))
                {
                    PlayerItem playerItem = CreatePlayerItemForResource(tradeMission.GetDeparture(), tradeMission.ResourceName, tradeMission.PurchasePrice);
                    int demand = gameTown.GetDemand(tradeMission.ResourceName);
                    if (demand > 0)
                    {
                        Sell(tradeMission.Destination, tradeMission.ResourceName, tradeMission.PurchasePrice);
                        tradeMissionsToRemove.Add(tradeMission);
                    }
                    else if (tradeMission.Destination == tradeMission.Departure)
                    {
                        BuyBack(tradeMission.Departure, tradeMission.ResourceName, tradeMission.PurchasePrice);
                        tradeMissionsToRemove.Add(tradeMission);
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
            if (tradeMissionsToRemove.Count() > 0)
            {
                RemoveTradeMissions(tradeMissionsToRemove);
                SaveTradeMissions();
            }
        }
    }

    public void BuyBack(int departureId, string resourceName, int purchasePrice)
    {
        if (Owner != null)
        {
            tno.Send("OnBuyBack", Owner.id, departureId, resourceName, purchasePrice);
        }
    }

    [TNet.RFC]
    protected void OnBuyBack(int departureId, string resourceName, int purchasePrice)
    {
        float multi = GameZone.rewardMultiplier;
        var gameTown = GameTown.Find(departureId);
        int price = gameTown.GetBuyBackOffer(resourceName, purchasePrice, multi);
        gameTown.IncreaseProduction(resourceName);
        MyPlayer.ModifyResource("gold", -purchasePrice, true);
        MyPlayer.ModifyResource("gold", price, true);
        int profit = price - purchasePrice;
        TradeChat.Bad(GetGameShip().name + " sold " + Localization.Get(resourceName) + " back to " + gameTown.name + " as a buy back. " + (profit >= 0 ? "Profit" : "Loss") + " : " + Math.Abs(profit) + "g");
        MyPlayer.saveNeeded = true;
        MyPlayer.Sync();

    }

    public void Sell(int destination, string resourceName, int purchasePrice)
    {
        if (Owner != null)
        {
            tno.Send("OnSell", Owner.id, destination, resourceName, purchasePrice);
        }
    }

    [TNet.RFC]
    protected void OnSell(int destination, string resourceName, int purchasePrice)
    {
        var gameTown = GameTown.Find(destination);
        var playerItem = CreatePlayerItemForResource(gameTown, resourceName, purchasePrice);
        TradeChat.DebugTradeShip("Sale gold of playeritem: " + playerItem.gold);
        int salePrice = gameTown.Sell(playerItem);
        if (salePrice != 0)
        {
            MyPlayer.ModifyResource("gold", -purchasePrice, true);
            int profit = salePrice - purchasePrice;
            var text = GetGameShip().name + " sold " + Localization.Get(resourceName) + " at " + gameTown.name + ". " + (profit >= 0 ? "Profit" : "Loss") + " : " + Math.Abs(profit) + "g";
            if (profit > 0)
            {
                TradeChat.Chat(text);
            }
            else
            {
                TradeChat.Bad(text);
            }
            MyPlayer.saveNeeded = true;
            MyPlayer.Sync();

        }
    }

    private void OnClick() {
        UIPopupList popupList = UIGameWindow.popupList;
        popupList.Clear();
        if (Owner == null)
        {
            string gold = GameTools.FormatGold(CalculatePrice(), false, true);
            popupList.AddItem(Localization.Format("Hire as trader", gameShip.name, gold), "hire");
            EventDelegate.Set(popupList.onChange, new EventDelegate.Callback(HirePopupCallBack));
        } else if(Owner.id == MyPlayer.id)
        {
            string gold = GameTools.FormatGold(CalculatePrice(), false, true);
            popupList.AddItem(Localization.Format("Fire as trader", gameShip.name), "fire");
            EventDelegate.Set(popupList.onChange, new EventDelegate.Callback(FirePopupCallBack));
        } else
        {
            popupList.AddItem("No options", "fire");
        }
        popupList.Show();
    }
    
    private void HirePopupCallBack()
    {
        int price = CalculatePrice();
        MyPlayer.ModifyResource("gold", -price, true);
        MyPlayer.Sync();
        UpdateTraderOwner(GamePlayer.me.netPlayer.id);
        UIStatusBar.Show(Localization.Format("Hired", gameShip.name, price));

        UIPopupList popupList = UIGameWindow.popupList;
        popupList.Clear();
        EventDelegate.Remove(popupList.onChange, new EventDelegate.Callback(HirePopupCallBack));
    }

    private void FirePopupCallBack()
    {
        int price = CalculatePrice();
        UpdateTraderOwner(null);
        UIPopupList popupList = UIGameWindow.popupList;
        popupList.Clear();
        UIStatusBar.Show(Localization.Format("Fired", gameShip.name));
        EventDelegate.Remove(popupList.onChange, new EventDelegate.Callback(FirePopupCallBack));

    }

    private int CalculatePrice()
    {
        return GetCargoSlots() * 200;
    }
    public void Dettach()
    {
        OnDisable();
    }

    private void Buy(GameTown gameTown)
    {
        var GameShip = GetGameShip();

        List<TradeMission> tradeMissionsToRemove = new List<TradeMission>();
        var tradeMissions = TradeMissions;
        foreach (TradeMission tradeMission in tradeMissions.Where(m => m.Valid))
        {
            var departure = tradeMission.GetDeparture();
            if (departure.id.Equals(gameTown.id))
            {

                var production = departure.GetProduction(tradeMission.ResourceName);
                if (production >= 1)
                {
                    PlayerItem playerItem = CreatePlayerItemForResource(gameTown, tradeMission.ResourceName);
                    TradeChat.Chat(GameShip.name + " buying " + playerItem.name + " at " + gameTown.name + " for " + playerItem.gold + "g");
                    tradeMission.GetDeparture().ReduceProduction(tradeMission.ResourceName);
                    tradeMission.PurchasePrice = playerItem.gold;

                } else
                {
                    tradeMissionsToRemove.Add(tradeMission);
                }
            }
        }
        if (tradeMissionsToRemove.Count() > 0)
        {
            RemoveTradeMissions(tradeMissionsToRemove);
            SaveTradeMissions();
        }
    }

    private void RemoveTradeMissions(List<TradeMission> tradeMissionsToRemove)
    {
        foreach (TradeMission tradeMissionToRemove in tradeMissionsToRemove)
        {
            TradeMissions.Remove(tradeMissionToRemove);
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
        var GameShip = GetGameShip();
        return GetCargoSlots(GameShip.prefabName);
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

    public TNet.Player GetOwner()
    {
        return Owner;
    }

    public void UpdateTraderOwner(int? playerId)
    {
        if (playerId.HasValue)
        {
            TradeChat.DebugTradeShip("Updating playerId " + playerId.Value);
            tno.Send("OnUpdateTraderOwner", TNet.Target.All, playerId.Value);
        } else
        {
            tno.Send("OnClearTraderOwner", TNet.Target.All);
        }
    }

    [TNet.RFC]
    protected void OnUpdateTraderOwner(int playerId)
    {
        this.Owner = GamePlayer.Find(playerId).netPlayer;
        TradeChat.DebugTradeShip("Updated owner to " + Owner.id);
    }

    [TNet.RFC]
    protected void OnClearTraderOwner()
    {
        this.Owner = null;
    }

    public void UpdateTradeMission(TradeMission tradeMission)
    {
        if (tradeMission.StockedUp())
        {
            var destinationTown = tradeMission.GetDestination();
            if (destinationTown == null || !destinationTown.NeedsResource(tradeMission.ResourceName) || !GameTownIsAlly(gameShip, destinationTown))
            {
                if(destinationTown == null)
                {
                    TradeChat.Bad("destination is null");
                }
                if (!GameTownIsAlly(gameShip, destinationTown))
                {
                    TradeChat.Bad(destinationTown.name + " is not an ally");
                }
                if (!destinationTown.NeedsResource(tradeMission.ResourceName))
                {
                    TradeChat.Bad(destinationTown.name+ " doesn't need resource " + tradeMission.ResourceName);
                }
                tradeMission.Valid = false;
            }
        }
        else
        {
            var departure = tradeMission.GetDeparture();

            if (!departure.HasResource(tradeMission.ResourceName) || !GameTownIsAlly(gameShip, departure))
            {
                if (!GameTownIsAlly(gameShip, departure))
                {
                    TradeChat.Bad(departure.name + " is not an ally");
                }
                if (!departure.HasResource(tradeMission.ResourceName))
                {
                    TradeChat.Bad(departure.name + " doesn't have resource " + tradeMission.ResourceName);
                }
                tradeMission.Valid = false;
            }
        }
    }


    public static Boolean GameTownIsAlly(GameShip gameShip, GameTown gameTown)
    {
        return (gameTown.factionID == 0 && gameShip.factionID == 0) || (gameShip.factionID > 0 && gameTown.factionID > 0);
    }
}
