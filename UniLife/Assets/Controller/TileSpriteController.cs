using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileSpriteController : MonoBehaviour {

	//This dictionary is used to track which GO is used to render the tileure
	Dictionary<Tile, GameObject> tileGameObjectMap;

	World world {
		get { return WorldController.Instance.World; }
	}


	// Use this for initialization
	void Start () {
		tileGameObjectMap = new Dictionary<Tile, GameObject> ();

		for (int x = 0; x < WorldController.Instance.World.Width; x++) {
			for (int y = 0; y < WorldController.Instance.World.Height; y++) {
				Tile tile_data = WorldController.Instance.World.GetTileAt (x, y);

				GameObject tile_go = new GameObject ();

				tileGameObjectMap.Add (tile_data, tile_go);

				tile_go.name = "Tile_" + x + "_" + y;
				tile_go.transform.position = new Vector3 (tile_data.X, tile_data.Y, 0);
				tile_go.transform.SetParent (this.transform, true);

				SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer> () ;

				tile_data.RegisterTileTypeChangedCallback ((tile)=>{OnTileChanged(tile);});
				OnTileChanged (tile_data);

			}
			WorldController.Instance.World.RegisterTileChanged (OnTileChanged);
		}

	}

	void OnTileChanged (Tile tile){
		if (!tileGameObjectMap.ContainsKey (tile)) {
			Debug.LogError ("Trying to change visuals of a tile not in the list? ");
			return;
		}

		GameObject tile_go = tileGameObjectMap [tile];

		tile_go.GetComponent<SpriteRenderer> ().sprite = GetSpriteForTile (tile);
	}

	Sprite GetSpriteForTile(Tile t){
		string tileType;

		switch (t.Type) {
		case Tile.TileType.OUTSIDE:
			tileType = "grass";
			break;
		case Tile.TileType.FOUNDATION:
			tileType = "concrete";
			break;
		default:
			tileType = "grass";
			break;
		}

		return SpriteManager.current.GetSprite ("Tiles", tileType);

	}
}