Shader "CubeFrontier/Vista"
{
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 pos:POSITION; half3 normal:NORMAL; half4 color:COLOR; };
            struct V { float4 pos:SV_POSITION; half3 normal:TEXCOORD0; half3 color:COLOR; };
            V Vert(A a) { V o; o.pos=TransformObjectToHClip(a.pos.xyz); o.normal=TransformObjectToWorldNormal(a.normal); o.color=a.color.rgb; return o; }
            half4 Frag(V i):SV_Target {
                half lighting=.65+.35*saturate(dot(normalize(i.normal),normalize(half3(-.4,1,.6))));
                return half4(i.color*lighting,1);
            }
            ENDHLSL
        }
    }
}
