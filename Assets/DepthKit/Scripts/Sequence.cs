
// James George 
// james@simile.systems
// (C) 2016 Simile
// 
// This script and demonstration data is provided for 
// prototyping and educational purposes only

// Unity Mobile Plugin Version 0.1

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO; 
#if UNITY_EDITOR
using UnityEditor;
#endif
using monoflow;

namespace DepthKit {


	[Serializable]
	public class MetaData
	{
		public int versionMajor;
		public int versionMinor;

		public string format;
		public Vector2 depthImageSize;
		public Vector2 depthPrincipalPoint;
		public Vector2 depthFocalLength;
		public float farClip;
		public float nearClip;

		public int videoWidth;
		public int videoHeight;

		public Matrix4x4 projection;
		public Vector3 boundsCenter;
		public Vector3 boundsSize;

		public static MetaData CreateFromJSON(string jsonString)
		{
			MetaData md = JsonUtility.FromJson<MetaData>(jsonString);
			//TODO: fill out missing params based on combination of version & format
			if (md.format == "perpixel") {
				
				//set final image dimensions
				md.videoWidth  = (int)(md.depthImageSize.x);
				md.videoHeight = (int)(md.depthImageSize.y)*2;

				//calculate bounds
				md.boundsCenter = new Vector3 (0f, 0f, (md.farClip - md.nearClip) / 2.0f + md.nearClip);
				md.boundsSize   = new Vector3(
					md.depthImageSize.x * md.farClip / md.depthFocalLength.x, 
					md.depthImageSize.y * md.farClip / md.depthFocalLength.y, 
					md.farClip - md.nearClip);

			}
			return md;
		}
	}

	[ExecuteInEditMode] //this is to ensure our shader variables are set
	public class Sequence : MonoBehaviour {

		public enum MeshDensity{
			High,
			Medium,
			Low
		};

		public MeshDensity meshDensity = MeshDensity.Medium;
		private MeshDensity lastMeshDensity;
		private int vertices = 128;

		public Shader depthkitShader;
		public TextAsset metaDataFile;
		public MPMP movie;
		public Texture2D poster;

		public bool videoLoops = false;
		public bool startOnPlay = false;
		public float delaySeconds = 0.0f;
		private bool playTriggered = false;
		public Material material;

		private MetaData metaData;
		private Mesh mesh;
		private bool isSetup;

		// Use this for initialization
		void Start () {

			//parse the file containing the bounds for this sequence
			ReadMetaParams ();
			//Load the params into the shader
			SetupComponents ();

			playTriggered = false;

		}

		//The meta file contains information from the export, including the position and boundaries
		//of the pointcloud for reprojection, as well as the dimensions of the video
		void ReadMetaParams(){
			
			if (metaDataFile == null) {
				return;
			}

			metaData = MetaData.CreateFromJSON(metaDataFile.text);
		}

		void BuildMesh(){

			if (meshDensity == MeshDensity.High) {
				vertices = 255;
			} else if (meshDensity == MeshDensity.Medium) {
				vertices = 128;
			} else {
				vertices = 64;
			}
			lastMeshDensity = meshDensity;

			//Build mesh
			mesh.Clear();

			//Currently builds a fully tesselated mesh with Topology Triangles
			//Using Topology points or Lines would result in different styles
			Vector3[] verts = new Vector3[vertices*vertices];
			Vector2[] texcoords = new Vector2[vertices*vertices];
			int[] indices = new int[(vertices-1) * (vertices-1) * 3 * 2];
			
			int curIndex = 0;
			for (int y = 0; y < vertices-1; y++) {
				for (int x = 0; x < vertices-1; x++) {
					
					indices[curIndex++] = x+y*vertices;
					indices[curIndex++] = x+(y+1)*vertices;
					indices[curIndex++] = (x+1)+y*vertices;
					
					indices[curIndex++] = (x+1)+(y)*vertices;
					indices[curIndex++] = x+(y+1)*vertices;
					indices[curIndex++] = (x+1)+(y+1)*vertices;
					
				}
			}

			Vector4 textureStep = new Vector4(1.0f / (vertices), 1.0f / (vertices), 0.0f, 0.0f);
			curIndex = 0;
			for (int y = 0; y < vertices; y++) {
				for(int x = 0; x < vertices; x++){
					verts[curIndex].x = x * textureStep.x;
					verts[curIndex].y = y * textureStep.y;
					verts[curIndex].z = 0;
					curIndex++;
				}
			}
			
			mesh.vertices = verts;
			mesh.uv = texcoords;
			mesh.SetIndices (indices, MeshTopology.Triangles, 0);
			if (metaData != null) {
				mesh.bounds = new Bounds (metaData.boundsCenter, metaData.boundsSize);
			}

		}

