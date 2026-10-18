Shader "CubeFrontier/Sky"
{
 SubShader {
 Tags { "Queue"="Background" "RenderType"="Background" "RenderPipeline"="UniversalPipeline" "PreviewType"="Skybox" }
 Cull Off ZWrite Off
 Pass {
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 float4 _FrontierSky,_FrontierHorizon,_FrontierSun;
 struct A{float4 pos:POSITION;};struct V{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 V Vert(A a){V o;o.pos=TransformObjectToHClip(a.pos.xyz);o.dir=a.pos.xyz;return o;}
 half4 Frag(V i):SV_Target {
 float3 d=normalize(i.dir),sun=normalize(_FrontierSun.xyz);
 half3 color=lerp(_FrontierHorizon.rgb,_FrontierSky.rgb,pow(saturate(d.y),.55));
 float3 right=normalize(cross(sun,float3(0,0,1))),up=cross(right,sun);
 float square=step(abs(dot(d,right)),.032)*step(abs(dot(d,up)),.032)*step(.95,dot(d,sun));
 color+=half3(1,.87,.59)*square*1.5;
 float night=saturate(-sun.y*4);
 float moon=step(abs(dot(d,right)),.024)*step(abs(dot(d,up)),.024)*step(.95,dot(d,-sun));
 float crater=step(.004,dot(d,right))*step(.003,dot(d,up));
 color+=half3(.52,.71,1)*moon*(1-crater*.28)*night;
 float2 starUV=float2(atan2(d.z,d.x),asin(clamp(d.y,-1,1)))*150;
 float starHash=frac(sin(dot(floor(starUV),float2(127.1,311.7)))*43758.5453);
 float stars=step(.997,starHash)*step(max(abs(frac(starUV.x)-.5),abs(frac(starUV.y)-.5)),.17)*smoothstep(.05,.35,d.y);
 color+=half3(.65,.80,1)*stars*night;
 color+=half3(.34,.18,.06)*pow(saturate(dot(d,sun)),32)*saturate(1-abs(sun.y)*3);
 return half4(color,1);
 }
 ENDHLSL
 }
 }
}
