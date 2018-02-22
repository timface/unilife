using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HaulableItemSpriteController : MonoBehaviour {

	//This dictionary is used to track which GO is used to render the fixture
	Dictionary<HaulableItem, GameObject> haulableGameObjectMap;

	World world {
		get { return WorldController.Instance.World; }
	}


	// Use this for initialization
	void Start () {
		haulableGameObjectMap = new Dictionary<HaulableItem, GameObject> ();

		world.RegisterHaulableItemCreated (OnHaulableItemCreated);
		//We do this for any fixtures that may already exist in the world(via loading or otherwise) so that we render those
		foreach (HaulableItem item in world.HaulableItems) {
			OnHaulableItemCreated (item);
		}
	}

	public void OnHaulableItemCreated(HaulableItem item){
		Debug.Log ("Item is being created");
		GameObject item_go = new GameObject ();

		//Add data and UI to map
		haulableGameObjectMap.Add (item, item_go);

		//Set up general info about the GO
		item_go.name = item.ObjectType + "_" + item.Tile.X + "_" + item.Tile.Y;
		item_go.transform.position = new Vector3 (item.Tile.X, item.Tile.Y);
		item_go.transform.SetParent (this.transform, true);

		SpriteRenderer sr = item_go.AddComponent<SpriteRenderer> ();
		sr.sprite = GetSpriteForHaulableItem (item);
		sr.sortingLayerName = "Fixture";



		//TODO: Implement these
		//item.RegisterOnChanged (OnFixtureChanged);
	    item.RegisterOnRemoved (OnHaulableItemRemoved);
	}
		
	public Sprite GetSpriteForHaulableItem(HaulableItem item){
		string spriteName = item.ObjectType;


		//TODO: Implement sprite manager class and fetch the sprite through there
		//return SpriteManager.current.GetSprite("Fixture", spriteName);
		Debug.Log("FSC:: GetSpriteForItem - " + spriteName);
		return SpriteManager.current.GetSprite("HaulableItems", spriteName);
}

	void OnHaulableItemChanged (HaulableItem item){
		if (!haulableGameObjectMap.ContainsKey (item)) {
			Debug.LogError ("Trying to change visuals of a fixture not in the list? ");
			return;
		}

		GameObject fixt_go = haulableGameObjectMap [item];

		fixt_go.GetComponent<SpriteRenderer> ().sprite = GetSpriteForHaulableItem (item);
	}

	void OnHaulableItemRemoved (HaulableItem item){
		if (!haulableGameObjectMap.ContainsKey (item)) {
			Debug.LogError ("Trying to remove an item not in the list? ");
			return;
		}

		GameObject fixt_go = haulableGameObjectMap [item];
		Destroy (fixt_go);
		haulableGameObjectMap.Remove (item);
        world.HaulableItems.Remove(item);
	}
}