//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2018 //
/// Shader generate with Shadero 1.9.3                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Previews/PreviewXATXQ2"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_NewTex_1("NewTex_1(RGB)", 2D) = "white" { }
_Generate_Donut_PosX_1("_Generate_Donut_PosX_1", Range(-1, 2)) = 0.5
_Generate_Donut_PosY_1("_Generate_Donut_PosY_1", Range(-1, 2)) = 0.5
_Generate_Donut_Size_1("_Generate_Donut_Size_1", Range(-2, 2)) = 0.592
_Generate_Donut_SizeDonut_1("_Generate_Donut_SizeDonut_1", Range(-2, 2)) = -0.55
_Generate_Donut_SizeSmooth_1("_Generate_Donut_SizeSmooth_1", Range(0, 1)) = 0.159
_MaskRGBA_Fade_1("_MaskRGBA_Fade_1", Range(0, 1)) = 0
_ThresholdSmooth_Value_1("_ThresholdSmooth_Value_1", Range(-1, 2)) = 0.146
_ThresholdSmooth_Smooth_1("_ThresholdSmooth_Smooth_1", Range(0, 1)) = 0.114
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
sampler2D _NewTex_1;
float _Generate_Donut_PosX_1;
float _Generate_Donut_PosY_1;
float _Generate_Donut_Size_1;
float _Generate_Donut_SizeDonut_1;
float _Generate_Donut_SizeSmooth_1;
float _MaskRGBA_Fade_1;
float _ThresholdSmooth_Value_1;
float _ThresholdSmooth_Smooth_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 InverseColor(float4 txt, float fade)
{
float3 gs = 1 - txt.rgb;
return lerp(txt, float4(gs, txt.a), fade);
}
float2 PolarCoordinatesUV(float2 uv, float size)
{
float2 r = uv - float2(0.5, 0.5);
uv.y = sqrt(r.x * r.x + r.y * r.y);
uv.y /= 0.318471;
uv.y = 1.0 - uv.y;
uv.x = atan2(r.y, r.x);
uv.x -= 1.57079632679;
if (uv.x < 0.0) { uv.x += 6.28318530718; }
uv.x /= 6.28318530718;
uv.x = 1.0 - uv.x;
return uv;
}
float4 Color_Gradients2(float4 txt, float2 uv, float4 col1, float4 col2)
{
float4 c1 = lerp(col1, col2, smoothstep(0., 1, uv.x));
return c1;
}
float4 ThresholdSmooth(float4 txt, float value, float smooth)
{
float l = (txt.x + txt.y + txt.z) * 0.33;
txt.rgb = smoothstep(value, value + smooth, l);
return txt;
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

float4 TurnAlphaToBlack(float4 txt,float fade)
{
float3 gs = lerp(txt.rgb,float3(0,0,0), 1-txt.a);
return lerp(txt,float4(gs, 1), fade);
}

float4 Generate_Donut(float2 uv, float posx, float posy, float size, float sizedonut, float smooth, float black)
{
uv -= float2(posx, posy);
float l = length(uv*2);
float4 d = smoothstep(size, size + smooth, l);
d *= smoothstep(size+sizedonut, size + sizedonut + smooth, 1-l);
d.a = saturate(d + black);
return d;
}
float2 LiquidUV(float2 p, float WaveX, float WaveY, float DistanceX, float DistanceY, float Speed)
{ Speed *= _Time * 100;
float x = sin(p.y * 4 * WaveX + Speed);
float y = cos(p.x * 4 * WaveY + Speed);
x += sin(p.x)*0.1;
y += cos(p.y)*0.1;
x *= y;
y *= x;
x *= y + WaveY*8;
y *= x + WaveX*8;
p.x = p.x + x * DistanceX * 0.015;
p.y = p.y + y * DistanceY * 0.015;

return p;
}
float4 frag (v2f i) : COLOR
{
float4 NewTex_1 = tex2D(_NewTex_1, i.texcoord);
float4 _Generate_Donut_1 = Generate_Donut(i.texcoord,_Generate_Donut_PosX_1,_Generate_Donut_PosY_1,_Generate_Donut_Size_1,_Generate_Donut_SizeDonut_1,_Generate_Donut_SizeSmooth_1,1);
NewTex_1.a = lerp(_Generate_Donut_1.r, 1 - _Generate_Donut_1.r ,_MaskRGBA_Fade_1);
float4 _ThresholdSmooth_1 = ThresholdSmooth(NewTex_1,_ThresholdSmooth_Value_1,_ThresholdSmooth_Smooth_1);
float4 FinalResult = _ThresholdSmooth_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
FinalResult.rgb *= FinalResult.a;
FinalResult.a = saturate(FinalResult.a);
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
