#version 450

#include "shared/GammaCorrectionDefs.frag.inc"
#include "shared/RenderMode.inc"

const float NoLightStyle = 255;
const int MaxLightStyles = 64;
const float FullbrightValue = 1.0;
const float OverbrightColorMultiplier = 2.0 / 3.0;

layout(set = 0, binding = 3) uniform LightingInfo
{
	LightingInfoStruct _LightingInfo;
};

layout(set = 0, binding = 4) uniform LightStyles
{
	int _LightStyles[MaxLightStyles];
};

layout(set = 0, binding = 5) uniform RenderColor
{
	vec4 _RenderColor;
	int _RenderMode;
};

layout(set = 1, binding = 0) uniform texture2D Texture;
layout(set = 1, binding = 1) uniform sampler Sampler;

layout(set = 2, binding = 0) uniform texture2D Lightmaps;

layout(location = 0) in vec2 TextureCoords;
layout(location = 1) in vec2 LightmapCoords;
layout(location = 2) in flat float LightmapXOffset;
layout(location = 3) in flat ivec4 StyleIndices;

layout(location = 0) out vec4 OutputColor;

#include "shared/GammaCorrection.frag.inc"

//Lighting data uses [0, 255] range values for increased precision
ivec3 GetLightData(int styleIndex)
{	
	vec2 lightmapCoords = LightmapCoords;
	lightmapCoords.x += LightmapXOffset * styleIndex;

	return ivec3(texture(sampler2D(Lightmaps, Sampler), lightmapCoords).rgb * 255) * _LightStyles[StyleIndices[styleIndex]];
}

vec3 CalculateLightData()
{
	if (_LightingInfo.Fullbright != 0)
	{
		return vec3(FullbrightValue);
	}
	else
	{
		//All surfaces have at least one style
		ivec3 lightData = GetLightData(0);
		
		//There is never a style following a NoLightStyle value
		for (int i = 1; i < MaxStylesPerSurface && StyleIndices[i] != NoLightStyle; ++i)
		{
			lightData += GetLightData(i);
		}
		
		//Apply light scale modifier
		lightData *= _LightingInfo.LightScale;
		
		//Clamp values to maximum
		for (int i = 0; i < 3; ++i)
		{
			lightData[i] = min(StyledLightValueRangeMultiplier, lightData[i] / StyledLightValueRangeDivisor);
		}
		
		//Normalize the value from [0, StyledLightValueRangeMultiplier] to [0, 1]
		vec3 finalLightData = LightingGamma(lightData) / (MaxStylesPerSurface * 255);
		
		return finalLightData;
	}
}

void main()
{
	//Color render mode ignores all texture and lighting data, and is not gamma corrected
	if (_RenderMode == RenderModeTransColor)
	{
		OutputColor = _RenderColor;
		return;
	}
	
	vec4 color = texture(sampler2D(Texture, Sampler), TextureCoords) * _RenderColor;
	
	//Discard early if the fragment is invisible
	if (_RenderMode == RenderModeTransAlpha && color.a <= 0.25)
	{
		discard;
	}

	//Gamma correction
	color.rgb = TextureGamma(color.rgb);
	
	vec3 finalLightData = CalculateLightData();
	
	vec4 finalColor = color * vec4(finalLightData, 1.0);
	
	if (_LightingInfo.OverbrightEnabled != 0)
	{
		finalColor = OverbrightColorMultiplier * (2 * finalColor);
	}
	
	OutputColor = finalColor;
}
