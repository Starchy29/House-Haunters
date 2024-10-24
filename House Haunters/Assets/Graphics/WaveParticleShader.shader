Shader "Unlit/WaveParticleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        LOD 100

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
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 radius : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float radius : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
                 
            v2f vert (appdata v)
            {
                v2f o;
                o.radius = v.radius.x;
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float radius = i.radius;
                if(radius == 0.0f) {
                    discard;
                }

                float dx = i.uv.x - 0.5f;
                float dy = i.uv.y - 0.5f;
                float dist = sqrt(dx * dx + dy * dy);

                float edgeDelta = dist - radius;
                float inCircle = clamp(min(radius, 0.5f) - dist, 0.0f, 1.0f);
                inCircle = ceil(inCircle); // 1 or 0

                float halfRad = radius / 2.0f;
                float alpha = clamp(edgeDelta, -halfRad, 0);
                alpha = (alpha + halfRad) / halfRad;
                alpha = alpha * alpha;
                return fixed4(i.color.r, i.color.g, i.color.b, inCircle * alpha);
            }
            ENDCG
        }
    }
}
