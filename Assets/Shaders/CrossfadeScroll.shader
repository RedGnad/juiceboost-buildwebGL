Shader "Unlit/ScrollMixte"
{
    Properties
    {
        _MainTex ("Texture courante", 2D) = "white" {}
        _NextTex ("Texture suivante", 2D) = "white" {}
        _Mix     ("Facteur de mélange", Range(0,1)) = 0
    }
    SubShader
    {
        // >>> Passage en Transparent + alpha blending
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        // <<<

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _NextTex;
            float     _Mix;
            float4    _MainTex_ST;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 a = tex2D(_MainTex,  i.uv);
                fixed4 b = tex2D(_NextTex, i.uv);
                return lerp(a, b, _Mix);
            }
            ENDCG
        }
    }
}
