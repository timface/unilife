using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CharacterSpriteController : MonoBehaviour {

	//This dictionary is used to track which GO is used to render the fixture
	Dictionary<Character, GameObject> characterGameObjectMap;

	World world {
		get { return WorldController.Instance.World; }
	}


	// Use this for initialization
	void Start () {
		characterGameObjectMap = new Dictionary<Character, GameObject> ();

		world.RegisterCharacterCreated (OnCharacterCreated);
		//We do this for any fixtures that may already exist in the world(via loading or otherwise) so that we render those
		foreach (Character cha in world.Characters) {
            Debug.Log("Character: " + cha.CharRole);
			OnCharacterCreated (cha);
		}
	}

	public void OnCharacterCreated(Character cha){
		Debug.Log ("Character is being created");
		GameObject cha_go = new GameObject ();

		//Add data and UI to map
		characterGameObjectMap.Add (cha, cha_go);

		//Set up general info about the GO
		cha_go.name = cha.CharRole.ToString();
		cha_go.transform.position = new Vector3 (cha.X, cha.Y, 0f);
		cha_go.transform.SetParent (this.transform, true);

		SpriteRenderer sr = cha_go.AddComponent<SpriteRenderer> ();
		sr.sprite = GetSpriteForCharacter (cha);
		sr.sortingLayerName = "Character";



		//TODO: Implement these
		cha.RegisterOnChangeCallback (OnCharacterChanged);
		//cha.RegisterOnRemoved (OnCharacterRemoved);
	}

	void OnCharacterChanged (Character cha){
		if (!characterGameObjectMap.ContainsKey (cha)) {
			Debug.LogError ("Trying to change visuals of a Character not in the list? ");
			return;
		}

		GameObject cha_go = characterGameObjectMap [cha];

		cha_go.transform.position = new Vector3 (cha.X, cha.Y, 0f);
	}

	Sprite GetSpriteForCharacter(Character cha){
		//TODO: Implement switch to return char sprite based on role and probably some rando thing for students.
		//switch(cha.CharRole){
		//case CharacterRole.BUILDER:

		return SpriteManager.current.GetSprite ("Characters", "builder");
	}

	void OnCharacterRemoved (Character cha){
		if (!characterGameObjectMap.ContainsKey (cha)) {
			Debug.LogError ("Trying to remove a fixture not in the list? ");
			return;
		}

		GameObject cha_go = characterGameObjectMap [cha];
		Destroy (cha_go);
		characterGameObjectMap.Remove (cha);

	}
}