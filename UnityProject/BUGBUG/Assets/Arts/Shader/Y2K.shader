Shader "BUG/Y2K"
{
    Properties
    {
        _MainTex ("Screen", 2D) = "white" {}
        _ScanInt ("Scan Line Intensity", Range(0,1)) = 0.5
        _Pixelate ("Pixelate", Range(1,20)) = 8
        _RGBShift ("RGB Shift", Range(0,1)) = 0.3
        _Noise ("Noise", Range(0,1)) = 0.08
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _ScanInt, _Pixelate, _RGBShift, _Noise;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 像素化
                float2 px = floor(i.uv * _Pixelate) / _Pixelate;
                fixed4 col = tex2D(_MainTex, px);

                // 2. 扫描线
                float scan = sin(i.uv.y * 3.1415 * 2) * 0.5 + 0.5;
                col.rgb = lerp(col.rgb, col.rgb * scan, _ScanInt);

                // 3. RGB错位
                float2 shift = float2(_RGBShift * 0.01, 0);
                float r = tex2D(_MainTex, px + shift).r;
                float b = tex2D(_MainTex, px - shift).b;
                col.rgb = float3(r, col.g, b);

                // 4. 噪点
                col.rgb += random(i.uv * _Time.y) * _Noise;

                return col;
            }
            ENDCG
        }
    }
}