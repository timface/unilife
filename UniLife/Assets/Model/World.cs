using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World {
	Tile[,] tiles;

	public EstateJobManager estateJobManager;

	Dictionary<string, Fixture> FixturePrototypes;
	List<Fixture> _fixtures;
	public List<Fixture> Fixtures {
		get { return _fixtures; }
		protected set { _fixtures = value; }
	}

    Dictionary<string, HaulableItem> HaulableItemPrototypes;
    List<HaulableItem> _haulableItems;
    public List<HaulableItem> HaulableItems
    {
        get { return _haulableItems; }
        protected set { _haulableItems = value; }
    }

    List<Character> _characters;
	public List<Character> Characters {
		get { return _characters; }
		protected set { _characters = value; }
	}

	Action<Fixture> cbFixtureCreated;
	Action<Tile> onTileChanged;
	Action<EstateJob> onJobCreated;
	Action<Character> onCharacterCreated;
    Action<HaulableItem> onHaulableItemCreated;

    public PathTileGraph tileGraph;

	int width;
	int height;

	public int Width {
		get {
			return width;
		}
	}

	public int Height {
		get {
			return height;
		}
	}

	static public World Current { get; protected set; }
	public World (int width = 100, int height = 100){
		SetupWorld (width, height);
	}

	void SetupWorld(int width, int height){
		estateJobManager = new EstateJobManager ();

		Current = this;

		this.Fixtures = new List<Fixture> ();
		this.Characters = new List<Character> ();
        this.HaulableItems = new List<HaulableItem>();
		this.width = width;
		this.height = height;

		tiles = new Tile[width, height];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				tiles [x, y] = new Tile (this, x, y);
			}
		}

		Debug.Log ("World created with " + (width * height) + " tiles!");

		CreateFixturePrototypes ();
        CreateHaulableItemPrototypes();
        Character cha = new Character(Current.GetTileAt(50, 50), CharacterRole.BUILDER);
        Characters.Add(cha);

        //PlaceHaulableItem("boxofbooks", Current.GetTileAt(49, 50));
        //Current.onCharacterCreated(cha);

        
	}

	public void Update(float deltaTime){
		foreach (Character cha in Characters) {
			cha.Update (deltaTime);
		}

        foreach(Fixture fixt in Fixtures)
        {
            fixt.Update(deltaTime);
        }
	}

	void CreateFixturePrototypes(){
		FixturePrototypes = new Dictionary<string, Fixture> ();

		FixturePrototypes.Add ("wall", Fixture.CreatePrototype ("wall", 0, 1, 1, "Wall", true));

        Fixture bookShelfProto = Fixture.CreatePrototype("bookshelf", 0, 2, 1, "Misc", true);
        bookShelfProto.SetParameter("contents", "books");
        bookShelfProto.SetParameter("contentCurrAmount", 0);
        bookShelfProto.SetParameter("contentMaxAmount", 50);
        FixturePrototypes.Add("bookshelf", bookShelfProto);
	}

    void CreateHaulableItemPrototypes()
    {
        HaulableItemPrototypes = new Dictionary<string, HaulableItem>();

        HaulableItemPrototypes.Add("boxofbooks", HaulableItem.CreatePrototype("boxofbooks", "books", 10));
    }

	public Tile GetTileAt (int x, int y){
		if (x >= Width || x < 0 || y >= Height || y < 0) {
			//Debug.LogError ("Tile: " + x + " : " + y + " does not exist");
			return null;
		}else 
			return tiles [x, y];
	}

	public void PlaceFixture(string objType, Tile tile){
		//TODO: This function assumes 1x1 no rotation objects

		if (!FixturePrototypes.ContainsKey(objType)){
			Debug.LogError ("FixtureProtoypes does not contain a definition for: " + objType);
			return;
		}

		Fixture obj = Fixture.PlaceInstance (FixturePrototypes [objType], tile);

		if (obj == null) {
			Debug.LogError ("World:PlaceInstance returned a null object");
			return;
		}
        Fixtures.Add(obj);
        InvalidateTileGraph();
		if (cbFixtureCreated != null) {
			cbFixtureCreated (obj);
		}
	}

    public void PlaceHaulableItem(string objType, Tile tile)
    {
        if (!HaulableItemPrototypes.ContainsKey(objType))
        {
            Debug.LogError("HaulableItemPrototypes does not contain a definition for: " + objType);
            return;
        }

        HaulableItem obj = HaulableItem.PlaceHaulableItem(HaulableItemPrototypes[objType], tile);

        if (obj == null)
        {
            Debug.LogError("World:PlaceHaulableItem returned a null object");
            return;
        }

        HaulableItems.Add(obj);
        if (onHaulableItemCreated != null)
            onHaulableItemCreated(obj);
    }

	void OnTileChanged(Tile t) {
		if(onTileChanged != null)
			onTileChanged(t);

		InvalidateTileGraph(); //This is pathfinding stuff
	}

	void InvalidateTileGraph(){
		tileGraph = null;
	}

	public bool IsFixturePlacementValid(string fixtureType, Tile t){
		return FixturePrototypes [fixtureType].IsValidPosition (FixturePrototypes[fixtureType], t);
	}

	public Fixture GetFixturePrototype(string proto){
		if (FixturePrototypes.ContainsKey (proto)) {
			return FixturePrototypes [proto];
		}
		Debug.LogError ("World::GetFixturePrototypes couldnt find a prototype");
		return null;
	}

	public void RegisterFixtureCreated( Action<Fixture> cbFunc){
		cbFixtureCreated += cbFunc;
	}

	public void UnregisterFixtureCreated( Action<Fixture> cbFunc){
		cbFixtureCreated -= cbFunc;
	}

	public void RegisterTileChanged( Action<Tile> cbFunc){
		onTileChanged += cbFunc;
	}

	public void RegisterJobCreated(Action<EstateJob> cbFunc){
		onJobCreated += cbFunc;
	}

	public void RegisterCharacterCreated( Action<Character> cbFunc){
		onCharacterCreated += cbFunc;
	}

	public void UnregisterCharacterCreated( Action<Character> cbFunc){
		onCharacterCreated -= cbFunc;
	}

    public void RegisterHaulableItemCreated(Action<HaulableItem> cbFunc)
    {
        onHaulableItemCreated += cbFunc;
    }

    public void UnregisterHaulableItemCreated(Action<HaulableItem> cbFunc)
    {
        onHaulableItemCreated -= cbFunc;
    }
}
