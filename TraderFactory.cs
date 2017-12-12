using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TradeShipFactory : MonoBehaviour
{
    string shipName;
    string shipType;
    TNet.Player owner;

    public TradeShip tradeShip = null;

    public static TradeShipFactory Create(string shipName, string shipType, TNet.Player owner = null)
    {
        TradeShipFactory traderFactory = new TradeShipFactory();
        traderFactory.shipType = shipType;
        traderFactory.shipName = shipName;
        traderFactory.owner = owner;

        QuestPirate.FindSuitableSpawn(MyPlayer.ship.position, new PFQuery.OnPathCallback(traderFactory.CreateTrader));
        traderFactory.InvokeRepeating("FindTrader", 0.5f, 0.1f);
        return traderFactory;
    }


    private void FindTrader()
    {
        var possibleTrader = GameWorld.FindObjectsOfType<GameShip>().Where(x => x.name.Equals(shipName)).FirstOrDefault();
        if (possibleTrader != null)
        {
            TradeShip traderShip = possibleTrader.GetComponent<TradeShip>();
            if (traderShip == null)
            {
                var tradeShip = possibleTrader.gameObject.AddMissingComponent<TradeShip>();
                if (owner == null)
                {
                    tradeShip.Activate();
                }
                else
                {
                    tradeShip.Activate(owner);
                }
                this.tradeShip = tradeShip;
                this.CancelInvoke(nameof(FindTrader));
            }
        }
    }

    private void CreateTrader(PFQuery query)
    {
        Vector3 pos = query.GetNode(query.nodeCount - 26).pos;
        pos.y = 0.0f;
        Vector3 forward = query.end - pos;
        forward.y = 0.0f;
        Pathfinding.instance.RecycleQuery(query);

        GameShip.Create(shipName, shipType, MyPlayer.factionID, pos, Quaternion.LookRotation(forward).eulerAngles.y, 0, 1.5f, 0.75f, 0);
    }
}