using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MeshRenderer), typeof (MeshFilter))]
public class StringLight : MonoBehaviour
{
	[SerializeField]
	private bool repeated;
	[SerializeField]
	private float gravityStrength = .1f;
	[SerializeField]
	private float lightDensity = 2;
	[SerializeField]
	private Vector2 minLightSize = new Vector2 (.1f, .1f);
	[SerializeField]
	private Vector2 maxLightSize = new Vector2 (.3f, .3f);
	[SerializeField]
	private float edgeDensity = .2f;
	[SerializeField]
	private float lineWidth = .5f;
	[SerializeField]
	private Color lineColor = new Color (.3f, .3f, .3f);

	void Awake ()
	{
		GenerateLightStrip ();
	}

	float CalculateDroop (float t) { return gravityStrength * (1 - Mathf.Pow (Mathf.Abs (.5f - Mathf.Clamp01 (t)), 2) / .25f); }

	void GenerateLightStrip ()
	{
		List<Vector3> verts = new List<Vector3> ();
		List<int> tris = new List<int> ();
		List<Color> colors = new List<Color> ();
		List<Vector2> uvs = new List<Vector2>();
		Vector3 rayStart = transform.position;

		//Empty == .5,.5 and texture needs to be white there
		Vector2 empty = new Vector2(.5f,.5f);

		while (repeated || verts.Count == 0)
		{
			if (Physics.Raycast (rayStart, transform.forward, out RaycastHit hit))
			{
				float distance = hit.distance;

				//Get actual physics object if there is one
				if (Physics.Raycast (hit.point, -transform.forward, out RaycastHit startHit))
				{
					rayStart = startHit.point;
					distance = startHit.distance;
				}

				Vector3 end = hit.point;
				int lights = (int) (distance / lightDensity);
				int edges = (int) (distance / edgeDensity);
				if (edges < 2) return;
				int startIndex = verts.Count;
				for (int i = 0; i < edges; i++)
				{
					Color edgeColor = lineColor;
					float t = i * edgeDensity;
					edgeColor.a = t;
					float droop = CalculateDroop (t);
					Vector3 center = transform.InverseTransformPoint (Vector3.Lerp (rayStart, hit.point, t)) + Vector3.down * droop;
					Vector3 top = center + Vector3.up * lineWidth / 2f;
					Vector3 bot = center - Vector3.up * lineWidth / 2f;

					verts.Add (top);
					verts.Add (bot);
					
					uvs.Add(empty);
					uvs.Add(empty);
					
					colors.Add (edgeColor);
					colors.Add (edgeColor);


					tris.Add (i + startIndex);
					tris.Add (i + 2 + startIndex);
					tris.Add (i + 1 + startIndex);
					tris.Add (i + 2 + startIndex);
					tris.Add (i + 3 + startIndex);
					tris.Add (i + 1 + startIndex);
				}

				//Add lights
				Vector3[] lightCenters = new Vector3[lights];
				for (int i = 0; i < lights; i++)
				{
					float t = Random.Range (.2f, .8f);
					float droop = CalculateDroop (t);
					Color col = new Color();
					col.r = Random.Range(0,3)/2f;
					col.g = Random.Range(0,3)/2f;
					col.b = Random.Range(0,3)/2f;

					col.a = t;
					Vector3 center = transform.InverseTransformPoint (Vector3.Lerp (rayStart, hit.point, t) + Vector3.down * droop) + Vector3.down * lineWidth/2f;
					Vector2 size = new Vector2 (Random.Range (minLightSize.x, maxLightSize.x), Random.Range (minLightSize.y, maxLightSize.y));
					Vector3 tl = center - Vector3.forward * (size.x / 2f);
					Vector3 tr = center + Vector3.forward * (size.x / 2f);
					Vector3 bl = tl + Vector3.down * size.y;
					Vector3 br = tr + Vector3.down * size.y;

					verts.Add (tl);
					verts.Add (tr);
					verts.Add (br);
					verts.Add (bl);

					uvs.Add(Vector2.up);
					uvs.Add(Vector2.one);
					uvs.Add(Vector2.right);
					uvs.Add(Vector2.zero);

					colors.Add (col);
					colors.Add (col);
					colors.Add (col);
					colors.Add (col);

					tris.Add (verts.Count - 4);
					tris.Add (verts.Count - 3);
					tris.Add (verts.Count - 2);
					tris.Add (verts.Count - 4);
					tris.Add (verts.Count - 2);
					tris.Add (verts.Count - 1);

				}
				rayStart = hit.point - hit.normal;
			}
			else repeated = false;
		}
		MeshRenderer mr = GetComponent<MeshRenderer> ();
		MeshFilter mf = GetComponent<MeshFilter> ();
		Mesh mesh = new Mesh ();
		mesh.SetVertices (verts);
		mesh.SetTriangles (tris, 0);
		mesh.SetColors (colors);
		mesh.SetUVs(0,uvs);
		mf.mesh = mesh;
	}

}
