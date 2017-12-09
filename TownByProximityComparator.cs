using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TownByProximityComparator : IComparer<GameTown>
{
    private Vector3 position;

    public TownByProximityComparator(Vector3 position)
    {
        this.position = position;
    }

    public int Compare(GameTown x, GameTown y)
    {
        return GetDistance(x).CompareTo(GetDistance(y));
    }

    private int GetDistance(GameTown gameTown)
    {
        Vector3 townPosition = gameTown.dockPos;
        return (int) Math.Round(Math.Abs(position.x - townPosition.x) + Math.Abs(position.y - townPosition.y) + Math.Abs(position.z - townPosition.z));
    }
}
