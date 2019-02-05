Shader "Hidden/Blit"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
	    _AdditiveColor ("Additive Color", Color) = (0,0,0,0)
	    _MainTex ("Base Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 _Color;
            float4 _AdditiveColor;
            sampler2D _MainTex;

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                return col* _Color + _AdditiveColor;
            }
            ENDCG
        }
    }
}
