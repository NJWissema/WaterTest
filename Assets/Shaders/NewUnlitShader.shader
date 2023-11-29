Shader "MyGame/WaterUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        //Parameters for Gerstner
        _Gravity ("Gravity" , Float) = 9.8
        _Phase ("Phase" , Float) = 0
        _Depth ("Depth" , Float) = 20
        _TimeScale ("TimeScale", Float) = 1
        _NeightbourDistance ("NeightbourDistance", Float) = 1


        _Amplitude1 ("Amplitude1" , Float) = 1
        _Amplitude2 ("Amplitude2" , Float) = 1
        _Amplitude3 ("Amplitude3" , Float) = 1
        _Amplitude4 ("Amplitude4" , Float) = 1
        _Direction1 ("Direction", Vector) = (0.0 ,0.0, 0.0)
        _Direction2 ("Direction", Vector) = (0.0 ,0.0, 0.0)
        _Direction3 ("Direction", Vector) = (0.0 ,0.0, 0.0)
        _Direction4 ("Direction", Vector) = (0.0 ,0.0, 0.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 10

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Direction1, _Direction2, _Direction3, _Direction4;
            float _Amplitude1, _Amplitude2, _Amplitude3, _Amplitude4;
            float _Gravity, _Phase, _Depth, _TimeScale, _NeightbourDistance;

            float frequency_calc(in float gravity, float3 direction, float depth)
            {
                return sqrt((gravity * length(direction)) * tanh(length(direction) * depth));
            }
            float theta_calc(in float3 position, float phase, float time, float gravity, float3 direction, float depth)
            {
                //var final  = (((direction.x * position.x) + (direction.z * position.z)) - (frequency_calc(gravity, direction, depth) * time)) - phase;
                return (((direction.x * position.x) + (direction.z * position.z)) - (frequency_calc(gravity, direction, depth) * time)) - phase;
            }

            float3 gerstner_displacement (in float3 position, float phase, float time, float gravity, float3 direction, float depth, float amplitude)
            {
                float theta = theta_calc(position, phase, time, gravity, direction, depth);
                float xComponent = sin(theta) * (amplitude/tanh(length(direction*depth))) * (direction.x/length(direction));
                float yComponent = cos(theta) * amplitude;
                float zComponent = sin(theta) * (amplitude/tanh(length(direction*depth))) * (direction.z/length(direction));

                return float3(-xComponent, yComponent, -zComponent);
            }

            float3 gerstner_multi_displacement(float3 position, float phase, float4 time, float gravity, float3 direction1, float3 direction2, float3 direction3, float3 direction4, float depth, float amplitude1, float amplitude2, float amplitude3, float amplitude4)
            {
                return 
                gerstner_displacement(position, phase, time, gravity, direction1, depth, amplitude1) + 
                gerstner_displacement(position, phase, time, gravity, direction2, depth, amplitude2) + 
                gerstner_displacement(position, phase, time, gravity, direction3, depth, amplitude3) + 
                gerstner_displacement(position, phase, time, gravity, direction4, depth, amplitude4);
            }
            float3 gerstner_calc(in float3 position, float3 source)
            {
                float3 neighbor1 = gerstner_multi_displacement(position, _Phase, _Time * _TimeScale, _Gravity, _Direction1, _Direction2, _Direction3, _Direction4, _Depth, _Amplitude1, _Amplitude2, _Amplitude3, _Amplitude4);
                float3 neighbor2 = gerstner_multi_displacement(position, _Phase, _Time * _TimeScale, _Gravity, _Direction1, _Direction2, _Direction3, _Direction4, _Depth, _Amplitude1, _Amplitude2, _Amplitude3, _Amplitude4);
                
                return normalize( cross( normalize(neighbor1 - source), normalize(neighbor2 - source) ) );
            }

            //Vertex shader
            v2f vert (appdata v)
            {
                v2f o;
                //v.vertex.y += sin(v.vertex.x + _Time.y)*.5 + .5;
                
                float3 vert_pos = gerstner_multi_displacement(v.vertex, _Phase, _Time * _TimeScale, _Gravity, _Direction1, _Direction2, _Direction3, _Direction4, _Depth, _Amplitude1, _Amplitude2, _Amplitude3, _Amplitude4);
                float3 vert_normal = gerstner_calc(v.vertex, vert_pos);
                


                v.vertex.x += vert_pos.x;
                v.vertex.y += vert_pos.y;
                v.vertex.z += vert_pos.z;
                v.normal = vert_normal;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv //TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            //pixel shader / fragment shader
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uv = i.uv - .5;
                float a = _Time.y;
                float2 p = float2(sin(a), cos(a)) *.5;
                float d = length(uv - p);
                float m = smoothstep(.1, .08, d);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
