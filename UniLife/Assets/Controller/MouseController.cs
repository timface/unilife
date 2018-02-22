using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

	public GameObject circleCursorPrefab;

	Vector3 dragStartPosition;

	Vector3 lastFramePosition;
	Vector3 currFramePosition;

	List<GameObject> dragSelectionPreviewGOs;

	// Use this for initialization
	void Start () {
		dragSelectionPreviewGOs = new List<GameObject> ();
		Camera.main.transform.position = new Vector3 (WorldController.Instance.World.Height / 2, WorldController.Instance.World.Width / 2, -1);
	}
	
	// Update is called once per frame
	void Update () {
		currFramePosition = (Camera.main.ScreenToWorldPoint (Input.mousePosition) + new Vector3(0.5f, 0.5f, 0));
		currFramePosition.z = -1;

		//UpdateCursor ();
		UpdateDragging ();
		UpdateCamera ();

		lastFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition) + new Vector3(0.5f, 0.5f, 0);
		lastFramePosition.z = -1;
	}

//	void UpdateCursor(){
//		Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoordinate (currFramePosition);
//
//		if (tileUnderMouse != null) {
//			circleCursor.SetActive (true);
//			Vector3 cursorPosition = new Vector3 (tileUnderMouse.X, tileUnderMouse.Y, 0);
//			circleCursor.transform.position = cursorPosition;
//		} else {
//			circleCursor.SetActive (false);
//		}
//	}

	void UpdateCamera(){
		if (Input.GetMouseButton (2) || Input.GetMouseButton (1)) {
			Vector3 diff = lastFramePosition - currFramePosition;
			Camera.main.transform.Translate(diff);
		}

		Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis ("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 3f, 26.5f);
	}

	void UpdateDragging(){

		if (EventSystem.current.IsPointerOverGameObject ()) {
			return;
		}
		if (Input.GetMouseButtonDown (0)) {
			dragStartPosition = currFramePosition;
		}

		int start_x = Mathf.FloorToInt (dragStartPosition.x);
		int end_x = Mathf.FloorToInt (currFramePosition.x);
		if (end_x < start_x) {
			int tmp = end_x;
			end_x = start_x;
			start_x = tmp;
		}

		int start_y = Mathf.FloorToInt (dragStartPosition.y);
		int end_y = Mathf.FloorToInt (currFramePosition.y); 
		if (end_y < start_y) {
			int tmp = end_y;
			end_y = start_y;
			start_y = tmp;
		}

		while (dragSelectionPreviewGOs.Count > 0) {
			GameObject go = dragSelectionPreviewGOs [0];
			dragSelectionPreviewGOs.RemoveAt (0);
			SimplePool.Despawn (go);
		}
		 
		if (Input.GetMouseButton (0)) {
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Tile t = WorldController.Instance.World.GetTileAt (x, y);
					if (t != null) {
						GameObject go = SimplePool.Spawn (circleCursorPrefab, new Vector3 (x, y, 0), Quaternion.identity);
						go.transform.SetParent (this.transform, true);
						dragSelectionPreviewGOs.Add(go);
					}
				}
			}
		}
			
		if (Input.GetMouseButtonUp (0)) {
			GameObject.FindObjectOfType<BuildModeController> ().DoBuild (start_x, start_y, end_x, end_y);
		}
	}
}
