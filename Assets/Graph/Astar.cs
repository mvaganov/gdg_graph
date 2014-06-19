using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Astar : MonoBehaviour {
	
	public GameObject startObj, goalObj;
	public bool waitToIterate = true;
	public bool drawLines = true;
	public List<List<Graph.Node>> foundPaths = new List<List<Graph.Node>>();
	
	private List<LineRenderer> pathLines = new List<LineRenderer>();
	
	Graph.Node start, goal;
	
	private AstarLogic a;
	
	void Update () {
		if(startObj != null
		&&(start == null || start.gameObject != startObj))
		{
			NodeHolder nh = startObj.GetComponent<NodeHolder>();
			if(nh != null)
				start = nh.node;
			Reset();
		}
		if (goalObj != null
		&& (goal == null || goal.gameObject != goalObj))
		{
			NodeHolder nh = goalObj.GetComponent<NodeHolder>();
			if(nh != null)
				goal = nh.node;
			Reset();
		}
		if (start != null && goal != null
		&& (a == null 
		|| (a != null && (a.start != start || a.goal != goal))) )
		{
//			print("A* started!");
//			Reset();
			a = new AstarLogic(start, goal);
			if(drawLines)UpdateAStarVisualization();
		}
		if(!waitToIterate && CanIterate())
		{
			Iterate();
		}
	}

	public void Clear()
	{
		Reset();
		startObj = null;
		goalObj = null;
		start = null;
		goal = null;
	}

	public void Reset()
	{
		ClearPaths();
		if (a != null) a.ClearVisualizations();
		a = null;
	}
	
	public bool CanIterate()
	{
		return a != null && !a.IsFinished();
	}
	
	void DrawPathLine(List<Graph.Node> path, Vector3 offset)
	{
		//System.Text.StringBuilder str = new System.Text.StringBuilder();
		LineLib.PushColor(Color.yellow);
		Vector3 v1 = path[0].gameObject.transform.position + offset, v2;
		for(int i = 0; i < path.Count; ++i)
		{
			v2 = path[i].gameObject.transform.position + offset;
			pathLines.Add(LineLib.CreateArrowRender(v2, v1));
			//if(i > 0)str.Append("->");str.Append(path[i].gameObject.name);
			v1 = v2;
		}
		LineLib.PopColor();
		//print(str);
	}
	
	void ClearPaths()
	{
		foreach(LineRenderer l in pathLines)
			Destroy(l.gameObject);
		pathLines.Clear();
		foundPaths.Clear();
	}
	
	void UpdateAStarVisualization()
	{
		a.CalculateVisualizationObjectData(Vector3.up, Vector3.right*0.2f, new Vector3(0,0.5f,0));
	}
	
	public void Iterate()
	{
		if (CanIterate())
		{
			List<Graph.Node> path = a.Update();
			if(path != null)
			{
				foundPaths.Add(path);
				if(drawLines)
				{
					DrawPathLine(path, Vector3.up * 0.3f * foundPaths.Count);
				}
			}
			if(drawLines)UpdateAStarVisualization();
		}
	}
	
	public class AstarLogic
	{
		Dictionary<Graph.Node,LineRenderer[]> lineObjects = new Dictionary<Graph.Node,LineRenderer[]>();
		Dictionary<Graph.Node,TextMesh[]> textObjects = new Dictionary<Graph.Node,TextMesh[]>();
		public void CalculateVisualizationObjectData(Vector3 up, Vector3 right, Vector3 offset)
		{
			float maxg = Vector3.Distance(
				start.gameObject.transform.position, 
				goal.gameObject.transform.position);
			float maxf = f_score[start];
			if(maxg == 0)maxg = 0.1f;
			if(maxf == 0)maxf = 0.1f;
			LineRenderer[] lines;
			TextMesh[] texts;
			bool found;
			List<Graph.Node>[] sets = new List<Graph.Node>[2];
			sets[0] = openset;
			sets[1] = closedset;
			List<Graph.Node> s;
			for(int setIndex = 0; setIndex < sets.Length; ++setIndex)
			{
				s = sets[setIndex];
				for(int i = 0; i < s.Count; ++i)
				{
					Graph.Node n = s[i];
					Vector3 p = n.gameObject.transform.position + offset;
					found = lineObjects.TryGetValue(n, out lines);
					found = textObjects.TryGetValue(n, out texts);
					if(!found)
					{
						lines = new LineRenderer[2];
						texts = new TextMesh[2];
						// [0] = fscore
						LineLib.SetColor(Color.magenta);
						LineLib.SetWidth(.1f);
						lines[0] = LineLib.CreateLineRendererGameObject();
						// [1] = gscore
						LineLib.SetColor(Color.white);
						LineLib.SetWidth(.1f);
						lines[1] = LineLib.CreateLineRendererGameObject();
						lines[0].SetPosition(0, p);
						lines[1].SetPosition(0, p + right);
						LineLib.Identity();
						// text meshes
						texts[0] = LineLib.CreateTextMesh(""+f_score[n], Vector3.zero, Quaternion.Euler(0,0,-90));
						texts[1] = LineLib.CreateTextMesh(""+g_score[n], Vector3.zero, Quaternion.Euler(0,0,-90));
						// use these lines
						lineObjects[n] = lines;
						textObjects[n] = texts;
					}
					switch(setIndex){
					case 0:	lines[0].material.color = new Color(1,0,1);
							lines[1].material.color = new Color(1,1,1);	
							break;
					case 1:	lines[0].material.color = new Color(.25f, 0, .25f);
							lines[1].material.color = new Color(.25f, .25f, .25f);
							break;		
					}
					Vector3 endPos = p + (up * (f_score[n] / maxf));
					lines[0].SetPosition(1, endPos);
					texts[0].transform.position = endPos;
					endPos = p + right + (up * (g_score[n] / maxg));
					lines[1].SetPosition(1, endPos);
					texts[1].transform.position = endPos;
				}
			}
		}
		public void ClearVisualizations()
		{
			foreach (KeyValuePair<Graph.Node,LineRenderer[]> data in lineObjects)
			{
				Destroy(data.Value[0].gameObject);
				Destroy(data.Value[1].gameObject);
			}
			foreach (KeyValuePair<Graph.Node,TextMesh[]> data in textObjects)
			{
				Destroy(data.Value[0].gameObject);
				Destroy(data.Value[1].gameObject);
			}
		}
		
		bool finished = false;
		public bool IsFinished(){return finished;}
//function A*(start,goal)
		public Graph.Node start, goal;
// closedset := the empty set    // The set of nodes already evaluated.
		public List<Graph.Node> closedset = new List<Graph.Node>();
// openset := {start}    // The set of tentative nodes to be evaluated, initially containing the start node
		public List<Graph.Node> openset = new List<Graph.Node>();
// came_from := the empty map    // The map of navigated nodes.
		public Dictionary<Graph.Node, Graph.Node> came_from = new Dictionary<Graph.Node, Graph.Node>();
// g_score[start] := 0    // Cost from start along best known path.
		public Dictionary<Graph.Node, float> g_score = new Dictionary<Graph.Node, float>();
// // Estimated total cost from start to goal through y.
// f_score[start] := g_score[start] + heuristic_cost_estimate(start, goal)
		public Dictionary<Graph.Node, float> f_score = new Dictionary<Graph.Node, float>();
		
		public AstarLogic(Graph.Node start, Graph.Node goal)
		{
			this.start = start;
			this.goal = goal;
			openset.Add(start);
			g_score[start] = 0;
			f_score[start] = g_score[start] + heuristic_cost_estimate(start, goal);
		}
		
		public List<GameObject> text = new List<GameObject>();
		
		public List<Graph.Node> Update()
		{
// while openset is not empty
			/*while*/if(openset.Count > 0)
			{
//     current := the node in openset having the lowest f_score[] value
				Graph.Node current = smallestFrom(openset, f_score);
//     remove current from openset
				openset.Remove(current);
//     add current to closedset
				closedset.Add(current);
//     if current = goal
				if(current == goal)
				{
//         return reconstruct_path(came_from, goal)
					return reconstruct_path(came_from, goal);
				}
//     for each neighbor in neighbor_nodes(current)
				for(int i = 0; i < current.edges.Count; ++i)
				{
					Graph.Node neighbor = current.edges[i].b;
//         tentative_g_score := g_score[current] + dist_between(current,neighbor)
					float tentative_g_score = g_score[current] + dist_between(current,neighbor);
//         if neighbor in closedset and tentative_g_score >= g_score[neighbor]
					if(closedset.IndexOf(neighbor) >= 0 && tentative_g_score >= g_score[neighbor])
					{
//                 continue
						continue;
					}
//         if neighbor not in openset or tentative_g_score < g_score[neighbor] 
					if(openset.IndexOf(neighbor) < 0 || tentative_g_score < g_score[neighbor])
					{
//             came_from[neighbor] := current
						came_from[neighbor] = current;
//             g_score[neighbor] := tentative_g_score
						g_score[neighbor] = tentative_g_score;
//             f_score[neighbor] := g_score[neighbor] + heuristic_cost_estimate(neighbor, goal)
						f_score[neighbor] = g_score[neighbor] + heuristic_cost_estimate(neighbor, goal);
//             if neighbor not in openset
						if(openset.IndexOf(neighbor) < 0)
						{
//                 add neighbor to openset
							openset.Add(neighbor);
						}
					}
				}
			}
// return failure
			finished = openset.Count == 0;
			return null;
		}
		public static float dist_between(Graph.Node a, Graph.Node b)
		{
			return Vector3.Distance(a.gameObject.transform.position, b.gameObject.transform.position);
		}
		public static float heuristic_cost_estimate(Graph.Node start, Graph.Node goal)
		{
			return dist_between(start, goal) * 10;
		}
		public static Graph.Node smallestFrom(List<Graph.Node> list, Dictionary<Graph.Node, float> values)
		{
			int minIndex = 0;
			float smallest = values[list[minIndex]], score;
			for(int i = 1; i < list.Count; ++i)
			{
				score = values[list[i]];
				if(score < smallest)
				{
					minIndex = i;
					smallest = score;
				}
			}
			return list[minIndex];
		}
		static List<Graph.Node> reconstruct_path(Dictionary<Graph.Node,Graph.Node> previous, Graph.Node goal)
		{
			List<Graph.Node> list = new List<Graph.Node>();
			Graph.Node prev = goal;
			while (prev != null)
			{
				list.Add(prev);
				if(!previous.TryGetValue(prev, out prev))
					prev = null;
			}
			return list;
		}
	}
}
