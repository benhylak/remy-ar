Shader "Custom/listening_ring" {
	Properties {
		_PrimaryColor ("Color", Color) = (1,1,1,1)
		_SecondaryColor ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(1.0, 6.0)) = 3.0
        _Radius("Radius", Range(0.0, 1.0)) = 0.2
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Lambert alpha:fade

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
		fixed4 _PrimaryColor;
		fixed4 _SecondaryColor;
		fixed4 _RimColor;
		float _RimPower;
		float _Radius;

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
			 
			float angle = atan2(IN.worldPos.z - objectOrigin.z,IN.worldPos.x - objectOrigin.x);
			angle += PI;
		
		    //0 -> 2 PI
		    
		    float movingAngle = 2*PI * (_Time.w/6); /// 2 + .05f);
			                                                           
			float angleDist = abs(movingAngle - angle); 
			
            angleDist = abs(angleDist % (2 * PI) - PI);
			//angle = PI;
			//angle /= PI;
			
			//angle is 0 -> 1 -> 0 around the circle
			
			//float targetAngle = angle * abs(_SinTime);
			
//		    if(abs(targetAngle - angle) < PI/3)
//		    {
//		        o.Albedo = float3(0, 0, 1);
//		    }
//		    else o.Albedo = float3(1, 0, 0);
		    
		    o.Albedo = lerp(_SecondaryColor, _PrimaryColor, angleDist/PI);
		    
			
			//Calculate Alpha

			float3 camLookVect = IN.worldPos - _WorldSpaceCameraPos;
		    float3 flattened = float3(camLookVect.x, objectOrigin.y, camLookVect.z);
		    
		    float3 closestObjectPoint = objectOrigin - normalize(flattened) * _Radius;
			
			float lerpVal = 1 - abs(distance(closestObjectPoint, IN.worldPos)) / (2*_Radius); 
		
			o.Alpha = lerp(0.2, 1, lerpVal);
			
			//calculate rim power
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = o.Albedo * pow(rim, _RimPower);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
