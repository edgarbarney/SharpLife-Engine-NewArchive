// Shared gamma correction definitions (types, constants)

//Expected inputs:
//None

const int MaxStylesPerSurface = 4;

//The maximum value for styled lights
const int StyledLightValueRangeMultiplier = (256 * MaxStylesPerSurface) - 1;

//Rescales a styled light value to the range [0, StyledLightValueRangeMultiplier]
const int StyledLightValueRangeDivisor = 16384;

const int DefaultLightScale = 256;

struct LightingInfoStruct
{
	float MainGamma;
	float TextureGamma;
	float LightingGamma;
	float Brightness;
	int LightScale;
	int OverbrightEnabled;
	int Fullbright;
};
