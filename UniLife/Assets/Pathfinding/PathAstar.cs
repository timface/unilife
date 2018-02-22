using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

public class PathAstar {

	Queue<Tile> path;

	public PathAstar ( World world, Tile tileStart, Tile tileEnd, string objectType = null){

		if (world.tileGraph == null)
			world.tileGraph = new PathTileGraph (world);

		Dictionary<Tile, PathNode<Tile>> nodes = world.tileGraph.nodes;

		if (!nodes.ContainsKey(tileStart)){
			Debug.LogError("PathAstar: The starting tile isnt in the list of nodes!");
			return;
		}

		PathNode<Tile> start = nodes [tileStart];

		PathNode<Tile> goal = null;

		if(tileEnd != null) {
			if(!nodes.ContainsKey(tileEnd)){
				Debug.LogError("PathAstar: The ending tile isnt in the list of nodes!");
				return;
			}

			goal = nodes[tileEnd];
		}


		List<PathNode<Tile>> closedSet = new List<PathNode<Tile>> ();

		SimplePriorityQueue<PathNode<Tile>> openSet = new SimplePriorityQueue<PathNode<Tile>> ();
		openSet.Enqueue (start, 0);

		Dictionary<PathNode<Tile>, PathNode<Tile>> cameFrom = new Dictionary<PathNode<Tile>, PathNode<Tile>> ();

		Dictionary<PathNode<Tile>, float> gScore = new Dictionary<PathNode<Tile>, float> ();
		foreach(PathNode<Tile> n in nodes.Values){
			gScore[n] = Mathf.Infinity;
		}
		gScore [start] = 0;

		Dictionary<PathNode<Tile>, float> fScore = new Dictionary<PathNode<Tile>, float> ();
		foreach(PathNode<Tile> n in nodes.Values){
			fScore[n] = Mathf.Infinity;
		}

		fScore [start] = HeuristicCostEstimate (start, goal);

		while (openSet.Count > 0){
			PathNode<Tile> current =  openSet.Dequeue();

			//Check to see if we have a positional goal and if we're there.
			if(goal != null){
				if(current == goal){
					ReconstructPath(cameFrom, current);
					return;
				}
            }
            else
            {
                // We don't have a POSITIONAL goal, we're just trying to find
                // some king of inventory.  Have we reached it?
                if (current.data.haulableItem != null && current.data.haulableItem.Contents.Equals(objectType))
                {
                    Debug.Log("Found the item we need");
                        ReconstructPath(cameFrom, current);
                        return;
                }
            }

            closedSet.Add(current);

			foreach(PathEdge<Tile> edgeNeighbour in current.edges) {
				PathNode<Tile> neighbour = edgeNeighbour.node;

				//Check to see if we've already got this neighbour and skip if we do
				if (closedSet.Contains(neighbour))
					continue;

				float movementCostToNeighbour = neighbour.data.movementCost * DistanceBetween(current, neighbour);

				float tentativeGScore = gScore[current] + movementCostToNeighbour;

				if(openSet.Contains(neighbour) && tentativeGScore >= gScore[neighbour])
					continue;

				cameFrom[neighbour] = current;
				gScore[neighbour] = tentativeGScore;
				fScore[neighbour] = gScore[neighbour] + HeuristicCostEstimate(neighbour, goal);

				if(!openSet.Contains(neighbour)){
					openSet.Enqueue(neighbour, fScore[neighbour]);
				} else {
					openSet.UpdatePriority(neighbour, fScore[neighbour]);
				}
			}
		}
	}

	float HeuristicCostEstimate (PathNode<Tile> a, PathNode<Tile> b){
		if (b == null)
			return 0f;

		return Mathf.Sqrt (Mathf.Pow (a.data.X - b.data.X, 2) + Mathf.Pow (a.data.Y - b.data.Y, 2));
	}

	float DistanceBetween(PathNode<Tile> a, PathNode<Tile> b){
		if ((Mathf.Abs (a.data.X - b.data.X) + Mathf.Abs (a.data.Y - b.data.Y)) == 1)
			return 1f;

		if (Mathf.Abs (a.data.X - b.data.X) == 1 && Mathf.Abs (a.data.Y - b.data.Y) == 1)
			return 1.41421356237f;

		return Mathf.Sqrt (Mathf.Pow (a.data.X - b.data.X, 2) + Mathf.Pow (a.data.Y - b.data.Y, 2));
	}

	void ReconstructPath(Dictionary<PathNode<Tile>, PathNode<Tile>> cameFrom, PathNode<Tile> current){
		Queue<Tile> totalPath = new Queue<Tile> ();
		totalPath.Enqueue (current.data);

		while (cameFrom.ContainsKey (current)) {
			current = cameFrom [current];
			totalPath.Enqueue (current.data);
		}

		path = new Queue<Tile> (totalPath.Reverse ());
	}

	public Tile Dequeue(){
		if (path == null) {
			Debug.LogError ("PathAstar::Dequeue -  Attempting to dequeue from a null path");
			return null;
		}
		if (path.Count <= 0) {
			Debug.LogError ("PathAstar::Dequeue - Path is zero in length but not null. Huh?");
			return null;
		}
		return path.Dequeue ();
	}

	public int Length (){
		if (path == null)
			return 0;

		return path.Count;
	}

	public Tile EndTile() {
		if (path == null || path.Count == 0) {
			Debug.Log ("Path is null or empty");
			return null;
		}

		return path.Last ();
	}
}
