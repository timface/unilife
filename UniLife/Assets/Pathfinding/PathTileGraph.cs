using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTileGraph {

	public Dictionary<Tile, PathNode<Tile>> nodes;

	public PathTileGraph(World world) {
		nodes = new Dictionary<Tile, PathNode<Tile>> ();

		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {
				Tile t = world.GetTileAt (x, y);

				PathNode<Tile> n = new PathNode<Tile> ();
				n.data = t;
				nodes.Add (t, n);
			}
		}

		Debug.Log ("PathTileGraph: Created " + nodes.Count + " nodes");

		foreach (Tile t in nodes.Keys) {
			PathNode<Tile> n = nodes [t];

			List<PathEdge<Tile>> edges = new List<PathEdge<Tile>> ();

			Tile[] neighbours = t.GetNeighbours (true);

			for (int i = 0; i < neighbours.Length; i++) {
				if (neighbours [i] != null && neighbours [i].movementCost > 0 && !IsClippingCorner (t, neighbours [i])) {
					PathEdge<Tile> e = new PathEdge<Tile> ();
					e.cost = neighbours [i].movementCost;
					e.node = nodes [neighbours [i]];

					edges.Add (e);
				}
			}

			n.edges = edges.ToArray ();
		}
	}

	bool IsClippingCorner (Tile current, Tile neighbour){
		int dX = current.X - neighbour.X;
		int dY = current.Y - neighbour.Y;

		if (Mathf.Abs (dX) + Mathf.Abs (dY) == 2) {
			if (World.Current.GetTileAt (current.X - dX, current.Y).movementCost == 0)
				return true;

			if (World.Current.GetTileAt (current.X, current.Y - dY).movementCost == 0)
				return true;

		}

		return false;
	}
}
