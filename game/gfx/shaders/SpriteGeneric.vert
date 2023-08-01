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

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TextureCoords;

layout(location = 0) out vec2 fsin_TextureCoords;

void main()
{
    gl_Position = _Proj * _View * _WorldAndInverse.World * vec4(Position, 1.0f);
	fsin_TextureCoords = TextureCoords;
}
