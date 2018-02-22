using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile {

	public enum TileType { OUTSIDE, FOUNDATION};

	Action<Tile> cbTileTypeChanged;

	TileType type = TileType.OUTSIDE;
	public string floor;

	public TileType Type {
		get {
			return type;
		}

		set {
			TileType oldType = type;
			type = value;
			if(cbTileTypeChanged != null && oldType != type)
				cbTileTypeChanged (this);
		}
	}

	const float baseMovementCost = 1;

	public HaulableItem haulableItem;
	public List<Character> Characters;

	public Fixture Fixture {
		get;
		set;
	}
	World world;

	int x;
	int y;

	public int X {
		get { 
			return x;
		}
	}

	public int Y {
		get { 
			return y;
		}
	}

	public float movementCost {
		get {
			if (Type == TileType.OUTSIDE)
				return 2;

			if (Fixture == null)
				return baseMovementCost;

			return baseMovementCost * Fixture.movementCost;
		}
	}

	public Tile (World world, int x, int y, TileType type = TileType.OUTSIDE, string floor = "grass"){
		this.world = world;
		this.x = x;
		this.y = y;
		this.Type = type;
		this.floor = floor;
        this.Characters = new List<Character>();
	}
		
	public void RegisterTileTypeChangedCallback(Action<Tile> callback){
		cbTileTypeChanged += callback;
	}

	public void UnregisterTileTypeChangedCallback(Action<Tile> callback){
		cbTileTypeChanged -= callback;
	}

	public bool AssignInstanceFixture(Fixture objInstance){
		if (objInstance == null) {
			Fixture = null;
			return true;
		}

		if (Fixture != null) {
			Debug.LogError ("Trying to assign an installed object over another");
			return false;
		}

		Fixture = objInstance;
		return true;
	}

    public bool AssignHaulableItem(HaulableItem item)
    {
        if (item == null)
        {
            haulableItem = null;
            return true;
        }

        if (haulableItem != null)
        {
            Debug.LogError("Trying to put a haulableItem where there is one");
            return false;
        }

        haulableItem = item;
        return true;
    }

    public Tile[] GetNeighbours(bool diagOkay = false){
		Tile[] neighbours;

		if (diagOkay)
			neighbours = new Tile[8];
		else
			neighbours = new Tile[4];

		Tile neighbour;

		neighbour = World.Current.GetTileAt (X, Y + 1);
		neighbours [0] = neighbour;
		neighbour = World.Current.GetTileAt (X + 1, Y);
		neighbours [1] = neighbour;
		neighbour = World.Current.GetTileAt (X, Y - 1);
		neighbours [2] = neighbour;
		neighbour = World.Current.GetTileAt (X - 1, Y);
		neighbours [3] = neighbour;

		if (diagOkay) {
			neighbour = World.Current.GetTileAt (X + 1, Y + 1);
			neighbours [4] = neighbour;
			neighbour = World.Current.GetTileAt (X + 1, Y - 1);
			neighbours [5] = neighbour;
			neighbour = World.Current.GetTileAt (X - 1, Y - 1);
			neighbours [6] = neighbour;
			neighbour = World.Current.GetTileAt (X - 1, Y + 1);
			neighbours [7] = neighbour;
		}

		return neighbours;
	}
}
