// James George 
// james@simile.systems
// (C) 2016 Simile
// 

// DepthKit
// Unity Mobile Plugin Version 0.1

Shader "DepthKit/DepthKit" 
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_MainTex2 ("Texture", 2D) = "white" {}
		_TextureDimension ("Texture Dimension", Vector) = (0, 0, 0, 0)
		_MinDepth ("Min Depth", Float) = 0.0
		_MaxDepth ("Max Depth", Float) = 0.0
		_Width  ("Width", Float)  = 0.0
		_Height ("Height", Float) = 0.0
		_PrincipalPoint ("Principal Point", Vector) = (0,0,0,0)
		_FocalLength ("Focal Length", Vector) = (0,0,0,0)
		_MeshDensity ("Mesh Density", Range(0,255)) = 128
	}
    
	SubShader {
		Tags { "RenderType" = "Opaque" }

		Cull Off

		CGPROGRAM

		#pragma surface surf Lambert vertex:vert addshadow
		#pragma exclude_renderers d3d11_9x

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float2 uv2_MainTex2 : TEXCOORD1;
		};
		 
		sampler2D _MainTex;
		sampler2D _MainTex2;
		float2 _TextureDimension;
		float _MinDepth;
		float _MaxDepth;
		float _Width;
		float _Height;
		float2 _PrincipalPoint;
		float2 _FocalLength;		
		int _MeshDensity;

		#include "DepthKit.cginc"

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);

			float2 centerpix = float2(1./_TextureDimension.x, 1./_TextureDimension.y);
			float2 textureStep = float2(1.0/_MeshDensity, 1.0/_MeshDensity);

 			float2 basetex = v.vertex.xy;
 			//basetex.y = 1.0 - basetex.y;
			float2 depthTexCoord = basetex * float2(1.0, 0.5) + float2(0.0, 0.5) + centerpix;
			float2 colorTexCoord = basetex * float2(1.0, 0.5) + centerpix;

			//check neighbors
			float2 neighbors[8] = {
				float2(0.,textureStep.y),
				float2(textureStep.x,0.),
				float2(0.,-textureStep.y),
				float2(-textureStep.x,0.),
				float2(-textureStep.x, -textureStep.y),
				float2( textureStep.x,  textureStep.y),
				float2( textureStep.x, -textureStep.y),
				float2(-textureStep.x,  textureStep.y)
			};

	        //texture coords come in as [0.0 - 1.0] for this whole plane
			float depth = depthForPoint(depthTexCoord);

			//search neighbor verts in order to see if we are near an edge
			//if so, clamp to the surface closest to us
			if(depth < epsilon){
				float depthDif = 1.0f;
				float nearestDepth = depth;
				for(int i = 0; i < 8; i++){
					float depthneighbor = depthForPoint(depthTexCoord + neighbors[i]);
					if(depthneighbor >= epsilon){
						float thisDif = abs(nearestDepth - depthneighbor);
						if(thisDif < depthDif){
							depthDif = thisDif;
							nearestDepth = depthneighbor;
						}
					}
				}
				depth = nearestDepth;
			}

			if(depth < epsilon){
				depth = 1.0f; //spikes go backward
			}

			float z = depth * ( _MaxDepth - _MinDepth ) + _MinDepth;
			float4 vert = float4((basetex.x * _Width  - _PrincipalPoint.x) * z / _FocalLength.x,
                    			 (basetex.y * _Height - _PrincipalPoint.y) * z / _FocalLength.y, 
                    			  z, v.vertex.w);

			v.texcoord.xy = colorTexCoord;
			v.texcoord1.xy = depthTexCoord;
			v.vertex = vert;
		}
		
		void surf (Input IN, inout SurfaceOutput o) {

			float3 depth = tex2D (_MainTex, IN.uv2_MainTex2).rgb;
			float3 depthhsv = rgb2hsv(depth);
			int valid = (depthhsv.r > epsilon && depthhsv.g > filters.x && depthhsv.b > filters.y) ? 1.0 : -1.0;
		
			clip(valid);

			float3 baseColor = tex2D (_MainTex, IN.uv_MainTex).rgb;
			//float3 baseColor = float3(depthhsv.r,depthhsv.r,depthhsv.r);
			//o.Albedo = baseColor;
			o.Emission = baseColor;
		}
		ENDCG
	} 
	Fallback "Diffuse"
}