// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/listening_ring" {
	Properties {
		_PrimaryColor ("Color", Color) = (1,1,1,1)
		_SecondaryColor ("Color", Color) = (1,1,1,1)
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.1, 6.0)) = 3.0
        _Radius("Radius", Range(0.0, 1.0)) = 0.2
        _WaveSpeed("WaveSpeed", Range(0.0, 10.0)) = 1
        _WaveAmp("Wave Amplitude", Range(0.0, 0.2)) = 0.1
        _Volume("Volume", Range(0.0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha:fade vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0


		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float3 worldPos;
			float3 viewDir;
			float angle;
		};

		fixed4 _PrimaryColor;
		fixed4 _SecondaryColor;
		float _RimPower;
		float _Radius;
		
		float _WaveSpeed;
        float _WaveAmp;
        float _Volume;
		

        void vert(inout appdata_full v, out Input o) 
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);
        
            const float PI = 3.14159;
            			 
			float angle = atan2(v.vertex.z, v.vertex.x) + PI;

            float y = (sin(_Time.w+ angle*2)) *_WaveAmp; //_WaveSpeed* 
            v.vertex.y += y;
        }
    
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {
		    const float PI = 3.14159;
		 
		    // Metallic and smoothness come from slider variables
			float4 objectOrigin = mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0) );
		    
		    float heightPercent = (IN.worldPos.y - objectOrigin.y) / _WaveAmp;
		    
		    if(heightPercent>0.8f) heightPercent = 0.8f;
		    else if(heightPercent<0) heightPercent = 0;
		    
		    //o.Albedo = noiseSample;
		    o.Albedo = lerp(_PrimaryColor, _SecondaryColor, (heightPercent + 2*_Volume)/2);
		    		
			//Calculate Alpha
			float3 camLookVect = IN.worldPos - _WorldSpaceCameraPos;
			
			//camLookVect.y/
		    float3 flattened = normalize(float3(camLookVect.x, objectOrigin.y, camLookVect.z));
		    
		    //float3 flattened = camLookVect * objectOrigin.y/camLookVect.y;
		    float3 closestObjectPoint = objectOrigin - normalize(flattened) * _Radius;
			
			o.Alpha = (1 - abs(distance(closestObjectPoint, IN.worldPos)) / (2*_Radius)) - .015;
			
			//o.Alpha *= lerp(5, 1, (flattened.x + flattened.z)/1);
			
			
			//calculate rim power
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = o.Albedo * pow(rim, _RimPower) + o.Albedo * rim * 3 * _Volume;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
