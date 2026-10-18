Shader "CubeFrontier/Voxel"
{
    Properties { _BaseMap("Original block atlas", 2D) = "white" {} _BaseColor("Tint", Color) = (1,1,1,1) _Cutoff("Cutoff", Float) = 0.5 }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _FrontierAmbient;
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; half4 _BaseColor; float _Cutoff;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; float2 uv : TEXCOORD2; half4 color : COLOR; half fog : TEXCOORD3; };
            Varyings Vert(Attributes v)
            {
                Varyings o; VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS; o.positionWS = p.positionWS; o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv; o.color = v.color; o.fog = ComputeFogFactor(p.positionCS.z); return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                Light sun = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * i.color.rgb;
                half3 ambient = max(_FrontierAmbient.rgb, half3(.10, .13, .17));
                half diffuse = saturate(dot(normalize(i.normalWS), sun.direction));
                half3 lit = albedo * (ambient + sun.color * diffuse * sun.shadowAttenuation + i.color.a);
                return half4(MixFog(lit, i.fog), 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
