using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

static class TradeMissionCalculator
{
    /**
     * TODO Implement a real Trading algorithm, this works for testing
     * 
     */
    public static TradeMission FindATradeMission(Vector3 position)
    {
        GameTown[] gameTowns = GameWorld.FindObjectsOfType<GameTown>();
        Array.Sort(gameTowns, new TownByProximityComparator(position));
        if (gameTowns != null && gameTowns.Count() > 0)
        {
            List<TradeMission> tradeMissions = FindAll();
            foreach (GameTown gameTown in gameTowns)
            {
                foreach (TownResource production in gameTown.production)
                {
                    if (production.count > 0)
                    {
                        if (CanBuy(production.name, gameTown, tradeMissions))
                        {
                            GameTown buyer = FindBuyer(production.name, gameTowns.Where(x => !x.name.Equals(gameTown.name)).ToArray(), tradeMissions);
                            if(buyer != null)
                            {
                                return new TradeMission() { ResourceName = production.name, Departure = gameTown, Destination = buyer };
                            }
                        } 
                    }
                }
            }
        }
        return null;
    }


    public static TradeMission FindATradeMissionForResource(string resourceName, GameTown departure, Vector3 position)
    {
        GameTown[] gameTowns = GameWorld.FindObjectsOfType<GameTown>();
        Array.Sort(gameTowns, new TownByProximityComparator(position));

        if (gameTowns != null && gameTowns.Count() > 0)
        {
            List<TradeMission> tradeMissions = FindAll();

            GameTown buyer = FindBuyer(resourceName, gameTowns.Where(x=>!x.name.Equals(departure.name)).ToArray(), tradeMissions);
            if (buyer != null)
            {
                return new TradeMission() { ResourceName = resourceName, Departure = departure, Destination = buyer };
            }
        }
        return null;
    }

    private static GameTown FindBuyer(string resourceName, GameTown[] gameTowns, List<TradeMission> tradeMissions)
    {
        List<GameTown> destinations = FindTownsThatNeedResource(gameTowns, resourceName);
        foreach (GameTown destination in destinations)
        {
            if (CanSell(resourceName, destination, tradeMissions))
            {
                return destination;
            } else
            {
            }
        }
        return null;
    }

    private static List<GameTown> FindTownsThatNeedResource(IEnumerable<GameTown> gameTowns, string resourceName)
    {
        return gameTowns
            .Where(x => x.NeedsResource(resourceName))
            .ToList();
    }

    private static Boolean CanBuy(string resourceName, GameTown departure, List<TradeMission> allTradeMissionsForResource)
    {
        int production = departure.GetProduction(resourceName);
        production -= allTradeMissionsForResource.Where(m => !m.stockedUp() && m.Departure.name.Equals(departure.name)).Count();
        return production > 0;
    }

    private static Boolean CanSell(string resourceName, GameTown destination, List<TradeMission> allTradeMissionsForResource)
    {
        int demand = destination.GetDemand(resourceName);
        demand -= allTradeMissionsForResource.Where(m => m.Destination.name.Equals(destination.name)).Count();
        return demand > 0;
    }

    private static List<TradeMission> FindAll()
    {
        List<TradeShip> tradeShips = TradeShips.FindAll();
        return tradeShips.SelectMany(s => s.tradeMissions).ToList();
    }
}
