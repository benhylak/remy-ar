// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/adaptive_transparency" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(1.0, 6.0)) = 3.0
        _Radius("Radius", Range(0.0, 6.0)) = 0.2
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha:fade
		//Lambert alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 viewDir;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float4 _RimColor;
        float _RimPower;
        float _Radius;



		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

    //    void vert (inout appdata_full v, out Input o) {
     //       UNITY_INITIALIZE_OUTPUT(Input,o);
       //     o.objPos = v.vertex;
        //} 
        
		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			//o.Albedo = float3(0, 1, 0); // c.rgb;
			
			// Metallic and smoothness come from slider variables
			float4 objectOrigin = mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0) );
			 
			
			float3 camLookVect = IN.worldPos - _WorldSpaceCameraPos;
		    float3 flattened = float3(camLookVect.x, objectOrigin.y, camLookVect.z);
		    
		    float3 closestObjectPoint = objectOrigin - normalize(flattened) * _Radius;
			
			float lerpVal = 1 - abs(distance(closestObjectPoint, IN.worldPos)) / (2*_Radius); 
			
			o.Albedo = float3(1, 1, 1);//lerp(float3(0, 0, 1), float3(1, 0, 0), lerpVal);
			o.Alpha = lerpVal - .02;
			
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = _RimColor.rgba * pow(rim, _RimPower);
   
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			//o.Alpha = c.a;

		}

		ENDCG
	}
	FallBack "Diffuse"
}
