
// James George 
// (C) 2015 Specular LLC

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO; 
using System.Xml;

[ExecuteInEditMode] //this is to ensure our shader variables are set
public class DepthKit : MonoBehaviour {

	//must be less then 256x256
	[Range(0, 255)]
	public int vertsWide = 128;
	[Range(0, 255)]
	public int vertsTall = 128;
	
	public bool startOnPlay = false;
	public bool loops = false;

	public MovieTexture _movieTexture;
	public Texture2D posterFrame;
	public TextAsset properties;

	private bool isInitialized;
	private Vector2 principalPoint;
	private Vector2 focalLength;
	private float width;
	private float height;
	private float minDepth;
	private float maxDepth;
	private Mesh mesh;

	// Use this for initialization
	void Start () {
		if (startOnPlay) {
			InitializeDepthVideo ();
		}
	}
	
	//JG overloaded this so start on play is easier
	public void InitializeDepthVideo(){
		isInitialized = true;

		//parse the file containing the bounds for this sequence
		ReadMetaParams ();

		//Build a mesh to project the texture
		InitDepthKitMesh ();

		//build a video texture
		InitVideoTexture ();

		//Load the params into the shader
		SetShaderProperties ();
	}

	//The meta file contains information from the export to reproject the cloud
	void ReadMetaParams(){

		if (properties == null) {
			Debug.LogError ("No properties XML found");
			isInitialized = false;
			return;
		}

		XmlDocument xml = new XmlDocument();
		xml.LoadXml( properties.text );

		focalLength = new Vector2 (float.Parse(xml["depth"]["fovx"].InnerText), float.Parse(xml["depth"]["fovy"].InnerText));
		principalPoint = new Vector2 (float.Parse(xml["depth"]["ppx"].InnerText), float.Parse(xml["depth"]["ppy"].InnerText));
		width  = float.Parse(xml["depth"]["width"].InnerText);
		height = float.Parse(xml["depth"]["height"].InnerText);
		minDepth = float.Parse(xml["depth"]["minDepth"].InnerText);
		maxDepth = float.Parse(xml["depth"]["maxDepth"].InnerText);

//		Debug.Log ("Focal Length: " + focalLength);
//		Debug.Log ("Principal Point: " + principalPoint);
//		Debug.Log ("Min Depth: " + minDepth);
//		Debug.Log ("Max Depth: " + maxDepth);
	}

	void InitDepthKitMesh(){
		
		mesh = new Mesh();
		MeshFilter mf = GetComponent<MeshFilter> ();
		//MeshRenderer mr = gameObject.GetComponent<MeshRenderer> ();
//		if (mf == null) {
//			mf = gameObject.AddComponent<MeshFilter>() as MeshFilter;
//		}
//		if (mr == null) {
//			mr = gameObject.AddComponent<MeshRenderer>() as MeshRenderer;
//		}

		mf.mesh = mesh;
		
		BuildMesh();	
	}

	void InitVideoTexture(){

		if(_movieTexture == null){
			Debug.LogError ("No video");
			isInitialized = false;
			return;
		}

		if (GetComponent<Renderer> ().sharedMaterial == null) {
			Debug.LogError("DepthKit: No Material");
			isInitialized = false;
			return;
		}

		_movieTexture.loop = loops;
		if (Application.isPlaying) {
			_movieTexture.Play ();
		}
	}
	
	void BuildMesh(){
		
		//Build mesh
		mesh.Clear();

		//Builds a fully tesselated mesh with Topology Triangles
		//Using Topology points or Lines would result in different styles
		Vector3[] verts = new Vector3[vertsWide*vertsTall];
		Vector2[] texcoords = new Vector2[vertsWide*vertsTall];
		int[] indices = new int[(vertsWide-1) * (vertsTall-1) * 3 * 2];
		
		int curIndex = 0;
		for (int y = 0; y < vertsTall-1; y++) {
			for (int x = 0; x < vertsWide-1; x++) {
				
				indices[curIndex++] = x+y*vertsWide;
				indices[curIndex++] = x+(y+1)*vertsWide;
				indices[curIndex++] = (x+1)+y*vertsWide;
				
				indices[curIndex++] = (x+1)+(y)*vertsWide;
				indices[curIndex++] = x+(y+1)*vertsWide;
				indices[curIndex++] = (x+1)+(y+1)*vertsWide;
				
			}
		}

		Vector4 textureStep = new Vector4(1.0f / vertsWide, 1.0f / vertsTall, 0.0f, 0.0f);
		curIndex = 0;
		for (int y = 0; y < vertsTall; y++) {
			for(int x = 0; x < vertsWide; x++){
				verts[curIndex].x = x * textureStep.x;
				verts[curIndex].y = y * textureStep.y;
				verts[curIndex].z = 0.0f;
				curIndex++;
			}
		}
		
		mesh.vertices = verts;
		mesh.uv = texcoords;
		mesh.SetIndices (indices, MeshTopology.Triangles, 0);

		mesh.bounds = new Bounds(new Vector3(0f, 0f, minDepth + maxDepth / 2.0f),
		                         new Vector3(width * .5f * maxDepth / focalLength.x, height * .5f * maxDepth / focalLength.y, (maxDepth - minDepth )));
	}


	//push all the variables into the shader from the properties file
	//also switch between placement post frame and playback	
	void SetShaderProperties(){
		GetComponent<Renderer>().sharedMaterial.SetFloat("_MinDepth", minDepth);
		GetComponent<Renderer>().sharedMaterial.SetFloat("_MaxDepth", maxDepth);
		GetComponent<Renderer>().sharedMaterial.SetFloat("_Width", width);
		GetComponent<Renderer>().sharedMaterial.SetFloat("_Height", height);
		GetComponent<Renderer>().sharedMaterial.SetVector("_PrincipalPoint", principalPoint);
		GetComponent<Renderer>().sharedMaterial.SetVector("_FocalLength", focalLength);

		if (Application.isPlaying || posterFrame == null) {
			GetComponent<Renderer>().sharedMaterial.mainTexture = _movieTexture;
		} 
		else {
			GetComponent<Renderer>().sharedMaterial.mainTexture = posterFrame;

		}
	}

	void Update () {
		if (isInitialized) {
			SetShaderProperties ();
		}
	}
}
