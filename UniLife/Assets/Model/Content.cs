using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Content {

    public string Name
    {
        get;
        protected set;
    }

    public int CurrentAmount
    {
        get;
        protected set;
    }

    public int MaxAmount
    {
        get;
        protected set;
    }

    public Content (string name, int currAmount, int maxAmount)
    {
        Name = name;
        CurrentAmount = currAmount;
        MaxAmount = maxAmount;
    }


    public void RestockContent(int amount)
    {
        CurrentAmount += amount;
        if (CurrentAmount > MaxAmount)
            CurrentAmount = MaxAmount;
    }
}
