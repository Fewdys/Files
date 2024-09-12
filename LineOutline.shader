Shader "Outline/LineRendererOutline"
{
    Properties
    {
        _OutlineWidth ("Outline Width", Range(0.0, 2.0)) = 0.1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _MinDistance ("Min Distance", Float) = 2.5
        _MaxDistance ("Max Distance", Float) = 13.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+69969" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Name "OUTLINE"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite On
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            uniform float _OutlineWidth;
            uniform float4 _OutlineColor;
            uniform float _MinDistance;
            uniform float _MaxDistance;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 viewDir = _WorldSpaceCameraPos - i.worldPos;
                float distance = length(viewDir);
                float falloff = saturate((distance - _MinDistance) / (_MaxDistance - _MinDistance));
                float alpha = (1.0 - falloff) * _OutlineColor.a;
                return float4(_OutlineColor.rgb, alpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
