using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph : MonoBehaviour
{	
	public class Node
	{
		public GameObject gameObject;
		public List<Edge> edges = new List<Edge> ();
		public Edge GetConnectionTo(Graph.Node n)
		{
			for (int i = 0; i < edges.Count; ++i)
			{
				if (edges[i].b == n)
					return edges[i];
			}
			return null;
		}
	}

	public class Edge
	{
		public Node a, b;
		public GameObject gameObject;
		public Edge(Node a, Node b) { this.a = a; this.b = b; }
		public float Distance ()
		{
			return Vector3.Distance (
				a.gameObject.transform.position,
				b.gameObject.transform.position);
		}
	}
/* //prefab_graphNode must have the following script component:
using UnityEngine;
using System.Collections;
public class NodeHolder : MonoBehaviour {
	public Graph.Node node;
}
*/
	public GameObject prefab_graphNode;
	public int numberOfNodes = 5;
	public bool connectBothWays = true;
	public Vector3 
		areaMin = new Vector3 (-10, 0, -10), 
		areaMax = new Vector3 (10, 0, 10);
	List<GameObject> nodes = new List<GameObject> ();	
	
	static float Dist (GameObject a, GameObject b)
	{
		return Vector3.Distance (a.transform.position,
			b.transform.position);
	}

	public static List<GameObject> GetClosest (
		GameObject src, int count, List<GameObject> all)
	{
		List<GameObject> r = new List<GameObject> ();
		bool addThisOne;
		for (int i = 0; i < all.Count; ++i) {
			float dist = 0, distOld = 0;
			dist = Dist (src, all [i]);
			if (r.Count >= count) {
				distOld = Dist (src, all [count - 1]);
			}
			addThisOne = src != all [i]
				&& (r.Count <= count || dist < distOld);
			if (addThisOne) {
				bool added = false;
				for (int a = 0; !added && a < r.Count; ++a) {
					distOld = Dist (src, r [a]);
					if (dist < distOld) {
						r.Insert (a, all [i]);
						added = true;
					}
				}
				if (!added)	r.Add (all [i]);
			}
		}
		if (r.Count > count)
			r.RemoveRange (count, r.Count - count);
		return r;
	}

	// Use this for initialization
	void Start ()
	{
		RandomGraphGeneration();
	}

	public void ClearGraph()
	{
		for (int i = 0; i < nodes.Count; ++i)
		{
			GameObject go = nodes[i].gameObject;
			NodeHolder nh = go.GetComponent<NodeHolder>();
			if(nh != null)
			{
				Graph.Node n = nh.node;
				for (int ed = 0; ed < n.edges.Count; ++ed)
				{
					Edge e = n.edges[ed];
					Destroy(e.gameObject);
				}
				Destroy(n.gameObject);
			}
		}
		nodes.Clear();
	}

	public void Restart()
	{
		ClearGraph();
		RandomGraphGeneration();
	}

	public void RandomGraphGeneration()
	{
		Vector3 randLoc;
		// create random nodes
		for (int i = 0; i < numberOfNodes; ++i)
		{
			randLoc = new Vector3(
				Random.Range(areaMin.x, areaMax.x),
				Random.Range(areaMin.y, areaMax.y),
				Random.Range(areaMin.z, areaMax.z));
			GameObject go = (GameObject)Instantiate(
				prefab_graphNode, randLoc,
				Quaternion.identity);
			go.name = "n" + i;
			go.transform.parent = transform;
			nodes.Add(go);
			Graph.Node n = new Graph.Node();
			n.gameObject = go;
			go.GetComponent<NodeHolder>().node = n;
		}
		// connect them
		for (int a = 0; a < numberOfNodes; ++a)
		{
			int minimumNeighborsNeeded = 3;
			Graph.Node n = nodes[a].GetComponent<NodeHolder>().node;
			minimumNeighborsNeeded -= n.edges.Count;
			if (minimumNeighborsNeeded > 0)
			{
				List<GameObject> con = GetClosest(nodes[a], minimumNeighborsNeeded, nodes);
				for (int e = 0; e < con.Count; ++e)
				{
					ConnectEdge(n, con[e].GetComponent<NodeHolder>().node, connectBothWays);
				}
			}
		}
	}

	public static void ConnectEdge(Node a_from, Node a_to, bool connectBothWays)
	{
		if (a_to != a_from)
		{
			if (a_from.GetConnectionTo(a_to) == null)
			{
				Edge ed = new Edge(a_from, a_to);
				a_from.edges.Add(ed);
				CreateEdgeObject(ed);
			}
			if (connectBothWays)
				ConnectEdge(a_to, a_from, false);
		}
	}

	public static void DisconnectEdge(Edge e, bool connectBothWays)
	{
		Destroy(e.gameObject);
		e.a.edges.Remove(e);
		if (connectBothWays)
		{
			Edge eOther = e.b.GetConnectionTo(e.a);
			if (eOther != null)
				DisconnectEdge(eOther, false);
		}
	}

	public static LineRenderer CreateEdgeObject (Edge e)
	{
		LineLib.PushColor(Color.white);
		GameObject lineObj = LineLib.CreateArrowRender(
			Vector3.zero, Vector3.up).gameObject;//new GameObject ("edge");
		LineLib.PopColor();
		e.gameObject = lineObj;
		return SetupEdgeObject(e);
	}
	static LineRenderer SetupEdgeObject(Edge e)
	{
		Vector3 start = e.a.gameObject.transform.position;
		Vector3 end = e.b.gameObject.transform.position;
		LineLib.ShortenLine(start, ref end, e.b.gameObject.transform.localScale.x/2);
		LineLib.ShortenLine(end, ref start, e.a.gameObject.transform.localScale.x);
		LineRenderer liner = e.gameObject.GetComponent<LineRenderer>();
		LineLib.SetupArrowRender(start, end, liner);
		return liner;
	}
	public static void RefreshEdgeCoordinates(Node n, bool andNeighbors)
	{
		for (int e = 0; e < n.edges.Count; ++e)
		{
			SetupEdgeObject(n.edges[e]);
			if (andNeighbors)
			{
				RefreshEdgeCoordinates(n.edges[e].b, false);
			}
		}
	}
	public void RefreshEdgeCoordinates()
	{
		NodeHolder nh;
		for(int i = 0; i < nodes.Count; ++i)
		{
			nh = nodes[i].GetComponent<NodeHolder>();
			if (nh != null)
				RefreshEdgeCoordinates(nh.node, false);
		}
	}
}
