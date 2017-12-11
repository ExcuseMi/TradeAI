using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


internal class TradeProposal
{
    internal GameTown Departure { get; set; }
    internal GameTown Destination { get; set; }
    internal string Resource { get; set; }
    internal int PurchaseValue { get; set; }
    internal int SaleValue { get; set; }

    internal int GetProfit()
    {
        return PurchaseValue - SaleValue;
    }
}