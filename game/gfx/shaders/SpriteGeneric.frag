#version 450

#include "shared/GammaCorrectionDefs.frag.inc"

layout(set = 0, binding = 3) uniform texture2D Texture;
layout(set = 0, binding = 4) uniform sampler Sampler;

layout(set = 0, binding = 5) uniform LightingInfo
{
	LightingInfoStruct _LightingInfo;
};

layout(set = 0, binding = 6) uniform RenderColor
{
	vec4 _RenderColor;
};

layout(location = 0) in vec2 TextureCoords;

layout(location = 0) out vec4 OutputColor;

#include "shared/GammaCorrection.frag.inc"

void main()
{
	vec4 color = texture(sampler2D(Texture, Sampler), TextureCoords);
	
	//Gamma correction
	color.rgb = TextureGamma(color.rgb);

	OutputColor = color * _RenderColor;
}
