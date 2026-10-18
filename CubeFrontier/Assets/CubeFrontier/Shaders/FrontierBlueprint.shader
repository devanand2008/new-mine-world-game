Shader "CubeFrontier/Blueprint"
{
 Properties {_Tint("Tint",Color)=(.2,1,.7,.18)}
 SubShader {
 Tags {"RenderType"="Transparent" "Queue"="Transparent+20" "RenderPipeline"="UniversalPipeline"}
 Pass {
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 float4 _Tint;
 struct A{float4 pos:POSITION;float3 normal:NORMAL;};struct V{float4 pos:SV_POSITION;float3 normal:TEXCOORD0;};
 V Vert(A a){V o;o.pos=TransformObjectToHClip(a.pos.xyz);o.normal=TransformObjectToWorldNormal(a.normal);return o;}
 half4 Frag(V i):SV_Target{return half4(_Tint.rgb*(.65+.35*saturate(i.normal.y)),_Tint.a);}
 ENDHLSL
 }
 }
}
