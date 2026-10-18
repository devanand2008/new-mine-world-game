Shader "CubeFrontier/Art"
{
 Properties { _BaseMap("Map",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1) _Cutoff("Cutoff",Float)=.5 _FogWeight("Fog",Float)=1 }
 SubShader {
 Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 Pass {
 Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 float4 _FrontierAmbient;
 float _FogWeight;
 struct A { float4 pos:POSITION; float3 normal:NORMAL; half4 color:COLOR; };
 struct V { float4 pos:SV_POSITION; float3 world:TEXCOORD0; half3 normal:TEXCOORD1; half4 color:COLOR; half fog:TEXCOORD2; };
 V Vert(A a) { V o; VertexPositionInputs p=GetVertexPositionInputs(a.pos.xyz); o.pos=p.positionCS; o.world=p.positionWS; o.normal=TransformObjectToWorldNormal(a.normal); o.color=a.color; o.fog=ComputeFogFactor(p.positionCS.z); return o; }
 half4 Frag(V i):SV_Target {
 Light light=GetMainLight(TransformWorldToShadowCoord(i.world));
 half diffuse=saturate(dot(normalize(i.normal),light.direction));
 half3 ambient=max(_FrontierAmbient.rgb,half3(.32,.36,.42));
 half3 color=i.color.rgb*(ambient+light.color*diffuse*light.shadowAttenuation)+i.color.rgb*i.color.a*1.4;
 return half4(lerp(color,MixFog(color,i.fog),_FogWeight),1);
 }
 ENDHLSL
 }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
