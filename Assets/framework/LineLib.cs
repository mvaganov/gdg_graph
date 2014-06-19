using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineLib : MonoBehaviour
{
	public GameObject textMeshBase;
	static LineLib instance = null;
	public float widthStart = 0.05f;
	public float widthFinish = 0.05f;
	/// <summary>The size of the arrow head, in multiples of the width largest width (widthStart or widthFinish)</summary>
	public float sizeArrow = 5;
	public ShaderOptionEnum shader = ShaderOptionEnum.SelfIlluminDiffuse;
	public Color color = Color.red;
	public int numPointsPerCircle = 24;
	
	List<Color> colorStack = new List<Color>();
	
	private float originalStart, originalFinish, originalSizeArrow;
	private ShaderOptionEnum originalShader;
	private Color originalColor;
	private int originalPointsPerCircle;

	private void RememberOriginals()
	{
		originalStart = widthStart;
		originalFinish = widthFinish;
		originalSizeArrow = sizeArrow;
		originalShader = shader;
		originalColor = color;
		originalPointsPerCircle = numPointsPerCircle;
	}
	public void ResetSettings()
	{
		widthStart = originalStart;
		widthFinish = originalFinish;
		sizeArrow = originalSizeArrow;
		shader = originalShader;
		color = originalColor;
		numPointsPerCircle = originalPointsPerCircle;
		colorStack.Clear();
	}
	static public void Identity()
	{
		FindGlobal().ResetSettings();
	}

	string[] shaders = { "Self-Illumin/Diffuse", "Particles/Additive" };//, "VertexLit", "Diffuse"};	
	public enum ShaderOptionEnum { SelfIlluminDiffuse = 0, ParticlesAdditive };
	//public class LineData{Vector3[] line; float s, f; ShaderOptionEnum shader; Color c1, c2;}

	public void SetWidth_(float start, float finish)
	{
		widthStart = start;
		widthFinish = finish;
	}
	public void SetSizeArrow_(float size){sizeArrow = size;}
	public float GetWidthStart_() { return widthStart; }
	public float GetWidthFinish_() { return widthFinish; }
	public float GetSizeArrow_() { return sizeArrow; }
	public float GetSizeArrowAbsolute_(){return Mathf.Max(GetWidthStart_(), GetWidthFinish_()) * GetSizeArrow_();}

	public void SetShaderOption_(ShaderOptionEnum shader)
	{
		this.shader = shader;
	}

	public void SetColor_(Color color) { this.color = color; }
	public Color GetColor_() { return this.color; }
	public void PushColor_(Color c){colorStack.Add(color);SetColor_(c);}
	public void PopColor_(){SetColor_(colorStack[colorStack.Count-1]);colorStack.RemoveAt(colorStack.Count-1);}

	public static void SetShaderOption(ShaderOptionEnum shader) { FindGlobal().SetShaderOption_(shader); }
	public static Color GetColor() { return FindGlobal().GetColor_(); }
	public static void SetColor(Color color) { FindGlobal().SetColor_(color); }
	public static void PushColor(Color color) { FindGlobal().PushColor_(color); }
	public static void PopColor() { FindGlobal().PopColor_(); }
	public static void SetWidth(float widthStart, float widthFinish)
	{ FindGlobal().SetWidth_(widthStart, widthFinish); }
	public static void SetWidth(float width) { FindGlobal().SetWidth_(width, width); }
	public static void SetSizeArrow(float size){ FindGlobal().SetSizeArrow_(size);}
	public static float GetWidthStart() { return FindGlobal().GetWidthStart_(); }
	public static float GetWidthFinish() { return FindGlobal().GetWidthFinish_(); }
	public static float GetSizeArrow() { return FindGlobal().GetSizeArrow_(); }
	public static float GetSizeArrowAbsolute(){return FindGlobal().GetSizeArrowAbsolute_();}

	public static LineRenderer CreateLineRender(Vector3 a, Vector3 b)
	{
		LineRenderer lr = CreateLineRendererGameObject();
		SetupLineRender(a, b, lr);
		return lr;
	}
	public static LineRenderer CreateArrowRender(Vector3 back, Vector3 front)
	{
		LineRenderer lrShaft = CreateLineRender(back, front);
		LineRenderer lrHead = CreateLineRender(back, front);
		lrHead.transform.parent = lrShaft.transform;
		SetupArrowRender(back, front, lrShaft);
		return lrShaft;
	}
	public static LineRenderer CreateLineRender(Vector3[] line)
	{
		LineRenderer lr = CreateLineRendererGameObject();
		SetupLineRender(line, lr);
		return lr;
	}
	public static LineRenderer CreateCircleRender(Vector3 center, float radius, Vector3 normal)
	{
		LineRenderer lr = CreateLineRendererGameObject();
		SetupCircleRender(center, radius, normal, 360, 0, instance.numPointsPerCircle, lr);
		return lr;
	}
	public static LineRenderer CreateArcRender(Vector3 center, float radius, float start, float end, int segmentsPerCircle, Vector3 normal)
	{
		LineRenderer lr = CreateLineRendererGameObject();
		SetupCircleRender(center, radius, normal, start, end, segmentsPerCircle, lr);
		return lr;
	}
	public static LineRenderer CreateSpiralRender(Vector3 center, int size, Vector3 normal)
	{
		LineRenderer lr = CreateLineRendererGameObject();
		SetupSpiralRender(center, size, normal, instance.numPointsPerCircle, lr);
		return lr;
	}
	public static LineRenderer CreateQuaternionRender(Vector3 p, float radius, Quaternion q)
	{
		LineRenderer lr = CreateLineRendererGameObject();
		SetupQuaternionRender(p, radius, q, instance.numPointsPerCircle, lr);
		return lr;
	}
	
	static public LineRenderer CreateLineRendererGameObject()
	{
		return FindGlobal().CreateLineRendererGameObjectInternal();
	}

	public static void ClearAllLines()
	{
		Transform t = FindGlobal().transform;
		List<GameObject> list = new List<GameObject>();
		for(int i = 0; i < t.GetChildCount(); ++i){
			list.Add(t.GetChild(i).gameObject);
		}
		for(int i = 0; i < list.Count; ++i){
			Destroy(list[i]);
		}
	}

	// MonoBehavior upkeep stuff, including test code -----------------------------	
	void Start()
	{
		if (instance == null)
		{
			instance = this;
			instance.RememberOriginals();
		}
// TEST CODE
		//LineRenderer g = LineLib.CreateLineRender(new Vector3(-1, -1, -2), new Vector3(5, 1, -2));
		//Destroy(g.gameObject, 3);
		//Vector3 v = new Vector3(1, 1, 1);
		//v.Normalize();
		//LineLib.CreateSpiralRender(new Vector3(-2, 2, -1), 2, v);
		//Vector3 p = new Vector3(2, -2, -1), n = new Vector3(-1, 1, 1).normalized;
		//for (int i = 0; i < 5; ++i)
		//{
		//    CreateArcRender(p, 1.0f + (0.4f * i), 360 - (i * 15), 0 + (i * 30), 24, n);
		//}
		//LineLib.SetColor(Color.white);
		//Quaternion q = transform.rotation;
		//CreateQuaternionRender(new Vector3(0, -5, 0), .5f, q);
		//LineLib.SetColor(Color.blue);
	}
	void LateUpdate()
	{
		ResetSettings();
	}

	// THE MEATY CODE -------------------------------------------------------------
	/// <summary>if no LineLib exists in the scene, one is created</summary>
	static LineLib FindGlobal()
	{
		if (instance == null)
		{
			TextMesh tm = null;
			Object[] objects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			for (int i = 0; (instance == null || tm == null) && i < objects.Length; ++i)
			{
				if (objects[i] is GameObject)
				{
					GameObject go = (GameObject)objects[i];
					if(instance == null)instance = go.GetComponent<LineLib>();
					if(tm == null)tm = go.GetComponent<TextMesh>();
					//if (instance != null)print("found it!");
				}
			}
			if (instance == null)
			{
				GameObject linelib = new GameObject("LineLib (auto generated)");
				instance = linelib.AddComponent<LineLib>();
				//print("made it myself!");
			}
			if(tm != null)
			{
				instance.textMeshBase = tm.gameObject;
			}
			instance.RememberOriginals();
		}
		return instance;
	}
	static public LineRenderer CreateRenderer(Shader shaderObject, Color color, float widthStart, float widthFinish)
	{
		GameObject go = new GameObject("line");
		LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(shaderObject);
		lineRenderer.material.color = color;
		lineRenderer.castShadows = false;
		lineRenderer.receiveShadows = false;
		lineRenderer.SetColors(color, color);
		lineRenderer.SetWidth(widthStart, widthFinish);
		return lineRenderer;
	}
	public LineRenderer CreateLineRendererGameObjectInternal()
	{
		string shaderName = shaders[(int)shader];
		UnityEngine.Shader shaderObject = Shader.Find(shaderName);
		LineRenderer lineRenderer = CreateRenderer(shaderObject, color, widthStart, widthFinish);
		lineRenderer.transform.parent = transform;
		return lineRenderer;
	}
	public TextMesh CreateTextMeshObjectInternal()
	{
		GameObject go = (GameObject)Instantiate(textMeshBase);
		TextMesh t = go.GetComponent<TextMesh>();
		return t;
	}
	static public TextMesh CreateTextMesh(string text, Vector3 position, Quaternion rotation)
	{
		TextMesh t = FindGlobal().CreateTextMeshObjectInternal();
		t.text = text;
		t.transform.position = position;
		t.transform.rotation = rotation;
		t.transform.Rotate(0,180,0);
		return t;
	}
	static public void SetupLineRender(Vector3 a, Vector3 b, LineRenderer liner)
	{
		liner.SetVertexCount(2);
		liner.SetPosition(0, a);
		liner.SetPosition(1, b);
	}
	public static LineRenderer SetupArrowRender(Vector3 back, Vector3 front, LineRenderer liner)
	{
		Vector3 delta = front - back;
		float arrowsize = GetSizeArrowAbsolute();
		Vector3 arrowBase = delta.normalized * arrowsize;
		Vector3 center = front - arrowBase;
		LineRenderer lrShaft = liner;
		LineRenderer lrHead = liner.transform.GetChild(0).GetComponent<LineRenderer>();
		SetupLineRender(back, center, lrShaft);
		SetupLineRender(center, front, lrHead);
		lrHead.SetWidth(arrowsize, 0);
		return lrShaft;
	}
	static public LineRenderer SetupLineRender(Vector3[] line, LineRenderer liner)
	{
		liner.SetVertexCount(line.Length);
		for (int i = 0; i < line.Length; ++i)
			liner.SetPosition(i, line[i]);
		return liner;
	}
	static public void SetupCircleRender(Vector3 p, float r, Vector3 normal, float start, float end, int pointsPerCircle, LineRenderer liner)
	{
		Vector3[] circle = CreateArc(p, r, start, end, pointsPerCircle, normal);
		SetupLineRender(circle, liner);
	}
	/// <summary>creates an Arc with the given details. TODO refactor/optimize?</summary>
	/// <param name="start">angle start</param>
	/// <param name="end">angle end</param>
	/// <param name="pointsPerCircle">how many even segments make a circle</param>
	/// <returns>vertices that create the described arc</returns>
	static public Vector3[] CreateArc(float start, float end, int pointsPerCircle)
	{
		return CreateArc(Vector3.zero, 1, start, end, pointsPerCircle, Vector3.forward);
	}
	/// <summary>creates an Arc with the given details. TODO refactor/optimize?</summary>
	/// <param name="center">focus of the arc, center if a circle</param>
	/// <param name="radius">distance vertices are from center</param>
	/// <param name="start">angle start</param>
	/// <param name="end">angle end</param>
	/// <param name="pointsPerCircle">how many even segments make a circle</param>
	/// <param name="normal">where 'up' is. Circle will be clockwise from here</param>
	/// <returns>vertices that create the described arc</returns>
	static public Vector3[] CreateArc(Vector3 center, float radius, float start, float end, int pointsPerCircle, Vector3 normal)
	{
		bool reverse = end < start;
		if (reverse)
		{
			float temp = start;
			start = end;
			end = temp;
		}
		while (start < 360) { start += 360; end += 360; }
		while (start > 360) { start -= 360; end -= 360; }
		if (normal == Vector3.zero)
			normal = Vector3.forward;
		Vector3[] lineStrip;
		Quaternion q = Quaternion.LookRotation(normal);
		Vector3 right = GetUpVector(q);
		Vector3 r = right * radius;
		if (end == start)
		{
			lineStrip = new Vector3[1];
			q = Quaternion.AngleAxis(start, normal);
			lineStrip[0] = center + q * r;
			return lineStrip;
		}
		float degreesPerSegment = 360f / pointsPerCircle;

		float startIndexF = start * pointsPerCircle / 360f;//start / degreesPerSegment;
		int startIndex = (int)startIndexF;
		float startRemainder = startIndexF - startIndex;
		startIndex = startIndex + ((startRemainder > 0) ? 1 : ((startRemainder < 0) ? -1 : 0));
		float endIndexF = end * pointsPerCircle / 360f;// end / degreesPerSegment;
		int endIndex = (int)endIndexF;
		float endRemainder = endIndexF - endIndex;

		int inBetweenSegments = (endIndex >= startIndex) ? (endIndex - startIndex + 1) : 0;
		int numPoints = inBetweenSegments + ((startRemainder != 0) ? 1 : 0) + ((endRemainder != 0) ? 1 : 0);
		// allocate the required memory
		lineStrip = new Vector3[numPoints];
		// fill the memory with the points of the arc
		int index = reverse ? (lineStrip.Length - 1) : 0;
		if (startRemainder != 0)
		{
			q = Quaternion.AngleAxis((startIndex - 1) * degreesPerSegment, normal);
			Vector3 preStart = center + q * r;
			q = Quaternion.AngleAxis((startIndex) * degreesPerSegment, normal);
			Vector3 actualStart = center + q * r;
			lineStrip[index] = Vector3.Lerp(preStart, actualStart, startRemainder);
			index += reverse ? -1 : 1;
		}
		for (int v = startIndex; v <= endIndex; ++v)
		{
			q = Quaternion.AngleAxis(v * degreesPerSegment, normal);
			lineStrip[index] = center + q * r;
			index += reverse ? -1 : 1;
		}
		if (endRemainder != 0)
		{
			q = Quaternion.AngleAxis((endIndex + 1) * degreesPerSegment, normal);
			Vector3 postEnd = center + q * r;
			q = Quaternion.AngleAxis((endIndex) * degreesPerSegment, normal);
			Vector3 actualEnd = center + q * r;
			lineStrip[index] = Vector3.Lerp(actualEnd, postEnd, endRemainder);
			index += reverse ? -1 : 1;
		}
		return lineStrip;
	}
	static public Vector3[] CreateAngle(Vector3 center, float radius, float start, float end, Vector3 normal)
	{
		Quaternion q = Quaternion.LookRotation(normal);
		Vector3 right = GetUpVector(q);
		Vector3 r = right * radius;
		Vector3[] lineStrip = new Vector3[3];
		q = Quaternion.AngleAxis(start, normal);
		lineStrip[0] = center + q * r;
		lineStrip[1] = center;
		q = Quaternion.AngleAxis(end, normal);
		lineStrip[2] = center + q * r;
		return lineStrip;
	}
	static public void SetupQuaternionRender(Vector3 p, float r, Quaternion quaternion, int pointsPerCircle, LineRenderer liner)
	{
		Vector3 axis; float angle;
		quaternion.ToAngleAxis(out angle, out axis);
		Vector3 r2 = axis * r * 2;
		liner = CreateLineRender(p - r2, p + r2);
		LineRenderer curve = CreateArcRender(p, r, 0, angle, pointsPerCircle, axis);
		curve.transform.parent = liner.transform;
	}
	static public void SetupSpiralRender(Vector3 p, int size, Vector3 normal, int pointsPerCircle, LineRenderer liner)
	{
		int numPoints = size * pointsPerCircle;
		Vector3 right = GetRightVector(Quaternion.LookRotation(normal));
		liner.SetVertexCount(numPoints);
		float rad = 0;
		for (int i = 0; i < numPoints; ++i)
		{
			Quaternion q = Quaternion.AngleAxis(i * 360.0f / pointsPerCircle, normal);
			Vector3 delta = q * right * rad;
			liner.SetPosition(i, p + (delta * 1));
			rad += 2f / pointsPerCircle;
		}
	}
	public static Vector3 GetForwardVector(Quaternion q)
	{
		return new Vector3(2 * (q.x * q.z + q.w * q.y),
						   2 * (q.y * q.z - q.w * q.x),
						   1-2*(q.x * q.x + q.y * q.y));
	}
	public static Vector3 GetUpVector(Quaternion q)
	{
		return new Vector3(2 * (q.x * q.y - q.w * q.z),
						   1-2*(q.x * q.x + q.z * q.z),
						   2 * (q.y * q.z + q.w * q.x));
	}
	public static Vector3 GetRightVector(Quaternion q)
	{
		return new Vector3(1-2*(q.y * q.y + q.z * q.z),
						   2 * (q.x * q.y + q.w * q.z),
						   2 * (q.x * q.z - q.w * q.y));
	}

	/// <summary>
	/// </summary>
	/// <param name="a_vertices"></param>
	/// <param name="a_newSize"></param>
	/// <returns></returns>

	public static Vector3[] InsertVertices(Vector3[] a_vertices, int a_numNewPoints)
	{
		if (a_numNewPoints <= 0)
		{
			return a_vertices;
		}
		/*
		+           +           +           +           +	old
		0-----------1-----------2-----------3-----------4
		oldSegmentCount = 5
		addableSegments = 4

		newSize = 20

		+ .  . L  . +  .  .  .  + .  M .  . + .  R .  . +	new
		0-1--3-4--5-6--7--8--9--A-B--C-D--E-F-G--H-I--J-K
		newSegmentCount = 20
		numNewSegments = 15
		numMidSegmentsPerSegment = 3 (remainderSegments 3)
		addToMiddle = 1
		addToBeg = 1
		addToEnd = 1
		*/
		int oldSegmentCount = a_vertices.Length;
		// how many old segments can have new points inserted after them (+ excluding the last one)
		int addableSegments = (oldSegmentCount - 1);
		// how many pairs total the new model will have (+,L,R,M,.)
		int newSegmentCount = oldSegmentCount + a_numNewPoints;
		// how many of those pairs need to be calculated (L,R,M,.)
		int numNewSegments = newSegmentCount - oldSegmentCount;
		// how many regular segments to add between existing segments (.)
		int numMidSegmentsPerSegment = numNewSegments / (addableSegments);
		// how many segments are left to add (L,R,M)
		int remainderSegments = numNewSegments - numMidSegmentsPerSegment * addableSegments;//numNewSegments % addableSegments;
		// how many segments to add to the middle (M)
		int addToMiddle = remainderSegments % 2;
		// how many segments to add to the beginning (L)
		int addToBeg = remainderSegments / 2;
		// how many segments to add to the end (R)
		int addToEnd = remainderSegments / 2;

		int middleGroup = addableSegments / 2;
		int additionalAdded = 0;

		//print("+new " + numNewSegments +
		//    "    +put " + addableSegments +
		//    "    +avg " + numMidSegmentsPerSegment +
		//    "    +rem " + remainderSegments +
		//    "    +beg " + addToBeg +
		//    "    +mid " + addToMiddle +
		//    "    +end " + addToEnd);

		//for (int i = 0; i < a_vertices.Length; ++i)
		//{
		//    print("->"+a_vertices[i]);
		//}
		//print("----------------");
		Vector3[] a_list = new Vector3[newSegmentCount];
		int cursor = 0;
		// start adding elements into the list. wait, zero element first.
		// ok, no start adding element to the list.
		for (int seg = 0; seg < addableSegments; ++seg)
		{
			// calculate how many extra segments to add here
			int additionalSeg = 0;
			additionalSeg += (seg >= addableSegments - addToEnd) ? 1 : 0;
			//if ((seg >= oldSegmentCount - addToEnd))print("add to end");
			additionalSeg += (seg < addToBeg) ? 1 : 0;
			//if (seg < addToBeg)print("add to beg");
			additionalSeg += (addToMiddle > 0 && seg == middleGroup) ? 1 : 0;
			//if (addToMiddle > 0 && seg == middleGroup)print("add to mid");
			if (additionalSeg != 0) additionalAdded++;
			int numPointsToInsertHere = 1 + numMidSegmentsPerSegment + additionalSeg;
			//print(numPointsToInsertHere);
			Vector3 segStart = a_vertices[seg];
			Vector3 segEnd = a_vertices[seg + 1];

			for (int i = 0; i < numPointsToInsertHere; ++i)
			{
				a_list[cursor] = Vector3.Lerp(segStart, segEnd, (float)i / numPointsToInsertHere);
				//if (i == 0) print("->" + a_list[cursor]);else print("   " + a_list[cursor]);
				cursor++;
			}
		}
		a_list[cursor] = a_vertices[a_vertices.Length - 1];
		//print("->" + a_list[cursor]);
		// final check
		if (additionalAdded != remainderSegments) print("ADDED A DIFFERENT AMOUNT! MATH WRONG! PLZ CHECK! " + additionalAdded + " vs " + remainderSegments);
		return a_list;
	}

	/// <example>CreateSpiralSphere(transform.position, 0.5f, transform.up, transform.forward, 16, 8);</example>
	/// <summary>
	/// creates a line spiraled onto a sphere
	/// </summary>
	/// <param name="center"></param>
	/// <param name="radius"></param>
	/// <param name="axis"></param>
	/// <param name="axisFace"></param>
	/// <param name="sides"></param>
	/// <param name="rotations"></param>
	/// <returns></returns>
	public static Vector3[] CreateSpiralSphere(Vector3 center, float radius, Vector3 axis, Vector3 axisFace,
		float sides, float rotations)
	{
		List<Vector3> points = new List<Vector3>();
		if (sides != 0 && rotations != 0)
		{
			float iter = 0;
			float increment = 1f / (rotations * sides);
			points.Add(center + axis * radius);
			do
			{
				iter += increment;
				Quaternion faceTurn = Quaternion.AngleAxis(iter * 360 * rotations, axis);
				Vector3 newFace = faceTurn * axisFace;
				Quaternion q = Quaternion.LookRotation(newFace);
				Vector3 right = LineLib.GetUpVector(q);
				Vector3 r = right * radius;
				q = Quaternion.AngleAxis(iter * 180, newFace);
				Vector3 newPoint = center + q * r;
				points.Add(newPoint);
			}
			while (iter < 1);
		}
		return points.ToArray();
	}

	public static Vector3[] CreateSpiralSphereTriangleStrip(
		Vector3 center, float radius, Vector3 axis, Vector3 axisFace,
		float sides, float rotations, bool twoPeels)
	{
		List<Vector3> both = new List<Vector3>();
		if (sides != 0 && rotations != 0)
		{
			Vector3[] swirl0, swirl1;
			swirl0 = CreateSpiralSphere(center, radius, axis, axisFace, sides, rotations);
			swirl1 = CreateSpiralSphere(center, radius, axis, -axisFace, sides, rotations);
			List<Vector3> insides = new List<Vector3>();
			List<Vector3> insides2 = new List<Vector3>();
			int offset = (int)(sides / 2);
			Vector3 v0, v1;
			if (twoPeels)
			{
				for (int i = -offset; i < swirl0.Length; ++i)
				{
					int s0 = i;
					int s1 = i + offset;
					v0 = s0 >= 0 ? swirl0[s0] : swirl1[-s0];
					v1 = s1 < swirl1.Length ? swirl1[s1] : swirl0[swirl0.Length - (s1 - swirl1.Length) - 1];
					insides.Add(v0);
					insides.Add(v1);
					v0 = s0 >= 0 ? swirl1[s0] : swirl0[-s0];
					v1 = s1 < swirl0.Length ? swirl0[s1] : swirl1[swirl1.Length - (s1 - swirl0.Length) - 1];
					insides2.Add(v0);
					insides2.Add(v1);
				}
			}
			else
			{
				for (int i = -offset; i < swirl0.Length; ++i)
				{
					int s0 = (i >= 0) ? i : 0;
					int s1 = (i + offset < swirl1.Length) ? i + offset : swirl1.Length - 1;
					insides.Add(swirl0[s0]);
					insides.Add(swirl1[s1]);
					insides2.Add(swirl1[s0]);
					insides2.Add(swirl0[s1]);
				}
			}
			both.AddRange(insides);
			insides2.Reverse();
			both.AddRange(insides2);
		}
		return both.ToArray();
	}

	static public int[] CreateTriangleMapForTriangleStrip(Vector3[] triStrip)
	{
		int[] triMap = new int[triStrip.Length * 3];
		for (int i = 0; i < triStrip.Length - 3; i += 1)
		{
			triMap[i * 3 + 0] = i + 0;
			triMap[i * 3 + 1] = i + 1;
			triMap[i * 3 + 2] = i + 2;
			i++;
			triMap[i * 3 + 0] = i + 0;
			triMap[i * 3 + 1] = i + 2;
			triMap[i * 3 + 2] = i + 1;
		}
		return triMap;
	}
	
	public static void ShortenLine(Vector3 start, ref Vector3 end, float amountToShorten)
	{
		Vector3 delta = end - start;
		Vector3 choppedOff = delta.normalized * amountToShorten;
		delta -= choppedOff;
		end = start + delta;
	}
}