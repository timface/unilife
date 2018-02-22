using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldController : MonoBehaviour {

	public static WorldController Instance { get; protected set;}

	public World World { get; protected set; }

	// Use this for initialization
	void OnEnable () {
		if (Instance != null) {
			Debug.LogError ("There should never be two instances of World controller");
		}
		Instance = this;
		World = new World ();

		//World.RandomiseTiles ();
	}
	
	// Update is called once per frame
	void Update () {
		World.Current.Update (Time.deltaTime);
	}

	public Tile GetTileAtWorldCoordinate (Vector3 coord){
		int x = Mathf.FloorToInt (coord.x);
		int y = Mathf.FloorToInt (coord.y);

		return World.GetTileAt (x, y);
	}

    public void TestItems(string objType)
    {
        World.PlaceHaulableItem("boxofbooks", World.GetTileAt(49, 50));
    }
//	public void OnFixtureCreated(Fixture obj){
//
//		GameObject obj_go = new GameObject ();
//
//		FixtureGameObjectMap.Add (obj, obj_go);
//
//		obj_go.name = obj.ObjectType + "_" + obj.Tile.X + "_" + obj.Tile.Y;
//		obj_go.transform.position = new Vector3 (obj.Tile.X, obj.Tile.Y, 0);
//		obj_go.transform.SetParent (this.transform, true);
//
//		SpriteRenderer sr = obj_go.AddComponent<SpriteRenderer>();
//		sr.sprite = SpriteManager;//FIXME
//		sr.sortingLayerName = "Fixture";
//
//		obj.RegisterOnChanged (OnFixtureChanged);
//	}
//
//	void OnFixtureChanged( Fixture obj){
//		Debug.LogError ("OnFixtureChanged -- not implemented");
//	}
}
