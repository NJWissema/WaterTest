Shader "Hidden/NewImageEffectShader"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _DepthMap ("Texture", 2D) = "black" {}
        [HideInInspector] _DepthStart ("Depth Start Distance", float) = 1
        [HideInInspector] _DepthEnd ("Depth End Distance", float) = 300
        [HideInInspector] _DepthColour ("Depth Colour", Color) = (1, 1, 1, 1)
        [HideInInspector] _WaterLevel ("Water Level", Vector) = (0.5, 0.5, 0)
    }
    SubShader
    {
        // Disable backface culling (Cull Off),
        // Depth buffer updating during rendering (ZWrite Off)
        // Always draw a pixel regardless of depth (ZTEst Always)
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture, _MainTex, _DepthMap;
            float _DepthStart, _DepthEnd;
            Vector _WaterLevel;
            fixed4 _DepthColour;    

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos: TEXTCOORD1;
            };

            /* 
                We add an extra screenPos attribute to the vertex data, and compute the
                screen position of each vertex in the vert() function below.
            */
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            // Run on every pixel seen by camera : post processing effects logic here.
            // The image that the camera recieves
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.screenPos);
                if(i.screenPos.y > _WaterLevel.x * i.screenPos.x + _WaterLevel.y - _WaterLevel.x *0.5){
                    return col;
                }
                // sample the pixel in i.screenPos from _CameraDepthTexture, then convert it to
                // linear depth (depth is stored non-linearly) that is clamped between 0 and 1
                float depth = LinearEyeDepth(tex2D(_DepthMap, i.screenPos.xy));
                // half3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

                //  clip depth between 0 and 1 again, where 1 is if the pixel is further
                // Than _DepthEnd, and 0 is if the pixel is nearer than _DepthStart.
                depth = saturate((depth - _DepthStart) / _DepthEnd);
                

                // Scale intensity of _DepthColour based on the depth by Lerping it
                // between the original pixel colour and our colour based on depthValue of the pixel
                // fixed4 col = tex2D(_MainTex, i.screenPos);
                // // unity_SHAr.w;
                // return lerp(col, (0.5 * unity_AmbientSky + _DepthColour*0.5) * unity_AmbientSky.w, depth);

                return lerp(col, _DepthColour, depth);
            }
            ENDCG
        }
    }
}
