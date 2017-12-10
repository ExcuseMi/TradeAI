using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TradeMissionComparator : Comparer<TradeMission>
{
    public override int Compare(TradeMission x, TradeMission y)
    {
        return x.StockedUp() ? -1 : y.StockedUp() ? 1 : 0;
    }
}