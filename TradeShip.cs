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
    private static readonly int DEFAULT_CARGO_SLOTS = 2;
    public int cost;
    public int profit = 0;

    public TNet.Player Owner;
    public int cargoSlots = 2;

    [NonSerialized]
    public List<TradeMission> TradeMissions = new List<TradeMission>();

    protected override void OnEnable()
    {
        base.OnEnable();
        this.gameShip = this.GetComponent<GameShip>();
        this.gameShip.onDeath += new GameShip.DeathCallback(this.Died);
        StartCoroutine(DoUpdateShipToPlayer());
    }

    protected virtual void OnDisable()
    {
        StopCoroutine(DoUpdateShipToPlayer());
    }


    private void Died(GameShip ship)
    {
        this.gameShip.onDeath -= new GameShip.DeathCallback(this.Died);
        Dettach();
        if (Owner != null && Owner.id == MyPlayer.id)
        {
            int cargoWorth = TradeMissions.Sum(m => !m.Completed ? m.PurchasePrice : 0);
            if (cargoWorth == 0)
            {
                TradeChat.Chat(Localization.Format("Ship destroyed", this.gameShip.name));
            }
            else
            {
                TradeChat.Chat(Localization.Format("Ship destroyed with cargo", this.gameShip.name, Format.FormatGold(cargoWorth)));
                MyPlayer.ModifyResource("gold", -cargoWorth, true);
                MyPlayer.Sync();
            }
        }
    }

    public GameShip GetGameShip()
    {
        return gameShip;
    }

    public Boolean HasResource(string resource)
    {
        return TradeMissions.Where(x => x.StockedUp() && x.ResourceName.Equals(resource)).Any();
    }

    internal void SetTradeMissions(List<TradeMission> tradeMissions)
    {
        this.TradeMissions = tradeMissions;
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
        if(GameShip.ai.state != AIShip.State.Combat) { 
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
                        AddHudText(Localization.Format("Delivering",resourcesCS, Format.FormatTown(currentDestinationTown)), 1f);
                    }
                    else
                    {
                        string[] resources = tradeMissions.Where(m => m.Departure == currentTradeMission.Departure && !m.StockedUp()).Select(m => Localization.Get(m.ResourceName))
                            .GroupBy(x => x)
                            .Select(g => g.Key + (g.Count() > 1 ? " (x" + g.Count() + ")" : "")).ToArray();
                        string resourcesCS = string.Join(", ", resources);
                        AddHudText(Localization.Format("Buying", resourcesCS, Format.FormatTown(currentDestinationTown)), 1f);
                        

                    }
                }
            } else
            {
                AddHudText("Where are the good deals ?", 1f);
            }
        }
    }

    internal void Activate()
    {
        var gameShip = GetGameShip();
        tno.onDestroy += Dettach;
        this.TradeMissions = new List<TradeMission>();
        this.cargoSlots = CalculateCargoSlots(gameShip.prefabName);
    }


    public void AddHudText(string message, float duration)
    {
        GetGameShip().AddHudText(message, Color.gray, duration, false);
    }

    public void UpdateTradeMissions()
    {
        TradeMissions.ForEach(x => UpdateTradeMission(x));
        TradeMissions.RemoveAll(x => !x.Valid);

        if (TradeMissions.Count() < cargoSlots)
        {
            List<TradeMission> newTradeMissions = TradeMissionCalculator.FindTradeMissions(GetGameShip(), cargoSlots - TradeMissions.Count());
            TradeMissions.AddRange(newTradeMissions);
        }
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
                if (tradeMission.StockedUp() && tradeMission.Destination.Equals(gameTown.id))
                {
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
            }           
        }
    }



    public void BuyBack(int departureId, string resourceName, int purchasePrice)
    {
        if (Owner != null)
        {
            float multi = GameZone.rewardMultiplier;
            var gameTown = GameTown.Find(departureId);
            int price = gameTown.GetBuyBackOffer(resourceName, purchasePrice, multi);
            gameTown.IncreaseProduction(resourceName);
            MyPlayer.ModifyResource("gold", price  - purchasePrice, true);
            int profit = price - purchasePrice;
            this.profit += profit;
            var profitText = profit >= 0 ? Localization.Format("profit") : Localization.Format("loss") + " : " + Format.FormatGold(Math.Abs(profit));
            TradeChat.Bad(Localization.Format("BuyBack", Format.FormatShip(GetGameShip()), Localization.Get(resourceName), Format.FormatTown(gameTown), profitText));
            MyPlayer.saveNeeded = true;
            MyPlayer.Sync();
        }
    }

    public void Sell(int destination, string resourceName, int purchasePrice)
    {
        if (Owner != null)
        {
            var gameTown = GameTown.Find(destination);
            var playerItem = ResourceFactory.CreatePlayerItemForResource(gameTown, resourceName, purchasePrice);
            int salePrice = gameTown.Sell(playerItem);
            if (salePrice > 0)
            {
                int profit = salePrice - purchasePrice;

                MyPlayer.ModifyResource("gold", -profit, true);
                MyPlayer.Sync();
                string profitOrLoss = profit >= 0 ?
                    Localization.Format("profit") :
                    Localization.Format("loss");
                string profitText = profitOrLoss + " : " + Format.FormatGold(Math.Abs(profit));
                var text = Localization.Format(
                    "Sold", 
                    Format.FormatShip(GetGameShip()),
                    Localization.Get(resourceName), 
                    Format.FormatTown(gameTown), 
                    profitText) ;
                this.profit += profit;
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
    }




    private IEnumerator DoUpdateShipToPlayer()
    {
        for (; ; )
        {
            UpdateShip();
            yield return new WaitForSeconds(5f);
        }
    }

    private void UpdateShip()
    {
        if (Owner != null)
        {
            var player = GamePlayer.Find(Owner.id);
            if (player != null && player.ship != null)
            {
                gameShip.factionID = player.factionID;
                gameShip.ai.state = AIShip.State.Idle;
                gameShip.symbolTex = player.ship.symbolTex;
                gameShip.sailColor0 = player.ship.sailColor0;
                gameShip.sailColor1 = player.ship.sailColor1;
                gameShip.hullColor0 = player.ship.hullColor0;
                gameShip.hullColor1 = player.ship.hullColor1;
            }
        }
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
            if (!tradeMission.StockedUp() && departure.id.Equals(gameTown.id))
            {

                var production = departure.GetProduction(tradeMission.ResourceName);
                if (production >= 1)
                {
                    PlayerItem playerItem = ResourceFactory.CreatePlayerItemForResource(gameTown, tradeMission.ResourceName);
                    profit -= playerItem.gold;
                    TradeChat.Chat(Localization.Format("Buying at", Format.FormatShip(gameShip),playerItem.name , Format.FormatTown(gameTown), Format.FormatGold(playerItem.gold)));
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
        }
    }

    private void RemoveTradeMissions(List<TradeMission> tradeMissionsToRemove)
    {
        foreach (TradeMission tradeMissionToRemove in tradeMissionsToRemove)
        {
            TradeMissions.Remove(tradeMissionToRemove);
        }
    }

    private static int CalculateCargoSlots(string name)
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
            tno.Send("OnUpdateTraderOwner", TNet.Target.AllSaved, playerId.Value);
        } else
        {
            tno.Send("OnClearTraderOwner", TNet.Target.AllSaved);
        }
    }

    [TNet.RFC]
    protected void OnUpdateTraderOwner(int playerId)
    {
        GamePlayer gamePlayer = GamePlayer.Find(playerId);
        if (gamePlayer != null) {
            this.Owner = gamePlayer.netPlayer;
            UpdateShip();
        }
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
                    TradeChat.Bad(Localization.Format("Not an ally", Format.FormatShip(gameShip), Format.FormatTown(destinationTown)));
                }
                if (!destinationTown.NeedsResource(tradeMission.ResourceName))
                {
                    TradeChat.Bad(Localization.Format("Does not need resource", Format.FormatShip(gameShip), Format.FormatTown(destinationTown),Localization.Get(tradeMission.ResourceName)));
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
                    TradeChat.Bad(Localization.Format("Not an ally", Format.FormatShip(gameShip), Format.FormatTown(departure)));
                }
                if (!departure.HasResource(tradeMission.ResourceName))
                {
                    TradeChat.Bad(Localization.Format("Does not have resource", Format.FormatShip(gameShip), Format.FormatTown(departure), Localization.Get(tradeMission.ResourceName)));
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
