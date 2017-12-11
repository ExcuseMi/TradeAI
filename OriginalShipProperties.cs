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

    public TNet.DataNode Convert()  
    {
        TNet.DataNode dataNode = new TNet.DataNode("OriginalShipProperties");
        dataNode.AddChild("sailColor0", sailColor0);
        dataNode.AddChild("sailColor1", sailColor1);
        dataNode.AddChild("symbolTex", symbolTex);

        return dataNode;
    }

    public static OriginalShipProperties FromData(TNet.DataNode dataNode)
    {
        return new OriginalShipProperties() { sailColor0 = dataNode.GetChild<Color>("sailColor0"), sailColor1 = dataNode.GetChild<Color>("sailColor1"), symbolTex = dataNode.GetChild<string>("symbolTex") };

    }
}

