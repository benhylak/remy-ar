// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Ring/indeterminate" {
	Properties {
		_PrimaryColor ("Color", Color) = (1,1,1,1)
		_SecondaryColor ("Color", Color) = (1,1,1,1)
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.1, 6.0)) = 3.0
        _Radius("Radius", Range(0.0, 1.0)) = 0.2
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float3 worldPos;
			float3 viewDir;
		};

		fixed4 _PrimaryColor;
		fixed4 _SecondaryColor;
		float _RimPower;
		float _Radius;
		
		float map(float value, float min1, float max1, float min2, float max2)
		{
		    // Convert the current value to a percentage
            // 0% - min1, 100% - max1
            float perc = (value - min1) / (max1 - min1);

            // Do the same operation backwards with min2 and max2
            return perc * (max2 - min2) + min2;
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
			 
			float angle = atan2(IN.worldPos.z - objectOrigin.z,IN.worldPos.x - objectOrigin.x);
			angle += PI;
		    
		    float movingAngle = 2*PI * (_Time.w/10); /// 2 + .05f);
			                                                           
			float angleDist = abs(movingAngle - angle); 
			
            angleDist = abs(angleDist % (2 * PI) - PI);

            float amt = angleDist/PI;

		    //o.Albedo = lerp(_PrimaryColor, _SecondaryColor, sqrt(amt)*amt);
		    
		    o.Albedo = _SecondaryColor;
		    			
			//Calculate Alpha
			float3 camLookVect = IN.worldPos - _WorldSpaceCameraPos;
		    float3 flattened = float3(camLookVect.x, objectOrigin.y, camLookVect.z);
		    
		    float3 closestObjectPoint = objectOrigin - normalize(flattened) * _Radius;
			
			//o.Alpha = (map(sin(_Time.y*2), -1, 1, 0, 1) + .1) * (1 - abs(distance(closestObjectPoint, IN.worldPos)) / (2*_Radius)); 
		   
		    float val = abs(sin(angleDist));//map(sin(angleDist), -1, 1, 0, 1); 
		    o.Alpha = val * val * val * val * val * val * (1-abs(distance(closestObjectPoint, IN.worldPos)) / (4*_Radius));    
			
			//calculate rim power
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = o.Albedo * pow(rim, _RimPower);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
