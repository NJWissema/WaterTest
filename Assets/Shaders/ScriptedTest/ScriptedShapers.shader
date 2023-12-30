Shader "MyGame/WaterLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Saturation ("Saturation", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        

        half _Glossiness;  // half, float, fixed, int : different types of numbers
        half _Metallic, _Saturation;
        fixed4 _Color;  //Vector4 : RGBA
        struct Input
        {
            float2 uv_MainTex;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            float2 uv = IN.uv_MainTex;
            float x = uv.x;
            float y = uv.y;
            uv.y += sin(x*6.2831 + _Time.y)*0.1;
            uv.y += sin(y*6.2831 + _Time.x)*0.1;
            fixed4 c = tex2D (_MainTex, uv) * _Color;

            

            // Change Albedo for material. Linear interpolation with saturation
            float saturation = _Saturation;
            o.Albedo = lerp( (c.r + c.g + c.b )/3, c, saturation);
            
            //o.Albedo = 0.21;
            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
