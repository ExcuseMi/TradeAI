using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Tasharen;
using System.Collections;

public class TradeAI : MonoBehaviour
{
   IEnumerator Start()
   {
      
       for (; ; )
       {
            if(MyPlayer.ship != null) { 
                if(GameShip.list.Count < 10) {
                    string name = "Satan " + GameShip.list.Count;
                    GameShip.CreateRandom("Satan " + GameShip.list.Count, MyPlayer.ship.position, 1f);
                    UIStatusBar.Show("Created " + name);
                    yield return new WaitForFixedUpdate();

                }
                else
                {
                    yield return UIStatusBar.Show("Full");
                }
            } else
            {
                yield return UIStatusBar.Show("Not ingame");
            }
        }
   }

}

