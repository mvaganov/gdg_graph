using UnityEngine;
using System.Collections;

public class NodePicker : MonoBehaviour {
	/// <summary>must be an object with a Graph and Astar component</summary>
	public GameObject aStarObject;
	/// <summary>which object is selected</summary>
	public GameObject selectedObject;
	/// <summary>
	///  null if the selected object does not have a NodeHolder component and associated Graph.Node 
	/// </summary>
	public Graph.Node selectedNode;
	/// <summary>if true, will not allow non-graph nodes to be selected</summary>
	public bool selectNodesOnly = true;
	/// <summary>the A* algorithm MonoBehavior (controls path finding)</summary>
	Astar aStar;
	/// <summary>the Graph monobehavior (controls graph generation)</summary>
	Graph graph;
	/// <summary>particle effect user interface visualization</summary>
	public GameObject cursorSelect, cursorStart, cursorGoal;
	/// <summary>UI text variables </summary>
	string txtSelected = "", txtStart = "?", txtGoal = "?", txtNumberOfNodes;

	void RefreshUIText()
	{
		txtSelected = (selectedObject)?selectedObject.name:"?";
		txtStart = (aStar.startObj)?aStar.startObj.name:"?";
		txtGoal = (aStar.goalObj)?aStar.goalObj.name:"?";
		txtNumberOfNodes = graph.numberOfNodes.ToString();
	}

	void Start ()
	{
		aStar = aStarObject.GetComponent<Astar>();
		graph = aStarObject.GetComponent<Graph>();
		RefreshUIText();
	}

