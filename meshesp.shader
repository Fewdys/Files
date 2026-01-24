Shader "Custom/MeshESP"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        _BlurWidth ("Blur Width", Range(0, 0.05)) = 0.005
        _BlurStrength ("Blur Strength", Range(0, 1)) = 0.3
        _StencilRef ("Stencil Ref", Int) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100"
            "IgnoreProjector"="True"
        }
        
        // Pass 0: Write to stencil buffer
        Pass
        {
            Name "STENCIL_WRITE"
            ZWrite Off
            ZTest Always
            ColorMask 0
            
            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
        
        // Pass 1: Solid outline
        Pass
        {
            Name "OUTLINE_SOLID"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            
            Stencil
            {
                Ref [_StencilRef]
                Comp NotEqual
                Pass Keep
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            float _OutlineWidth;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };
            
            fixed4 _OutlineColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Expand vertices along normals
                float3 normal = normalize(v.normal);
                float3 outlineOffset = normal * _OutlineWidth;
                float4 expandedVertex = v.vertex + float4(outlineOffset, 0);
                
                o.pos = UnityObjectToClipPos(expandedVertex);
                o.color = _OutlineColor;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
        
        // Pass 2: Blur outline (optional)
        Pass
        {
            Name "OUTLINE_BLUR"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            
            Stencil
            {
                Ref [_StencilRef]
                Comp NotEqual
                Pass Keep
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            float _OutlineWidth;
            float _BlurWidth;
            float _BlurStrength;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };
            
            fixed4 _OutlineColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Expand vertices further for blur
                float3 normal = normalize(v.normal);
                float3 outlineOffset = normal * (_OutlineWidth + _BlurWidth);
                float4 expandedVertex = v.vertex + float4(outlineOffset, 0);
                
                o.pos = UnityObjectToClipPos(expandedVertex);
                o.color = _OutlineColor;
                o.color.a *= _BlurStrength;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
    
    Fallback "Diffuse"
}