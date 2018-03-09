using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HaulableItem {

    Tile tile;
    public Tile Tile
    {
        get
        {
            return tile;
        }
        protected set
        {
            tile = value;
        }
    }

    string objectType;
    public string ObjectType
    {
        get
        {
            return objectType;
        }
        protected set
        {
            objectType = value;
        }
    }

    int stacksSize;
    public int StackSize
    {
        get { return stacksSize; }
        protected set
        {
            stacksSize = value;
        }
    }
    int requiredStackSize;
    public int RequiredStackSize
    {
        get { return requiredStackSize; }
        protected set
        {
            requiredStackSize = value;
        }
    }

    public Content content;

    Action<HaulableItem> onItemRemoved;

    protected HaulableItem()
    {

    }
    static public HaulableItem CreatePrototype(string objectType, Content content, int stackSize = 0, int requiredStackSize = 0)
    {
        HaulableItem item = new HaulableItem();
        item.ObjectType = objectType;
        item.StackSize = stackSize;
        item.RequiredStackSize = requiredStackSize;
        item.content = content;

        return item;
    }

    static public HaulableItem PlaceHaulableItem(HaulableItem proto, Tile t)
    {
        HaulableItem item = new HaulableItem();
        item.ObjectType = proto.ObjectType;
        item.StackSize = proto.StackSize;
        item.RequiredStackSize = proto.RequiredStackSize;
        item.content = proto.content;

        item.Tile = t;

        if (!t.AssignHaulableItem(item))
        {
            Debug.LogError("HI::PlaceHaulableItem - Couldnt place haulableItem");
            return null;
        }

        return item;
    }

    public void RegisterOnRemoved(Action<HaulableItem> cbfunc)
    {
        onItemRemoved += cbfunc;
    }

    public void OnRemoved(HaulableItem item)
    {
        onItemRemoved(item);
    }
}
