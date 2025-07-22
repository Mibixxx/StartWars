Shader "URP/LitDissolve"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Base Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0

        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _DissolveWidth ("Dissolve Width", Range(0,0.1)) = 0.02
        _DissolveColor ("Dissolve Edge Color", Color) = (1,0.5,0,1)
        _DissolveEmission ("Dissolve Emission", Range(0,3)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float2 uv : TEXCOORD0;
            };

            sampler2D _BaseMap;
            float4 _BaseColor;

            sampler2D _NormalMap;
            float _NormalStrength;

            sampler2D _DissolveMap;
            float _DissolveAmount;
            float _DissolveWidth;
            float4 _DissolveColor;
            float _DissolveEmission;

            float _Metallic;
            float _Smoothness;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangentOS.w;
                OUT.uv = IN.uv;
                return OUT;
            }

            float3 GetNormalWS(Varyings IN)
            {
                float3 normalTS = UnpackNormal(tex2D(_NormalMap, IN.uv)) * _NormalStrength;
                float3x3 TBN = float3x3(normalize(IN.tangentWS), normalize(IN.bitangentWS), normalize(IN.normalWS));
                return normalize(mul(normalTS, TBN));
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float dissolveVal = tex2D(_DissolveMap, IN.uv).r;

                if (dissolveVal < _DissolveAmount)
                    discard;

                float isEdge = step(_DissolveAmount, dissolveVal) * step(dissolveVal, _DissolveAmount + _DissolveWidth);

                float4 baseColor = tex2D(_BaseMap, IN.uv) * _BaseColor;

                // Prepare SurfaceData
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.alpha = baseColor.a;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = float3(0, 0, 0);
                surfaceData.normalTS = UnpackNormal(tex2D(_NormalMap, IN.uv)) * _NormalStrength;
                surfaceData.clearCoatMask = 0.0;
                surfaceData.clearCoatSmoothness = 0.0;

                float3 normalWS = GetNormalWS(IN);
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.positionWS);

                // Prepare InputData for PBR
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = float4(0, 0, 0, 0);
                inputData.fogCoord = 0;
                inputData.vertexLighting = surfaceData.albedo * 0.5;
                inputData.bakedGI = surfaceData.albedo * 0.5;
                inputData.normalizedScreenSpaceUV = float2(0, 0);
                inputData.shadowMask = float4(1, 1, 1, 1);

                half4 color = UniversalFragmentPBR(inputData, surfaceData);

                // Edge glow
                if (isEdge > 0)
                {
                    color.rgb = _DissolveColor.rgb * _DissolveEmission;
                }

                return color;
            }

            ENDHLSL
        }
    }
    FallBack Off
}
