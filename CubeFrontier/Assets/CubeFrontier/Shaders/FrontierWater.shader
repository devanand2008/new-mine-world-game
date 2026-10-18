Shader "CubeFrontier/Water"
{
 SubShader {
 Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
 Pass {
 Tags { "LightMode"="UniversalForward" }
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A{float4 pos:POSITION;float3 normal:NORMAL;};
 struct V{float4 pos:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;half fog:TEXCOORD2;};
 float4 _FrontierAmbient;
 V Vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.pos.xyz);o.pos=p.positionCS;o.world=p.positionWS;o.normal=TransformObjectToWorldNormal(a.normal);o.fog=ComputeFogFactor(p.positionCS.z);return o;}
 half4 Frag(V i):SV_Target {
 float2 p=i.world.xz;float waves=sin(p.x*3.7+p.y*2.1+_Time.y*.8)*sin(p.y*5.1-p.x*1.2-_Time.y*.55);
 half3 n=normalize(i.normal+half3(waves*.08,0,cos(p.x*2+_Time.y)*.08));
 half fresnel=pow(1-saturate(dot(n,GetWorldSpaceNormalizeViewDir(i.world))),3);
 Light sun=GetMainLight();half shine=pow(saturate(dot(reflect(-sun.direction,n),GetWorldSpaceNormalizeViewDir(i.world))),80);
 half3 color=lerp(half3(.025,.30,.36),half3(.35,.72,.75),fresnel)+step(.77,waves)*.10+sun.color*shine*.5;
 color*=max(.28,dot(_FrontierAmbient.rgb,half3(.5,.8,.3)));
 return half4(MixFog(color,i.fog),.66+fresnel*.21);
 }
 ENDHLSL
 }
 }
}
