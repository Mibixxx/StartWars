Shader "Custom/BuildingEffectURP"
{
    Properties
    {
        _MainTex ("Diffuse", 2D) = "white" {}
        _Progress ("Progress", Range(0,100)) = 100
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Progress;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Usa il colore del vertice per calcolo clip come nel vecchio shader:
                float temp_output = (IN.color.r + IN.color.g * 100 + IN.color.b * 10000) * 10.0;

                float dist = distance(float3(temp_output, temp_output, temp_output), float3(0,0,0));
                float clipVal = saturate(1.0 - ((dist - 0.0025) / max(25.0, 1e-5)));

                if ((clipVal / 0.01) - _Progress < 0)
                    discard;

                return col;
            }
            ENDHLSL
        }
    }
}
