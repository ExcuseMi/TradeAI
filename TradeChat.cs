using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TradeChat
{
    private static Color ORANGE = new Color(213, 168, 154);
    private static Color GREEN = new Color(111, 206, 101);

    public static void Chat(string message)
    {
        UIGameChat.AddCurrent(message, GREEN);
    }

    public static void Bad(string message)
    {
        UIGameChat.AddCurrent(message, ORANGE);
    }

    public static void Warn(string message)
    {
        UIGameChat.AddCurrent(message, Color.red);
    }

    public static void DebugQuest(string message)
    {
        UIGameChat.AddCurrent(message, Color.red);
    }

    public static void DebugTradeShip(string message)
    {
        UIGameChat.AddCurrent(message, Color.cyan);
    }
}
