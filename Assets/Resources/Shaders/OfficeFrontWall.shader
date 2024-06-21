Shader "Unlit/OfficeFrontWall"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _WallTile ("Wall Texture", 2D) = "white" {}
        _TileSizeOffset ("Wall Tile Size / Offset", Vector) = (22, 6, 0, 0)
        _TileGuide ("Alpha Map", 2D) = "white" {}
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
            float4 _TileSizeOffset;

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
                float2 _TileSize = float2(_TileSizeOffset.x, _TileSizeOffset.y);
                float2 _TileOffset = float2(_TileSizeOffset.z, _TileSizeOffset.w);
                float alpha = tex2D(_TileGuide, i.uv).r;
                float2 tileUv = float2(i.uv.x * _TileSize.x + _TileOffset.x, i.uv.y * _TileSize.y + _TileOffset.y);
                // sample the texture
                fixed4 col = lerp(tex2D(_MainTex, i.uv), tex2D(_WallTile, tileUv), lerp(alpha, 1, step(0.5f, alpha)));
                return col;
            }
            ENDCG
        }
    }
}
