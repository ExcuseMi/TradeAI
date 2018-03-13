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
    public static List<TradeMission> FindTradeMissions(GameShip gameShip, int howMany)
    {
        GameTown[] gameTowns = GameWorld.FindObjectsOfType<GameTown>().Where(t=> TradeShip.GameTownIsAlly(gameShip, t)).ToArray();
        Array.Sort(gameTowns, new TownByProximityComparator(gameShip.position));
        List<TradeMission> newTradeMissions = new List<TradeMission>();
        if (gameTowns != null && gameTowns.Count() > 0)
        {
            List<TradeMission> tradeMissions = FindAll();
            foreach (GameTown gameTown in gameTowns)
            {
                foreach (TownResource production in gameTown.production)
                {
                    if (production.count > 0)
                    {
                        var allCurrentTradeMissions = new List<TradeMission>(newTradeMissions.Count + tradeMissions.Count);
                        allCurrentTradeMissions.AddRange(newTradeMissions);
                        allCurrentTradeMissions.AddRange(tradeMissions);

                        int count = HowManyCanWeBuy(production.name, gameTown, tradeMissions);
                        if (count > 0)
                        {

                            GameTown buyer = FindBuyer(production.name, gameTowns.Where(x => !x.name.Equals(gameTown.name)).ToArray(), allCurrentTradeMissions);
                            while (count > 0 && buyer != null)
                            {
                                newTradeMissions.Add(new TradeMission() { ResourceName = production.name, Departure = gameTown.id, Destination = buyer.id });
                                if (newTradeMissions.Count() >= howMany)
                                {
                                    return newTradeMissions;
                                }
                                count--;
                                allCurrentTradeMissions = new List<TradeMission>(newTradeMissions.Count + tradeMissions.Count);
                                allCurrentTradeMissions.AddRange(newTradeMissions);
                                allCurrentTradeMissions.AddRange(tradeMissions);
                                buyer = FindBuyer(production.name, gameTowns.Where(x => !x.name.Equals(gameTown.name)).ToArray(), allCurrentTradeMissions);
                            }
                        } 
                    }
                }
            }
        }
        return newTradeMissions;
    }


    public static TradeMission FindATradeMissionForResource(string resourceName, GameTown departure, Vector3 position)
    {
        GameTown[] gameTowns = GameWorld.FindObjectsOfType<GameTown>().Where(t => t.factionID != 0).ToArray();
        Array.Sort(gameTowns, new TownByProximityComparator(position));

        if (gameTowns != null && gameTowns.Count() > 0)
        {
            List<TradeMission> tradeMissions = FindAll();

            GameTown buyer = FindBuyer(resourceName, gameTowns.Where(x=>!x.name.Equals(departure.name)).ToArray(), tradeMissions);
            if (buyer != null)
            {
                return new TradeMission() { ResourceName = resourceName, Departure = departure.id, Destination = buyer.id };
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

    private static int HowManyCanWeBuy(string resourceName, GameTown departure, List<TradeMission> allTradeMissionsForResource)
    {
        if(departure == null)
        {
            return 0;
        }
        int production = departure.GetProduction(resourceName);
        production -= allTradeMissionsForResource.Where(m => !m.StockedUp() && m.Departure.Equals(departure.id)).Count();
        return production;
    }

    private static Boolean CanSell(string resourceName, GameTown destination, List<TradeMission> allTradeMissionsForResource)
    {
        int demand = destination.GetDemand(resourceName);
        demand -= allTradeMissionsForResource.Where(m => m.Destination.Equals(destination.id)).Count();
        return demand > 0;
    }

    private static List<TradeMission> FindAll()
    {
       return FindAllTraders().SelectMany(x => x.TradeMissions).ToList();
    }

    public static List<TradeShip> FindAllTraders()
    {
        return GameWorld.FindObjectsOfType<TradeShip>().Where(x=> x.Owner != null && x.Owner.id == MyPlayer.id).ToList();
    }
}
