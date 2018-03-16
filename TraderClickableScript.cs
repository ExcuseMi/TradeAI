using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TradeShip))]
class TraderClickableScript : MonoBehaviour
{
    const int CONST_BASE_PRICE = 10;
    const int DEFAULT_CARGO_SLOTS = 2;
    const int PIRATE_CONVERSION_MULTIPLIER = 3;
    const int TRADER_LIMIT = 3;

    void OnClick()
    {
        var tradeShip = GetTradeShip();
        var owner = tradeShip.Owner;
        var gameShip = tradeShip.gameShip;


        if (tradeShip.Owner != null && tradeShip.Owner.id != MyPlayer.id)
        {
            UIStatusBar.Show(Localization.Format("Owned by", gameShip.name, tradeShip.Owner.name));
        }
        else
        {
            UIPopupList popupList = UIGameWindow.popupList;
            popupList.Clear();

            if (owner == null)
            {
                string gold = Format.FormatGold(CalculatePrice());
                popupList.AddItem(Localization.Format("Hire as trader", Format.FormatShip(gameShip), gold), "hire");
                EventDelegate.Set(popupList.onChange, new EventDelegate.Callback(HirePopupCallBack));
            }
            else if (owner.id == MyPlayer.id)
            {
                var profitText = tradeShip.profit >= 0 ? Localization.Format("profit") : Localization.Format("loss");

                UIStatusBar.Show(Localization.Format("Status", Format.FormatShip(gameShip), tradeShip.cargoSlots, profitText, Format.FormatGold(tradeShip.profit)));

                string gold = Format.FormatGold(CalculatePrice());
                popupList.AddItem(Localization.Format("Fire as trader", Format.FormatShip(gameShip)), "fire");
                //popupList.AddItem(Localization.Format("Upgrade for 1000g", gameShip.name), "upgrade");
                EventDelegate.Set(popupList.onChange, new EventDelegate.Callback(FirePopupCallBack));
            }
            popupList.Show();
        }
    }

    private void HirePopupCallBack()
    {
        var tradeShip = GetTradeShip();
        var gameShip = tradeShip.gameShip;

        int price = CalculatePrice();
        if (FindObjectsOfType<TradeShip>().Where(x => x.Owner != null && x.Owner.id == MyPlayer.id).Count() >= TRADER_LIMIT)
        {
            UIStatusBar.Show(Localization.Format("Trader limit", TRADER_LIMIT));
        } else if (MyPlayer.GetResource("gold") >= price)
        {
            MyPlayer.ModifyResource("gold", -price, true);
            MyPlayer.Sync();
            tradeShip.cost = price;
            tradeShip.UpdateTraderOwner(GamePlayer.me.netPlayer.id);
            UIStatusBar.Show(Localization.Format("Hired", Format.FormatShip(gameShip), Format.FormatGold(price), tradeShip.cargoSlots));
        }
        else
        {
            UIStatusBar.Show(Localization.Format("Cant afford to hire", Format.FormatShip(gameShip), Format.FormatGold(price)));
        }
        UIPopupList popupList = UIGameWindow.popupList;
        popupList.Clear();
        EventDelegate.Remove(popupList.onChange, new EventDelegate.Callback(HirePopupCallBack));
    }

    private void FirePopupCallBack()
    {
        var tradeShip = GetTradeShip();
        var gameShip = tradeShip.gameShip;

        int price = CalculatePrice();
        tradeShip.UpdateTraderOwner(null);
        UIPopupList popupList = UIGameWindow.popupList;
        string data = UIPopupList.current.data as string;
        popupList.Clear();
        switch (data)
        {
            case "fire":
                tradeShip.UpdateTraderOwner(null);
                tradeShip.TradeMissions = new List<TradeMission>();
                UIStatusBar.Show(Localization.Format("Fired", Format.FormatShip(gameShip)));
                break;
        }
        EventDelegate.Remove(popupList.onChange, new EventDelegate.Callback(FirePopupCallBack));
    }

    private TradeShip GetTradeShip()
    {
        return GetComponent<TradeShip>();
    }

    private int CalculatePrice()
    {
        var tradeShip = GetTradeShip();
        var gameShip = tradeShip.gameShip;

        return tradeShip.cargoSlots * CONST_BASE_PRICE * (gameShip.factionID == 0 ? PIRATE_CONVERSION_MULTIPLIER : 1);
    }
}
