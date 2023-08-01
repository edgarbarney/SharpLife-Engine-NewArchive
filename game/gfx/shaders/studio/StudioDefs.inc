//modifier for pseudo-hemispherical lighting
const float v_lambert1 = 1.5;

struct AmbientLightStruct
{
	vec3 Color;
	int Ambient;
	
	vec3 Normal;
	int Shade;
};

struct StudioRenderArgumentsStruct
{
	vec4 RenderColor;
	AmbientLightStruct GlobalLight;
};

struct StudioTextureDataStruct
{
	int FlatShade;
};