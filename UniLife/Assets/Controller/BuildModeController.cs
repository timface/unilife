using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public enum BuildMode {
	FOUNDATION,
	FIXTURE,
	FLOOR,
	DECONSTRUCT
}

public class BuildModeController : MonoBehaviour {

	World world {
		get { return WorldController.Instance.World; }
	}

	public BuildMode buildMode = BuildMode.FOUNDATION;
	Tile.TileType buildModeTile;
	string buildModeObjectType;

	public bool IsObjectDraggable(){
		if (buildMode != BuildMode.FIXTURE) {
			return true;
		} else
			return world.GetFixturePrototype(buildModeObjectType).IsDraggable;
	}

	public void SetModeBuildFloor(){
		buildMode = BuildMode.FLOOR;
		buildModeTile = Tile.TileType.FOUNDATION;
	}

	public void SetModeBulldoze(){
		buildMode = BuildMode.DECONSTRUCT;
		buildModeTile = Tile.TileType.OUTSIDE;
	}

	public void SetModeBuildFixture(string objType){
		buildMode = BuildMode.FIXTURE;
		buildModeObjectType = objType;
	}

	public void SetModeBuildFoundations(string wallType){
		buildMode = BuildMode.FOUNDATION;
		buildModeObjectType = wallType;
		buildModeTile = Tile.TileType.FOUNDATION;
	}

	public void DoBuild (int startX, int startY, int endX, int endY) {

		Debug.Log ("Doing building");
		for (int x = startX; x <= endX; x++) {
			for (int y = startY; y <= endY; y++) {
				Tile t = WorldController.Instance.World.GetTileAt (x, y);
				if (t != null) {
					if (buildMode == BuildMode.FIXTURE) {
						//Create objs
						//FIXME: Instantly does stuff, no jobs yet 
						if (world.IsFixturePlacementValid (buildModeObjectType, t)) {
							world.estateJobManager.AddJob (new EstateJob (t, EstateJobType.BUILD, 1, CompleteBuildJob, world.GetFixturePrototype (buildModeObjectType)));
						}
					} else if (buildMode == BuildMode.FOUNDATION){
						if (t.Type != Tile.TileType.FOUNDATION) {
							t.Type = Tile.TileType.FOUNDATION;
							if (WorldController.Instance.World.IsFixturePlacementValid (buildModeObjectType, t)) {
								if (x == startX || x == endX || y == startY || y == endY) {
									world.estateJobManager.AddJob (new EstateJob (t, EstateJobType.BUILD, 1, CompleteBuildJob, world.GetFixturePrototype (buildModeObjectType)));
								} else {
									t.Type = buildModeTile;
								}
							}
						}
					} else {
						t.Type = buildModeTile;
					}
				}
			}
		}
	}

    public static void CompleteBuildJob(EstateJob doneJob)
    {
        WorldController.Instance.World.PlaceFixture(doneJob.Fixture.ObjectType, doneJob.jobTile); 
    }
}
