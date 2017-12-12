using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class QuestHireTrader : Quest
{
    protected override void OnEnable()
    {
        base.OnEnable();
        GameUnit.onKill += new GameUnit.UnitCallback(this.OnKill);
        MyPlayer.target = this.mTargetUnit;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameUnit.onKill -= new GameUnit.UnitCallback(this.OnKill);
    }

    private void OnKill(GameUnit mUnit)
    {
        if (!((UnityEngine.Object)this.mTargetUnit == (UnityEngine.Object)mUnit) || !this.tno.isMine)
            return;
        this.Complete(false);
    }

    protected override void OnInit()
    {
        activeBonuses.Clear();
        activeBonuses.Add(Bonus.Trader);

        if ((UnityEngine.Object)MyPlayer.ownedShip == (UnityEngine.Object)this.mUnit)
        {
            if (!QuestPirate.FindSuitableSpawn(this.mUnit.position, new PFQuery.OnPathCallback(this.CreateTrader)))
            {
                TradeChat.DebugQuest("Not suitable spawn");
                this.Complete(false);
                return;
            }
            this.InvokeRepeating("FindTrader", 0.5f, 0.1f);

        } else
        {
            TradeChat.DebugQuest("Not my unnit");

        }
        this.StartCoroutine("MainLogicSD");
    }

    private void FindTrader()
    {
        var possibleTrader = GameWorld.FindObjectsOfType<GameShip>().Where(x => x.name.Equals(target)).FirstOrDefault();
        if (possibleTrader != null)
        {
            TradeShip traderShip = possibleTrader.GetComponent<TradeShip>();
            if (traderShip == null)
            {
                possibleTrader.gameObject.AddMissingComponent<TradeShip>().Activate();
            } 
            this.mTargetUnit = possibleTrader;
            MyPlayer.target = possibleTrader;
            this.target = possibleTrader.name;
            this.CancelInvoke(nameof(FindTrader));
        } else
        {
            TradeChat.DebugQuest("No trader found");
        }
        return;
    }

    public void Track()
    {
        if (!this.isValid)
            return;
        MyPlayer.target = this.mTargetUnit;
    }

    private void CreateTrader(PFQuery query)
    {
        Vector3 pos = query.GetNode(query.nodeCount - 26).pos;
        pos.y = 0.0f;
        Vector3 forward = query.end - pos;
        forward.y = 0.0f;
        Pathfinding.instance.RecycleQuery(query);
        target = GameConfig.GenerateRandomName();
        CreateTrader(target, pos, Quaternion.LookRotation(forward).eulerAngles.y, 0, 1.5f, 0.75f, 0);
    }

    public void CreateTrader(string shipName, Vector3 pos, float angle, int extraChallenge = 0, float hp = 1f, float damage = 1f, int boss = 0)
    {
        int level;
        string shipType = GameShip.ChooseRandomShip(shipName, extraChallenge, boss, out level, false);
        GameShip.Create(shipName, shipType, MyPlayer.factionID, pos, angle, level, hp, damage, boss);

    }

    private IEnumerator MainLogicSD()
    {
         for (; ;  )
        {

            if (mTargetUnit != null)
            {
                GameShip gameShip = (GameShip)mTargetUnit;
                TradeChat.DebugQuest("Checking " + gameShip.name);
                if (gameShip.gameObject.GetComponent<TradeShip>() != null)
                {
                    if (gameShip.gameObject.GetComponent<TradeShip>().Owner != null)
                    {
                        if (gameShip.gameObject.GetComponent<TradeShip>().Owner.id == GamePlayer.me.netPlayer.id)
                        {

                            Complete(true);
                        }
                        else
                        {
                            UIStatusBar.Show(Localization.Format("Hired by someone else", gameShip.gameObject.GetComponent<TradeShip>().Owner.name, gameShip.name));
                            Complete(false);
                        }
                    } else
                    {
                        TradeChat.DebugQuest("No owner");
                    }
                }
                else
                {
                    gameShip.gameObject.AddMissingComponent<TradeShip>().Activate();
                }
            } else
            {
                TradeChat.DebugQuest("No target");
            }

            yield return new WaitForSeconds(1);
        }
    }

    protected override void OnComplete(bool success)
    {
        this.StopCoroutine("MainLogicSD");
        base.OnComplete(success);
    }
}