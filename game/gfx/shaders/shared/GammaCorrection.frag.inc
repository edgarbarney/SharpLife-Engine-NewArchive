// Shared gamma correction functions
//Include GammaCorrectionDefs.frag before defining the uniform

//Expected inputs:
//uniform LightingInfoStruct _LightingInfo

//Compute the gamma value for the given texture value
vec3 TextureGamma(vec3 value)
{
	float gamma = _LightingInfo.TextureGamma * (1.0 / _LightingInfo.MainGamma);
	
	for (int i = 0; i < 3; ++i)
	{
		value[i] = clamp(pow(value[i], gamma), 0.0, 1.0);
	}
	
	return value;
}

//Compute the gamma value for the given lighting value
//Expects inputs to be in the range [0, StyledLightValueRangeMultiplier]
vec3 LightingGamma(ivec3 value)
{
	float gamma = 1.0 / _LightingInfo.MainGamma;
	
	//Clamp brightness to 1.0 for this calculation
	float g3Brightness = min(1.0, _LightingInfo.Brightness);

	float g3 = 0.125 - g3Brightness * g3Brightness * 0.075;

	for (int i = 0; i < 3; ++i)
	{
		float f = pow(value[i] / float(StyledLightValueRangeMultiplier), _LightingInfo.LightingGamma);

		if (_LightingInfo.Brightness > 1.0)
		{
			f *= _LightingInfo.Brightness;
		}
		
		float base;

		if (g3 >= f)
		{
			base = (f / g3) * 0.125;
		}
		else
		{
			base = (((f - g3) / (1.0 - g3)) * 0.875) + 0.125;
		}

		value[i] = max(0, int(pow(base, gamma) * StyledLightValueRangeMultiplier));
	}
	
	return value;
}
