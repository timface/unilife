using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fixture {

	World world {
		get { return WorldController.Instance.World; }
	}

	Tile tile;
	public Tile Tile {
		get {
			return tile;
		}
		protected set {
			tile = value;
		}
	}

	string objectType;
	public string ObjectType {
		get {
			return objectType;
		}
		protected set {
			objectType = value;
		}
	}

	string category;
	public string Category {
		get { return category; }
		protected set {
			category = value;
		}
	}

	int height;
	public int Height {
		get { return height; }
		protected set {
			height = value;
		}
	}
	int width;
	public int Width {
		get { return width; }
		protected set {
			width = value;
		}
	}

	public float movementCost {
		get;
		protected set;
	}

	bool isDraggable;
	public bool IsDraggable {
		get { return isDraggable; }
		protected set { 
			isDraggable = value;
		}
	}

	public HaulableItem[] requiredHaulableItems {
		get;
		protected set;
	}

	Action<Fixture> fixtureOnRemovedCallback;
	Action<Fixture> fixtureOnChangedCallback;

	Func<Fixture, Tile, bool> funcPositionValidation;

    protected Dictionary<string, System.Object> fixtParameters; //use this dictionary for storing contents or increase <character value> amounts

    float jobCreateCooldown = 0f;

	//TODO: Implement object rotation

	protected Fixture(){
	
	}

	static public Fixture CreatePrototype(string objectType, float movementCost = 1f, int width  = 1, int height  = 1, string category = "Misc", bool isDraggable = false, HaulableItem[] haulItems = null) {
		Fixture obj = new Fixture ();
		obj.objectType = objectType;
		obj.movementCost = movementCost;
		obj.width = width;
		obj.height = height;
		obj.Category = category;
		obj.IsDraggable = isDraggable;
		obj.funcPositionValidation = DEFAULT_PositionValidater;
		obj.requiredHaulableItems = haulItems;
        obj.fixtParameters = new Dictionary<string, System.Object>();

		return obj;
	}

	static public Fixture PlaceInstance( Fixture proto, Tile tile ){
		Fixture obj = new Fixture ();
		obj.objectType = proto.objectType;
		obj.movementCost = proto.movementCost;
		obj.width = proto.width;
		obj.height = proto.height;
		obj.Category = proto.category;
        obj.fixtParameters = proto.fixtParameters;

		obj.tile = tile;


        for (int xOffset = tile.X; xOffset < (tile.X + obj.Width); xOffset++)
        {
            for (int yOffset = tile.Y; yOffset < (tile.Y + obj.Height); yOffset++)
            {
                Tile offsetTile = WorldController.Instance.World.GetTileAt(xOffset, yOffset);
                if (!offsetTile.AssignInstanceFixture(obj))
                {
                    Debug.LogError("Seems to be an issue with tiles not being assigned callbacks");
                    return null;
                }
            }
        }

		//Check for walls near by and tell them to update their shit
		if (obj.Category == "Wall") {
			int x = tile.X;
			int y = tile.Y;
			Tile neighbour;

			neighbour =  WorldController.Instance.World.GetTileAt (x, y + 1);
			if(neighbour != null && neighbour.Fixture != null && neighbour.Fixture.Category == "Wall"){
				if (neighbour.Fixture.fixtureOnChangedCallback != null)
					neighbour.Fixture.fixtureOnChangedCallback (neighbour.Fixture);
			}
			neighbour = WorldController.Instance.World.GetTileAt (x + 1, y);
			if(neighbour != null && neighbour.Fixture != null && neighbour.Fixture.Category == "Wall"){
				if (neighbour.Fixture.fixtureOnChangedCallback != null)
					neighbour.Fixture.fixtureOnChangedCallback (neighbour.Fixture);
			}
			neighbour = WorldController.Instance.World.GetTileAt (x, y - 1);
			if(neighbour != null && neighbour.Fixture != null && neighbour.Fixture.Category == "Wall"){
				if (neighbour.Fixture.fixtureOnChangedCallback != null)
					neighbour.Fixture.fixtureOnChangedCallback (neighbour.Fixture);
			}
			neighbour = WorldController.Instance.World.GetTileAt (x - 1, y);
			if(neighbour != null && neighbour.Fixture != null && neighbour.Fixture.Category == "Wall"){
				if (neighbour.Fixture.fixtureOnChangedCallback != null)
					neighbour.Fixture.fixtureOnChangedCallback (neighbour.Fixture);
			}
		}

		return obj;
	}

    public void Update(float deltaTime)
    {
        //Debug.Log(fixtParameters["contents"] + " " + fixtParameters["contentCurrAmount"] + " " + fixtParameters["contentMaxAmount"]);
        if (jobCreateCooldown <= 0f)
        {
            if (fixtParameters.ContainsKey("contents") && fixtParameters.ContainsKey("contentCurrAmount") && fixtParameters.ContainsKey("contentMaxAmount")
                && (int)fixtParameters["contentCurrAmount"] < (int)fixtParameters["contentMaxAmount"])
            {

                if (!world.estateJobManager.IsTileReserved(this.tile))
                {
                    PathAstar pathToItem = new PathAstar(world, this.tile, null, "books");
                    Debug.Log("pathToItem is long: " + pathToItem.Length());
                    if (pathToItem != null && pathToItem.Length() > 0)
                    {
                        Debug.Log("Fixture::Update -- we've got a path to an item");
                        Tile tile = pathToItem.EndTile();
                        world.estateJobManager.AddJob(new EstateJob(this.tile, EstateJobType.HAUL, 1, Character.DeliverGoods, new Vector2(0, -1), this, tile));
                    }
                    else
                        Debug.LogError("We cant find an item for this job, aborting"); //TODO: Implement some sort of goods ordering. 

                    jobCreateCooldown = 5f;
                }
            }
        }

        jobCreateCooldown -= deltaTime;
    }

    public void SetParameter(string key, System.Object value)
    {
        fixtParameters.Add(key, value);
        //Debug.Log("Current Keys: " + fixtParameters.Keys.ToString());
    }

    public System.Object GetParameter(string key)
    {
        return fixtParameters[key];
    }

    public void RecieveContents(string content, int amount)
    {
        if (!fixtParameters.ContainsKey(content))
            Debug.LogError("Fixture::RecieveGoods - Trying to put contents into a fixture that dont support this type: " + content);

        int currAmount = (int)fixtParameters["contentCurrAmount"];
        currAmount += amount;

        fixtParameters["contentCurrAmount"] = currAmount;
    }

	public void RegisterOnChanged (Action<Fixture> cbFunc){
		fixtureOnChangedCallback += cbFunc;
	}

	public void UnregisterOnChanged (Action<Fixture> cbFunc){
		fixtureOnChangedCallback -= cbFunc;
	}

	public void RegisterOnRemoved (Action<Fixture> cbFunc){
		fixtureOnRemovedCallback += cbFunc;
	}

	public void UnregisterOnRemoved (Action<Fixture> cbFunc){
		fixtureOnRemovedCallback -= cbFunc;
	}

	public bool IsValidPosition(Fixture fixt, Tile t){
		return funcPositionValidation (fixt, t);
	}

	protected static bool DEFAULT_PositionValidater(Fixture fixt, Tile t){
		for (int xOffset = t.X; xOffset < (t.X + fixt.Width); xOffset++) {
			for (int yOffset = t.Y; yOffset < (t.Y + fixt.Height); yOffset++) {
				Tile offsetTile = WorldController.Instance.World.GetTileAt (xOffset, yOffset);

				if (offsetTile.Type == Tile.TileType.OUTSIDE)
					return false;

                if (offsetTile.Fixture != null)
                    return false;

                if (WorldController.Instance.World.estateJobManager.IsTileReserved(offsetTile))
                    return false;

			}
		}
		return true;
	}
}
