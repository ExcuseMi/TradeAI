using System;
using UnityEngine;

static class Format
{
    public static string FormatGold(int gold)
    {
        return GameTools.FormatGold(gold) + "g";
    }

    public static String FormatTown(GameTown gameTown)
    {
        return FormatFactionColor(gameTown.name, gameTown.factionID);
    }

    public static String FormatShip(GameShip gameShip)
    {
        return FormatFactionColor(gameShip.name, gameShip.factionID);
    }

    public static String FormatFactionColor(String text, int factionId)
    {
        return "[" + NGUIText.EncodeColor(factionId != 0 ? GameConfig.GetFactionColor(factionId) : Color.white) + "]" + text + "[-]";
    }

}
