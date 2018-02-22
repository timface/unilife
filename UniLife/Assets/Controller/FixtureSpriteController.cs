using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FixtureSpriteController : MonoBehaviour {

	//This dictionary is used to track which GO is used to render the fixture
	Dictionary<Fixture, GameObject> fixtureGameObjectMap;

	World world {
		get { return WorldController.Instance.World; }
	}


	// Use this for initialization
	void Start () {
		fixtureGameObjectMap = new Dictionary<Fixture, GameObject> ();

		world.RegisterFixtureCreated (OnFixtureCreated);
		//We do this for any fixtures that may already exist in the world(via loading or otherwise) so that we render those
		foreach (Fixture fixt in world.Fixtures) {
			OnFixtureCreated (fixt);
		}
	}

	public void OnFixtureCreated(Fixture fixt){
		Debug.Log ("Fixture is being created");
		GameObject fixt_go = new GameObject ();

		//Add data and UI to map
		fixtureGameObjectMap.Add (fixt, fixt_go);

		//Set up general info about the GO
		fixt_go.name = fixt.ObjectType + "_" + fixt.Tile.X + "_" + fixt.Tile.Y;
		fixt_go.transform.position = new Vector3 (fixt.Tile.X + ((fixt.Width - 1) / 2f), fixt.Tile.Y + ((fixt.Height - 1) / 2f));
		fixt_go.transform.SetParent (this.transform, true);

		SpriteRenderer sr = fixt_go.AddComponent<SpriteRenderer> ();
		sr.sprite = GetSpriteForFixture (fixt);
		sr.sortingLayerName = "Fixture";



		//TODO: Implement these
		fixt.RegisterOnChanged (OnFixtureChanged);
		fixt.RegisterOnRemoved (OnFixtureRemoved);
	}
		
	public Sprite GetSpriteForFixture(Fixture fixt){
		string spriteName = fixt.ObjectType;
		switch (fixt.Category){
			case "Misc":
				break;
			case "Wall":
				spriteName = GetSpriteNameForWall(fixt);
				break;
			case "Door":
				//TODO: Probs some function related to animating and rotating doors
				break;
			default:
				break;
		}

		//TODO: Implement sprite manager class and fetch the sprite through there
		//return SpriteManager.current.GetSprite("Fixture", spriteName);
		Debug.Log("FSC:: GetSpriteForFixture - " + spriteName);
		return SpriteManager.current.GetSprite("Fixtures", spriteName);
}

	protected string GetSpriteNameForWall(Fixture wall){
		string spriteName = wall.ObjectType;

		if (wall.Tile == null)
			return spriteName;
		
		int x = wall.Tile.X;
		int y = wall.Tile.Y;

		Tile neighbour;

		neighbour = world.GetTileAt (x, y + 1);
		if(neighbour != null && neighbour.Fixture != null && neighbour.Fixture.Category == "Wall"){
			spriteName += "N";
		}
		neighbour = world.GetTileAt (x + 1, y);
		if(neighbour != null && neighbour.Fixture != null && neighbour.Fixture.Category == "Wall"){
			spriteName += "E";
		}
		neighbour = world.GetTileAt (x, y - 1);
		if(neighbour != null && neighbour.Fixture != null && neighbour.Fixture.Category == "Wall"){
			spriteName += "S";
		}
		neighbour = world.GetTileAt (x - 1, y);
		if(neighbour != null && neighbour.Fixture != null && neighbour.Fixture.Category == "Wall"){
			spriteName += "W";
		}

		return spriteName;
	}

	void OnFixtureChanged (Fixture fixt){
		if (!fixtureGameObjectMap.ContainsKey (fixt)) {
			Debug.LogError ("Trying to change visuals of a fixture not in the list? ");
			return;
		}

		GameObject fixt_go = fixtureGameObjectMap [fixt];

		fixt_go.GetComponent<SpriteRenderer> ().sprite = GetSpriteForFixture (fixt);
	}

	void OnFixtureRemoved (Fixture fixt){
		if (!fixtureGameObjectMap.ContainsKey (fixt)) {
			Debug.LogError ("Trying to remove a fixture not in the list? ");
			return;
		}

		GameObject fixt_go = fixtureGameObjectMap [fixt];
		Destroy (fixt_go);
		fixtureGameObjectMap.Remove (fixt);

	}
}