	public void SelectNodeAtMouse(ref GameObject obj, ref Graph.Node node)
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			obj = hit.collider.gameObject;
			node = obj.GetComponent<NodeHolder>() != null ?
				obj.GetComponent<NodeHolder>().node : null;
		}
	}

	/// <summary>which object is being dragged (may be null)</summary>
	GameObject dragging;
	Vector3 mouseClick;
	float dragMouseMoveThreshold = 20;
	void Update ()
	{
		if(Input.GetMouseButtonDown(0))
		{
			mouseClick = Input.mousePosition;
			GameObject obj = null;
			Graph.Node node = null;
			SelectNodeAtMouse(ref obj, ref node);
			if (obj != null && (!selectNodesOnly || node != null))
			{
				selectedObject = obj;
				selectedNode = node;
				RefreshUIText();
				cursorSelect.transform.position = selectedObject.transform.position;
			}
			dragging = obj;
		}
		else if (Input.GetMouseButton(0))
		{
			if (dragging == selectedObject && selectedNode != null
			&& Vector3.Distance(Input.mousePosition, mouseClick) > dragMouseMoveThreshold)
			{
				mouseClick.z = dragMouseMoveThreshold + 1;// ensure dragging is done with high precision
				float dist = Vector3.Distance(transform.position, selectedObject.transform.position);
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Vector3 nextLoc = ray.GetPoint(dist);
				Vector3 delta = nextLoc - transform.position;
				delta.Normalize();
				delta *= dist;
				selectedNode.gameObject.transform.position = transform.position + delta;
				RefreshParticleCursors();
				Graph.RefreshEdgeCoordinates(selectedNode, true);
				aStar.Reset();
			}
		}
		else if (Input.GetMouseButtonDown(1) && selectedObject)
		{
			GameObject obj = null;
			Graph.Node node = null;
			SelectNodeAtMouse(ref obj, ref node);
			if (node != null)
			{
				Graph.Edge e = selectedNode.GetConnectionTo(node);
				if (e == null)
				{
					Graph.ConnectEdge(selectedNode, node, graph.connectBothWays);
				}
				else
				{
					Graph.DisconnectEdge(e, graph.connectBothWays);
				}
				aStar.Reset();
			}
		}
		cursorSelect.SetActive(selectedObject != null);
		cursorStart.SetActive(aStar.startObj != null);
		cursorGoal.SetActive(aStar.goalObj != null);
		if (Input.GetKeyDown(KeyCode.H)) { showControls = !showControls; }
		if (Input.GetKeyDown(KeyCode.M)) { UI_Hitherto(); }
		if (Input.GetKeyDown(KeyCode.T)) { UI_Teleport(); }
		if (Input.GetKeyDown(KeyCode.B)) { UI_SetStart(); }
		if (Input.GetKeyDown(KeyCode.G)) { UI_SetGoal(); }
		if (Input.GetKeyDown(KeyCode.I)) { UI_Iterate(); }
		if (Input.GetKeyDown(KeyCode.C)) { graph.connectBothWays = !graph.connectBothWays; }
		if (Input.GetKeyDown(KeyCode.R)) { UI_Reset(); }
	}

	bool showControls = false;
	void OnGUI()
	{
		if (Camera.main)
		{
			GUILayout.BeginVertical();
				showControls = GUILayout.Toggle(showControls, showControls ? "Hide [H]elp" : "Show [H]elp");
				if (showControls)
				{
					GUILayout.Label("* WASD to move");
					GUILayout.Label("* Hold Right mouse to\n"+
									"  mouse-look");
					GUILayout.Label("* Left-click selects a node");
					GUILayout.Label("* Right-click toggles\n"+
									"  connection with currently\n"+
									"  selected node");
				}
				//GUILayout.Space(10);
				GUILayout.BeginHorizontal();
					GUILayout.Box("selected:"); GUILayout.Label(txtSelected);
					if (selectedObject != null)
					{
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("[M]ove")) UI_Hitherto();
						if (GUILayout.Button("[T]eleport")) UI_Teleport();
					}
					GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					if(GUILayout.Button("A* [B]egin:"))UI_SetStart();	GUILayout.Label(txtStart);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					if(GUILayout.Button("A* [G]oal:"))UI_SetGoal();	GUILayout.Label(txtGoal);
				GUILayout.EndHorizontal();
				if(aStar.CanIterate()){if(GUILayout.Button("A* [I]terate")) UI_Iterate();}
				else{GUILayout.Label("A* can't iterate");}
				GUILayout.BeginHorizontal();
				graph.connectBothWays = GUILayout.Toggle(graph.connectBothWays, "Dual [C]onnection");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					int num;
					txtNumberOfNodes = GUILayout.TextField(txtNumberOfNodes, 2);
					if (int.TryParse(txtNumberOfNodes, out num))
						graph.numberOfNodes = num;
					GUILayout.Label("Nodes");
				GUILayout.EndHorizontal();
				if (GUILayout.Button("[R]eset Graph")) UI_Reset();
			GUILayout.EndVertical();
		}
	}
	void UI_SetStart()
	{
		if (selectedNode != null)
		{
			aStar.startObj = selectedObject;
			cursorStart.transform.position = selectedObject.transform.position;
			RefreshUIText();
		}
	}
	void UI_SetGoal()
	{
		if (selectedNode != null)
		{
			aStar.goalObj = selectedObject;
			cursorGoal.transform.position = selectedObject.transform.position;
			RefreshUIText();
		}
	}
	void UI_Iterate()
	{
		aStar.Iterate();
	}
	void UI_Hitherto()
	{
		selectedObject.transform.position = transform.position;
		aStar.Reset();
		RefreshParticleCursors();
		Graph.RefreshEdgeCoordinates(selectedNode, true);
	}
	void UI_Teleport()
	{
		transform.position = selectedObject.transform.position + Vector3.up * 2;
	}
	void UI_Reset()
	{
		aStar.Clear();
		graph.Restart();
		selectedObject = null;
		selectedNode = null;
		if (transform.position.y < -10)
			transform.position = Vector3.zero + Vector3.up * 2;
	}
	void RefreshParticleCursors()
	{
		if (aStar.startObj != null) cursorStart.transform.position = aStar.startObj.transform.position;
		if (aStar.goalObj != null) cursorGoal.transform.position = aStar.goalObj.transform.position;
		if (selectedObject != null) cursorSelect.transform.position = selectedObject.transform.position;
	}
}
