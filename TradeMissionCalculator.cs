using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

static class TradeMissionCalculator
{
    /**
     * TODO Implement a real Trading algorithm, this works for testing
     * 
     */
    public static TradeMission FindATradeMission()
    {
        GameTown[] gameTowns = GameWorld.FindObjectsOfType<GameTown>();
        if(gameTowns != null && gameTowns.Count() > 0)
        {
            foreach (GameTown gameTown in gameTowns)
            {
                foreach (TownResource production in gameTown.production)
                {
                    if (production.count > 0)
                    {
                        GameTown destination = FindTownThatNeedsResource(gameTowns.Where(x => x.id != gameTown.id), production.name);
                        if (destination != null)
                        {
                            return new TradeMission() { Departure = gameTown, Destination = destination, ResourceName = production.name };
                        }
                    }

                }
            }
        }
        return null;
    }

    public static TradeMission FindATradeMissionForResource(string resourceName)
    {
        GameTown[] gameTowns = GameWorld.FindObjectsOfType<GameTown>();

        if (gameTowns != null && gameTowns.Count() > 0)
        {
            foreach (GameTown gameTown in gameTowns)
            {
                GameTown destination = FindTownThatNeedsResource(gameTowns.Where(x => x.id != gameTown.id), resourceName);
                if (destination != null)
                {
                    return new TradeMission() { Departure = gameTown, Destination = destination, ResourceName = resourceName };
                }
            }

        }
        return null;
    }

    private static GameTown FindTownThatNeedsResource(IEnumerable<GameTown> gameTowns, string resourceName)
    {
        return gameTowns
            .Where(x => x.NeedsResource(resourceName))
            .FirstOrDefault();
    }

}
