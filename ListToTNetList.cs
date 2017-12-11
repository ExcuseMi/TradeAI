using System;
using System.Linq;
using System.Text;

public static class ListToTNetList<T, TResult>
{
    public static System.Collections.Generic.List<TResult> From(TNet.List<T> list, Func<T, TResult> function)
    {
        if(list == null)
        {
            return new System.Collections.Generic.List<TResult>(0);
        }
        var enumerator = list.GetEnumerator();
        System.Collections.Generic.List<TResult> newList = new System.Collections.Generic.List<TResult>(list.Count);
        while (enumerator.MoveNext())
        {
            newList.Add(function.Invoke(enumerator.Current));
        }
        return newList;
    }

    public static TNet.List<TResult> To(System.Collections.Generic.List<T> list, Func<T, TResult> function)
    {
        if (list == null)
        {
            return new TNet.List<TResult>();
        }
        var enumerator = list.GetEnumerator();
        TNet.List<TResult> newList = new TNet.List<TResult>();
        while (enumerator.MoveNext())
        {
            newList.Add(function.Invoke(enumerator.Current));
        }
        return newList;
    }
}