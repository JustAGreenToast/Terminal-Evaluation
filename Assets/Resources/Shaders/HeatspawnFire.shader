Shader "Unlit/HeatspawnFire"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Color1", Color) = (1, 1, 1, 0)
        _Color2 ("Color2", Color) = (1, 1, 1, 0)
        _ScreenRes ("Screen Resolution", Vector) = (1920, 1080, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _ScreenRes;
            float4 _Color1;
            float4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewSpaceVertexPos = UnityObjectToViewPos(i.vertex);
                float3 viewSpacePivotPos = UnityObjectToViewPos(float3(0,0,0));
                float2 uv = viewSpaceVertexPos.xy - viewSpacePivotPos.xy;
                // Divide By Screen Resolution
                uv.x /= _ScreenRes.x;
                uv.y /= _ScreenRes.y;
                // Tiling
                uv.x *= _MainTex_ST.x;
                uv.y *= _MainTex_ST.y;
                // Offset
                uv.x += _MainTex_ST.z;
                uv.y += _MainTex_ST.w;
                return lerp(_Color1, _Color2, tex2D(_MainTex, uv).r);
            }
            ENDCG
        }
    }
}