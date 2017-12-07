using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TradeAIShip : GameShip
{
    private GameShip gameShip;


    public void OnStart()
    {
        this.name = "Satan";
        UIStatusBar.Show("I'm alive?");
    }

    protected void OnFollow(int id, bool canRepair)
    {

    }
    protected void OnNavigate(Vector3 pos, bool active, int priority)
    {

    }
    public void SayRandomEncouragement(GameShip target, float duration)
    {

    }

}
