Shader "Hidden/Outline"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _Thickness ("Thickness", Float) = 1.0
        _MinDepth ("Min Depth", Float) = 0.0
        _MaxDepth ("Max Depth", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "OUTLINE_PASS"
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Thickness;
            float _MinDepth;
            float _MaxDepth;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 offset = _Thickness / _ScreenParams.xy;

                float4 c = tex2D(_MainTex, i.uv);
                float4 c1 = tex2D(_MainTex, i.uv + float2(offset.x, 0));
                float4 c2 = tex2D(_MainTex, i.uv - float2(offset.x, 0));
                float4 c3 = tex2D(_MainTex, i.uv + float2(0, offset.y));
                float4 c4 = tex2D(_MainTex, i.uv - float2(0, offset.y));

                float edge = distance(c.rgb, c1.rgb) +
                            distance(c.rgb, c2.rgb) +
                            distance(c.rgb, c3.rgb) +
                            distance(c.rgb, c4.rgb);

                return float4(edge, edge, edge, 1.0);
            }

            ENDCG
        }
    }

    FallBack Off
}
