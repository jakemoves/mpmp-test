// James George 
// (C) 2015 Specular LLC

// DepthKit iOS prototpye
// Disney Epic Quest
//
// This script is provided for prototyping purposes only
// and may not be redistributed, or used for any purpose
// other than the EpicQuest Prototype for presentation
// at in 9/2015

Shader "DepthKit/DepthKit-Surface-Shader" 
{
	Properties {
		_MainTex  ("Texture", 2D) = "white" {}
		_MinDepth ("Min Depth", Float) = 0.0
		_MaxDepth ("Max Depth", Float) = 0.0
		_Width  ("Width", Float)  = 0.0
		_Height ("Height", Float) = 0.0
		_PrincipalPoint ("Principal Point", Vector) = (0,0,0,0)
		_FocalLength ("Focal Length", Vector) = (0,0,0,0)
	}
    
	SubShader {
		Tags { "RenderType" = "Opaque" }
		
		CGPROGRAM

		#pragma surface surf Lambert vertex:vert addshadow
		
		struct Input {
			float2 uv_MainTex;
			float valid;
		};
		
		sampler2D _MainTex;
		float _MinDepth;
		float _MaxDepth;
		float _Width;
		float _Height;
		float2 _PrincipalPoint;
		float2 _FocalLength;
		
		static const float epsilon = .1;
		
		fixed3 rgb2hsv(fixed3 c)
		{
	    	fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	    	fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
	    	fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));

	    	float d = q.x - min(q.w, q.y);
	    	return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + epsilon)), d / (q.x + epsilon), q.x);
		}
			
		float depthForPoint(float2 texturePoint){
			float4 textureSample = float4(texturePoint.x, texturePoint.y * .5, 0.0, 0.0);
			fixed4 depthsample = tex2Dlod(_MainTex, textureSample);
			fixed3 depthsamplehsv = rgb2hsv(depthsample.rgb);
			float z = depthsamplehsv.r * ( _MaxDepth - _MinDepth ) + _MinDepth;
			
			return depthsamplehsv.g > .3 && depthsamplehsv.b > .3 ? z : 0.0;
		}
		
		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
 			float2 basetex = v.vertex.xy;
 			
	        //texture coords come in as [0.0 - 1.0] for this whole plane
			float depth = depthForPoint( basetex );

	        //project point
	        float4 vert = float4((basetex.x * _Width  - _PrincipalPoint.x) * depth / _FocalLength.x,
                    			 (basetex.y * _Height - _PrincipalPoint.y) * depth / _FocalLength.y, 
                    			  depth, v.vertex.w);

			v.texcoord.xy = basetex * float2(1.0, 0.5) + float2(0.0, 0.5);
			//TODO check adjacency / edge clip
			v.vertex = vert;
			o.valid = depth > _MinDepth && depth < _MaxDepth ? .0 : -1.0;
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			clip(IN.valid);
			
			o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb ;
			o.Emission =  o.Albedo;
			
			//o.Albedo = float3(IN.valid+1.);
			//o.Emission = float3(IN.valid+1.);
		}
		ENDCG
	} 
	Fallback "Diffuse"
}