Shader "CubeFrontier/Glass"
{
 Properties {_BaseMap("Block atlas",2D)="white"{}}
 SubShader {
 Tags {"RenderType"="Transparent" "Queue"="Transparent+5" "RenderPipeline"="UniversalPipeline"}
 Pass {
 Tags {"LightMode"="UniversalForward"}
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 struct A{float4 pos:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
 struct V{float4 pos:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;float2 uv:TEXCOORD2;half fog:TEXCOORD3;};
 V Vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.pos.xyz);o.pos=p.positionCS;o.world=p.positionWS;o.normal=TransformObjectToWorldNormal(a.normal);o.uv=a.uv;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
 half4 Frag(V i):SV_Target {
 half3 color=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
 float2 tile=frac(i.uv*float2(8,16));float edge=min(min(tile.x,1-tile.x),min(tile.y,1-tile.y));
 half fresnel=pow(1-abs(dot(normalize(i.normal),GetWorldSpaceNormalizeViewDir(i.world))),4);
 half opacity=lerp(.21,.92,1-step(.07,edge))+fresnel*.15;
 return half4(MixFog(color*.82+fresnel*.13,i.fog),opacity);
 }
 ENDHLSL
 }
 }
}
