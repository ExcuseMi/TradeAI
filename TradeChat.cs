using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TradeChat
{
    private static Color ORANGE = new Color(213, 168, 454);

    public static void Chat(string message)
    {
        UIGameChat.AddCurrent(message, ORANGE);
    }

    public static void Warn(string message)
    {
        UIGameChat.AddCurrent(message, Color.red);
    }
}
