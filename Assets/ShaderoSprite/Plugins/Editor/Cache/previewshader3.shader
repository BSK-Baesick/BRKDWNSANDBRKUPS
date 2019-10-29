//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2018 //
/// Shader generate with Shadero 1.9.3                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Previews/PreviewXATXQ3"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_AuraDisplacementPack_ValueX_1("_AuraDisplacementPack_ValueX_1", Range(-1, 1)) = 0
_AuraDisplacementPack_ValueY_1("_AuraDisplacementPack_ValueY_1", Range(-1, 1)) = -0.08
_AuraDisplacementPack_Size_1("_AuraDisplacementPack_Size_1", Range(-3, 3)) = 1.836
AuraDisplacementPack_1("AuraDisplacementPack_1(RGB)", 2D) = "white" { }
_SpriteFade("SpriteFade", Range(0, 1)) = 1.0

// required for UI.Mask
[HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
[HideInInspector]_Stencil("Stencil ID", Float) = 0
[HideInInspector]_StencilOp("Stencil Operation", Float) = 0
[HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
[HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
[HideInInspector]_ColorMask("Color Mask", Float) = 15

}

SubShader
{

Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off

// required for UI.Mask
Stencil
{
Ref [_Stencil]
Comp [_StencilComp]
Pass [_StencilOp]
ReadMask [_StencilReadMask]
WriteMask [_StencilWriteMask]
}

Pass
{

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct appdata_t{
float4 vertex   : POSITION;
float4 color    : COLOR;
float2 texcoord : TEXCOORD0;
};

struct v2f
{
float2 texcoord  : TEXCOORD0;
float4 vertex   : SV_POSITION;
float4 color    : COLOR;
};

sampler2D _MainTex;
float _SpriteFade;
float _AuraDisplacementPack_ValueX_1;
float _AuraDisplacementPack_ValueY_1;
float _AuraDisplacementPack_Size_1;
sampler2D AuraDisplacementPack_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 OperationBlend(float4 origin, float4 overlay, float blend)
{
float4 o = origin; 
o.a = overlay.a + origin.a * (1 - overlay.a);
o.rgb = (overlay.rgb * overlay.a + origin.rgb * origin.a * (1 - overlay.a)) / (o.a+0.0000001);
o.a = saturate(o.a);
o = lerp(origin, o, blend);
return o;
}
float2 AnimatedMouvementUV(float2 uv, float offsetx, float offsety, float speed)
{
speed *=_Time*50;
uv += float2(offsetx, offsety)*speed;
uv = fmod(uv,1);
return uv;
}
float2 SimpleDisplacementUV(float2 uv,float x, float y, float value)
{
return lerp(uv,uv+float2(x,y),value);
}
float4 MorphingPerfectPack(float2 uv, sampler2D source, sampler2D source2, sampler2D source3, sampler2D source4, float blend, float StrangerValue)
{
float smooth = 0.10f;
float r = 1 - smoothstep(0.0, smooth, uv.x);
r += smoothstep(1. - smooth, 1., uv.x);
r += 1 - smoothstep(0.0, smooth, uv.y);
r += smoothstep(1 - smooth, 1., uv.y);
r = saturate(r);
float2 uv2 = tex2D(source3, uv).rg;
float2 uv3 = tex2D(source4, uv).rg;
uv2 = lerp(uv, uv2, blend);
uv3 = lerp(uv3, uv, blend);
float4 r1 = tex2D(source, uv2);
uv = lerp(uv, uv2, StrangerValue * blend - StrangerValue);
float4 r2 = tex2D(source2, uv3);
r1 = lerp(r1, r2, blend) ;
r1.a = lerp(r1.a, r2.a, blend)*(1-r);
return r1;
}
float4 ShadowLight(sampler2D source, float2 uv, float precision, float size, float4 color, float intensity, float posx, float posy,float fade)
{
int samples = precision;
int samples2 = samples *0.5;
float4 ret = float4(0, 0, 0, 0);
float count = 0;
for (int iy = -samples2; iy < samples2; iy++)
{
for (int ix = -samples2; ix < samples2; ix++)
{
float2 uv2 = float2(ix, iy);
uv2 /= samples;
uv2 *= size*0.1;
uv2 += float2(-posx,posy);
uv2 = saturate(uv+uv2);
ret += tex2D(source, uv2);
count++;
}
}
ret = lerp(float4(0, 0, 0, 0), ret / count, intensity);
ret.rgb = color.rgb;
float4 m = ret;
float4 b = tex2D(source, uv);
ret = lerp(ret, b, b.a);
ret = lerp(m,ret,fade);
return ret;
}
float2 AnimatedInfiniteZoomUV(float2 uv, float zoom2, float posx, float posy, float radius, float speed)
{
uv+=float2(posx,posy);
float2 muv = uv;
float atans = (atan2(uv.x - 0.5, uv.y - 0.5) + 3.1415) / (3.1415 * 2.);
float time = _Time * speed*10;
uv -= 0.5;
 uv *= (1. / pow(4., frac(time / 2.)));
uv += 0.5;
float2 tri = abs(1. - (uv * 2.));
 float zoom = min(pow(2., floor(-log2(tri.x))), pow(2., floor(-log2(tri.y))));
 float zoom_id = log2(zoom) + 1.;
 float div = ((pow(2., ((-zoom_id) - 1.)) * ((-2.) + pow(2., zoom_id))));
 float2 uv2 = (((uv) - (div)) * zoom);
 uv2 = lerp(muv * radius, uv2 * radius, zoom2);
 return uv2;
}
float4 AuraDisplacementPack(float2 uv,sampler2D source,float x, float y, float value, float motion, float motion2)
{
float t = _Time.y;
float2 mov = uv + (float2(x * t, y * t) * motion);
float2 mov2 = uv + (float2(x * t * 1.5, y * t * 1.5)*1.5 * motion2);
mov = lerp(uv, mov, value);
mov2 = lerp(uv, mov2, value);
float4 rgba = tex2D(source, mov);
float4 rgba2 = tex2D(source, mov2);
rgba.rgb = rgba.rgb * rgba2.rgb*8*value;
return rgba;
}
float4 frag (v2f i) : COLOR
{
float4 _AuraDisplacementPack_1 = AuraDisplacementPack(i.texcoord,AuraDisplacementPack_1,_AuraDisplacementPack_ValueX_1,_AuraDisplacementPack_ValueY_1,_AuraDisplacementPack_Size_1,1,1);
float4 FinalResult = _AuraDisplacementPack_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
