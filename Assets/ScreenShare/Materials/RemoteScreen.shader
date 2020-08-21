Shader "Unlit/RemoteScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _brightness ("Brightness", Float) = 0.0
        _contrast ("Contrast", Float) = 1.0
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
            float _brightness;
            float _contrast;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float r, g, b;
                float2 invertX = float2(1 - i.uv.x, i.uv.y); 
                float4 col = tex2D(_MainTex, invertX);

                r = col.x;
                g = col.y;
                b = col.z;

                r = r * _contrast + _brightness;
                g = g * _contrast + _brightness;
                b = b * _contrast + _brightness;
                return fixed4(r, g, b, 1.0);
            }
            ENDCG
        }
    }
}
