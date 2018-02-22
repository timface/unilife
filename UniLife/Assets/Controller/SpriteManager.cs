using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class SpriteManager : MonoBehaviour {


	static public SpriteManager current;

	Dictionary<string, Sprite> sprites;
	// Use this for initialization
	void OnEnable () {
		current = this;

		LoadSprites ();
	}
	
	void LoadSprites(){
		sprites = new Dictionary<string, Sprite> ();

		string filePath = System.IO.Path.Combine (Application.streamingAssetsPath, "Sprites");

		LoadSpritesFromDirectory (filePath);
	}

	void LoadSpritesFromDirectory(string filePath){
		//Check to see if there are further sub-folders first and go down into those
		string[] subDirs = Directory.GetDirectories (filePath);
		foreach (string subDir in subDirs) {
			Debug.LogAssertion (subDir);
			LoadSpritesFromDirectory (subDir);
		}

		//No more subDirs, we are now at image files so load em up
		string[] filesInDir = Directory.GetFiles (filePath);
		foreach (string fileName in filesInDir) {
			string spriteCategory = new DirectoryInfo (filePath).Name;
			//Debug.Log ("SpriteManager::LoadSprites " + fileName);
			LoadImage (spriteCategory, fileName);
		}
	}


	void LoadImage(string spriteCategory, string filePath){
		//NOTE: Work out a better way to check for image file maybe
		if (!filePath.Contains (".png") || filePath.Contains(".meta")) {
			return;
		}

		byte[] spriteBytes = System.IO.File.ReadAllBytes (filePath);

		Texture2D spriteTexture = new Texture2D (1, 1);

		if (spriteTexture.LoadImage (spriteBytes)) {

			//Debug.Log ("Created a texture, aboout to sprite" + filePath);
			string baseSpriteName = Path.GetFileNameWithoutExtension (filePath);
			string basePath = Path.GetDirectoryName (filePath);
			string xmlPath = System.IO.Path.Combine (basePath, baseSpriteName + ".xml");

			if (System.IO.File.Exists (xmlPath)) {
				string xmlText = System.IO.File.ReadAllText (xmlPath);

				XmlTextReader reader = new XmlTextReader (new StringReader (xmlText));

				if (reader.ReadToDescendant ("Sprites") && reader.ReadToDescendant ("Sprite")) {
					do {
						ReadSpriteDataFromXml (spriteCategory, reader, spriteTexture);
					} while(reader.ReadToNextSibling ("Sprite"));
				} else {
					Debug.LogError ("Couldnt find a Sprites tag");
					return;
				}
			} else {
				LoadSprite (spriteCategory, baseSpriteName, spriteTexture, new Rect (0, 0, spriteTexture.width, spriteTexture.height), 32);
			}

		}
	}

	void ReadSpriteDataFromXml(string spriteCategory, XmlReader reader, Texture2D spriteTexture){
		string name = reader.GetAttribute ("name");
		int x = int.Parse (reader.GetAttribute ("x"));
		int y = int.Parse (reader.GetAttribute ("y"));
		int w = int.Parse (reader.GetAttribute ("w"));
		int h = int.Parse (reader.GetAttribute ("h"));
		int pixelsPerUnit = int.Parse (reader.GetAttribute ("pixelPerUnit"));

		LoadSprite (spriteCategory, name, spriteTexture, new Rect (x, y, w, h), pixelsPerUnit);
	}

	void LoadSprite(string spriteCategory, string spriteName, Texture2D spriteTexture, Rect spriteCoordinates, int pixelsPerUnit){
		spriteName = spriteCategory + "/" + spriteName;
		Vector2 pivotPoint = new Vector2(0.5f, 0.5f); //Sets the pivot point to Centre

		Sprite sprite = Sprite.Create (spriteTexture, spriteCoordinates, pivotPoint, pixelsPerUnit);

		//Debug.LogAssertion ("Sprite successfully loaded " + spriteName);
		sprites [spriteName] = sprite;
	} 

	public Sprite GetSprite(string categoryName, string spriteName){
		spriteName = categoryName + "/" + spriteName;

		//Debug.Log ("Looking for sprite " + spriteName);
		if (sprites.ContainsKey (spriteName)) {
			//Debug.Log ("Found sprite " + spriteName);
			return sprites [spriteName];
		}

		return null; //TODO: Return an "missing sprite" sprite
	}

}