using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class OriginalShipProperties
{
    public string symbolTex { get; set; }
    public Color sailColor0 { get; set; }
    public Color sailColor1 { get; set; }

    public static OriginalShipProperties Create(GameShip gameShip)
    {
       return new OriginalShipProperties() { sailColor0 = gameShip.sailColor0, sailColor1 = gameShip.sailColor1, symbolTex = gameShip.symbolTex };
    }

    public void Apply(GameShip gameShip)
    {
        gameShip.sailColor0 = sailColor0;
        gameShip.sailColor1 = sailColor1;
        gameShip.symbolTex = symbolTex;
    }
}