		void SetupComponents(){

			if (depthkitShader == null || metaData == null) {
				return;
			}

			if (material == null) {
				material = new Material(depthkitShader);
			}

			mesh = new Mesh();
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null) {
				mf = gameObject.AddComponent<MeshFilter> ();
			}
			mf.mesh = mesh;

			MeshRenderer mr = GetComponent<MeshRenderer> ();
			if (mr == null) {
				mr = gameObject.AddComponent<MeshRenderer> ();
			}
			mr.sharedMaterial = material;

			BuildMesh();

			BoxCollider cl = GetComponent<BoxCollider> ();
			if (cl == null) {
				cl = gameObject.AddComponent<BoxCollider> ();
			}
			cl.center = metaData.boundsCenter;
			cl.size   = metaData.boundsSize;

			isSetup = true;
		}

		void Update () {
			
			if (!isSetup) {
				ReadMetaParams ();
				SetupComponents ();
			}

			if (isSetup && lastMeshDensity != meshDensity) {
				BuildMesh();
			}

			if (movie != null) {
				//movie.loop = videoLoops;
				
				//movie.loop = true;
				if (startOnPlay && Application.isPlaying && !playTriggered && Time.time > delaySeconds) {
					movie.Load ();
					movie.Play ();
					playTriggered = true;
				}			
			}

			if (material != null && metaData != null) {
				if (Application.isPlaying && movie != null && playTriggered) {
//					Debug.Log ("movie texture");
					material.SetTexture ("_MainTex", movie._videoTexture); 
					material.SetTexture ("_MainTex2", movie._videoTexture); 
					Debug.Log ("Setting video texture to " + movie._videoTexture);
				
				} else if (!Application.isPlaying && poster != null) {
//					Debug.Log ("poster");
					material.SetTexture ("_MainTex", poster); 
					material.SetTexture ("_MainTex2", poster); 
				
				} else {
//					Debug.Log ("texture null");
					material.SetTexture ("_MainTex", null); 
					material.SetTexture ("_MainTex2", null); 
				}

				/////////TEST
				if (movie != null && movie._videoTexture != null) {
					material.SetTexture ("_MainTex", movie._videoTexture); 
					material.SetTexture ("_MainTex2", movie._videoTexture); 
					Debug.Log ("Setting video texture to " + movie._videoTexture);
				}
				///////////////////
				/// 

				//Matrix4x4 flip = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1.0f, 1.0f, -1.0f));
				//material.SetMatrix ("_OrthoScaleMatrix", flip * metaData.projection);	

				material.SetFloat("_MinDepth", metaData.nearClip);
				material.SetFloat("_MaxDepth", metaData.farClip);
				material.SetFloat("_Width", metaData.depthImageSize.x);
				material.SetFloat("_Height", metaData.depthImageSize.y);
				material.SetVector("_PrincipalPoint", metaData.depthPrincipalPoint);
				material.SetVector("_FocalLength", metaData.depthFocalLength);

				material.SetVector ("_TextureDimension", new Vector4 (metaData.videoWidth, metaData.videoHeight, 0.0f, 0.0f));
				material.SetInt ("_MeshDensity", vertices);

			}
		}

		void OnDrawGizmos() {

			if (Application.isPlaying && metaData != null) {
				Gizmos.color = new Color (.5f, 1.0f, 0, 0.5f);
				Gizmos.DrawWireSphere (
					transform.localToWorldMatrix * new Vector4 (metaData.boundsCenter.x, metaData.boundsCenter.y, metaData.boundsCenter.z, 1.0f), 
					transform.localScale.x * metaData.boundsSize.x * .5f);
			}	
		}

		void OnApplicationQuit()
		{
			isSetup = false;
			movie.Stop ();
		}

		void OnApplicationPause( bool paused )
		{
		}

		#if UNITY_EDITOR
		void OnEnable(){
			EditorApplication.playmodeStateChanged += StateChange;
		}

		void OnDisable(){
			EditorApplication.playmodeStateChanged -= StateChange;
		}

		void StateChange(){
			if (EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying) {

				//remove all components so they can be added again fresh
				material = null;
				mesh = null;

				Destroy (GetComponent<MeshFilter> ());
				Destroy (GetComponent<MeshRenderer> ());
				Destroy (GetComponent<BoxCollider> ());

				isSetup = false;

			}
		}
		#endif
	}

}