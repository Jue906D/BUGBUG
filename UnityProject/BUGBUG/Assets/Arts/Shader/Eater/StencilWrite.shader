Shader "UI/StencilWrite_UIImage_URP"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue" = "Geometry+1"     // 比背景早一点点即可
        }

        Pass
        {
            Name "UniversalForward"
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
            ColorMask 0     // 不输出颜色，只写模板
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            half4 frag () : SV_Target
            {
                return 0;   // 颜色完全不写
            }
            ENDHLSL
        }
    }
}