using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//WIP not used
internal class Market
{
    private static List<TradeProposal> activeTradeProposals = new List<TradeProposal>();

    internal void Update()
    {
        List<TradeMission> allCurrentTradeMissions = FindAllTradeMissions();
        List<ResourcePrice> demands = FindAllDemand( allCurrentTradeMissions);
        List<ResourcePrice> productions = FindAllProduction();
        List<TradeProposal> tradeProposals = new List<TradeProposal>();
        foreach (ResourcePrice demand in demands)
        {
            ResourcePrice bestProduction = productions.Where(p => p.Resource.Equals(demand)).FirstOrDefault();
            TradeProposal newTradeProposal = new TradeProposal() { Departure = bestProduction.GameTown, Destination = demand.GameTown, PurchaseValue = bestProduction.Price, SaleValue = demand.Price, Resource = demand.Resource };
            tradeProposals.Add(newTradeProposal);
            productions.Remove(bestProduction);
        }
        activeTradeProposals = tradeProposals;
    }

    private static List<TradeMission> FindAllTradeMissions()
    {
        List<TradeShip> tradeShips = TradeShips.FindAll();
        return tradeShips.SelectMany(s => s.tradeMissions).ToList();
    }

    private List<ResourcePrice> FindAllDemand(List<TradeMission> allCurrentTradeMissions)
    {
        GameTown[] gameTowns = GameWorld.FindObjectsOfType<GameTown>().Where(t => TradeMission.GameTownIsAlly(t)).ToArray();
        List<ResourcePrice> demands = new List<ResourcePrice>();
        foreach (GameTown gameTown in gameTowns)
        {
            foreach(TownResource demand in gameTown.demand)
            {
                int demandCount = demand.count - allCurrentTradeMissions.Where(x => x.Destination.id == gameTown.id && x.ResourceName.Equals(demand.name)).Count();
                if (demandCount >  0)
                {
                    float multi = GameZone.rewardMultiplier;
                    int mPrice = gameTown.GetFinalDemandOffer(demand.name, multi);
                    ResourcePrice resourcePrice = new ResourcePrice() { Resource = demand.name, GameTown = gameTown, Price = mPrice };
                    demands.Add(resourcePrice);
                }
            }
        }
        demands.Sort((d1,d2)=> -1 * d1.Price.CompareTo(d2.Price));
        return demands;
    }

    private List<ResourcePrice> FindAllProduction()
    {
        GameTown[] gameTowns = GameWorld.FindObjectsOfType<GameTown>().Where(t => TradeMission.GameTownIsAlly(t)).ToArray();
        List<ResourcePrice> productions = new List<ResourcePrice>();
        foreach (GameTown gameTown in gameTowns)
        {
            foreach (TownResource production in gameTown.production)
            {
                if (production.count > 0)
                {
                    float multi = GameZone.rewardMultiplier;
                    int mPrice = gameTown.GetFinalProductionOffer(production.name, multi);
                    ResourcePrice resourcePrice = new ResourcePrice() { Resource = production.name, GameTown = gameTown, Price = mPrice };
                    productions.Add(resourcePrice);
                }
            }
        }
        productions.Sort((d1, d2) => d1.Price.CompareTo(d2.Price));

        return productions;
    }

    internal class ResourcePrice
    {
        internal string Resource { get; set; }
        internal GameTown GameTown { get; set; }
        internal int Price { get; set; }
    }
}