#version 450

#include "StudioDefs.inc"
#include "../shared/GammaCorrectionDefs.frag.inc"

layout(set = 0, binding = 4) uniform RenderArguments
{
	StudioRenderArgumentsStruct _RenderArguments;
};

layout(set = 0, binding = 6) uniform sampler Sampler;

layout(set = 0, binding = 7) uniform LightingInfo
{
	LightingInfoStruct _LightingInfo;
};

layout(set = 1, binding = 0) uniform texture2D Texture;

layout(location = 0) in vec2 TextureCoords;
layout(location = 1) in vec3 Normal;
layout(location = 2) in float Illumination;

layout(location = 0) out vec4 OutputColor;

#include "../shared/GammaCorrection.frag.inc"

//TODO: implement elights
vec3 LightLambert(vec3 normal, vec3 src)
{
	//if (!numlights )
	{
		return src;
	}

	/*
	vec3 value( 0, 0, 0 );

	for( int i = 0; i < numlights; ++i )
	{
		const auto dot = -( normal[ 0 ] * ( *light )[ 0 ] + normal[ 1 ] * ( *light )[ 1 ] + normal[ 2 ] * ( *light )[ 2 ] );

		if( dot > 0.0 )
		{
			if( ( *light )[ 3 ] == 0.0 )
			{
				const auto lengthSquared = ( *light )[ 0 ] * ( *light )[ 0 ] + ( *light )[ 1 ] * ( *light )[ 1 ] + ( *light )[ 2 ] * ( *light )[ 2 ];

				if( lengthSquared > 0.0 )
				{
					( *light )[ 3 ] = locallightR2[ i ] / ( lengthSquared * sqrt( lengthSquared ) );
				}
				else
				{
					( *light )[ 3 ] = 1.0f;
				}
			}

			const auto strength = dot * ( *light )[ 3 ];
			value.x += locallinearlight[ i ][ 0 ] * strength;
			value.y += locallinearlight[ i ][ 1 ] * strength;
			value.z += locallinearlight[ i ][ 2 ] * strength;
		}
	}

	if( value.x == 0.0 && value.y == 0.0 && value.z == 0.0 )
	{
		lambert = src;
		return;
	}

	for( int i = 0; i < 3; ++i )
	{
		lambert[ i ] = min( ( int ) ( value[ i ] + ( int ) ( src[ i ] * 1023.0 ) ), 1023 ) / 1023.0f;
	}
	*/
}

void main()
{
	vec4 textureColor = texture(sampler2D(Texture, Sampler), TextureCoords);

	//TODO: only do alpha test for masked textures
	if (textureColor.a <= 0.5)
	{
		discard;
	}
	
	vec3 lightValue = LightLambert(Normal, _RenderArguments.GlobalLight.Color * Illumination);
	
	//Gamma correction
	textureColor.rgb = TextureGamma(textureColor.rgb);
	lightValue = LightingGamma(ivec3(lightValue * StyledLightValueRangeMultiplier)) / float(StyledLightValueRangeMultiplier);
	
	vec4 color = textureColor * vec4(lightValue, 1) * _RenderArguments.RenderColor;
	
	OutputColor = color;
}
