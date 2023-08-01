#version 450

struct WorldAndInverseMatrices
{
    mat4 World;
    mat4 InverseWorld;
};

layout(set = 0, binding = 0) uniform Projection
{
    mat4 _Proj;
};

layout(set = 0, binding = 1) uniform View
{
    mat4 _View;
};

layout(set = 0, binding = 2) uniform WorldAndInverse
{
    WorldAndInverseMatrices _WorldAndInverse;
};

layout(location = 0) in vec3 vsin_Position;
layout(location = 1) in vec2 TextureCoords;
layout(location = 2) in vec2 LightmapCoords;
layout(location = 3) in float LightmapXOffset;
layout(location = 4) in ivec4 StyleIndices;

layout(location = 0) out vec2 fsin_TextureCoords;
layout(location = 1) out vec2 fsin_LightmapCoords;
layout(location = 2) out flat float fsin_LightmapXOffset;
layout(location = 3) out flat ivec4 fsin_StyleIndices;

void main()
{
    gl_Position = _Proj * _View * _WorldAndInverse.World * vec4(vsin_Position, 1.0f);
	fsin_TextureCoords = TextureCoords;
	fsin_LightmapCoords = LightmapCoords;
	fsin_LightmapXOffset = LightmapXOffset;
	fsin_StyleIndices = StyleIndices;
}
