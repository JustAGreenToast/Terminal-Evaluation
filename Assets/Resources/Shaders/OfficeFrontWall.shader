Shader "Unlit/OfficeFrontWall"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WallTile ("Texture", 2D) = "white" {}
        _TileGuide ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

        ZWrite Off
        Lighting Off

        Blend SrcAlpha OneMinusSrcAlpha 

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

            sampler2D _WallTile;
            sampler2D _TileGuide;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = lerp(tex2D(_MainTex, i.uv), tex2D(_WallTile, float2(i.uv.x * 22, i.uv.y * 6)), step(0.5f, tex2D(_TileGuide, i.uv).r));
                return col;
            }
            ENDCG
        }
    }
}
