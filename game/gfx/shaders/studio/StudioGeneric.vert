#version 450

const int MaxBones = 128;

#include "StudioDefs.inc"

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

layout(set = 0, binding = 3) uniform Bones
{
	mat4 _Bones[MaxBones];
};

layout(set = 0, binding = 4) uniform RenderArguments
{
	StudioRenderArgumentsStruct _RenderArguments;
};

layout(set = 0, binding = 5) uniform TextureData
{
	StudioTextureDataStruct _TextureData;
};

layout(location = 0) in vec3 vsin_Position;
layout(location = 1) in vec2 TextureCoords;
layout(location = 2) in vec3 Normal;
layout(location = 3) in int VertexBoneIndex;
layout(location = 4) in int NormalBoneIndex;

layout(location = 0) out vec2 fsin_TextureCoords;
layout(location = 1) out vec3 fsin_Normal;
layout(location = 2) out float fsin_Illumination;

// rotate by the inverse of the matrix
vec3 VectorIRotate(vec3 value, mat4 matrix)
{
	vec3 result;
	
	result[0] = value[0] * matrix[0][0] + value[1] * matrix[1][0] + value[2] * matrix[2][0];
	result[1] = value[0] * matrix[0][1] + value[1] * matrix[1][1] + value[2] * matrix[2][1];
	result[2] = value[0] * matrix[0][2] + value[1] * matrix[1][2] + value[2] * matrix[2][2];
	
	return result;
}

float StudioLighting(int bone, vec3 normal)
{
	float illum = _RenderArguments.GlobalLight.Ambient;

	if(_TextureData.FlatShade != 0)
	{
		illum += _RenderArguments.GlobalLight.Shade * 0.8;
	}
	else
	{
		vec3 lightVector = _RenderArguments.GlobalLight.Normal;
		
		if (bone != -1)
		{
			lightVector = VectorIRotate(lightVector, _Bones[bone]);//(inverse(_Bones[bone]) * vec4(lightVector, 1)).xyz;
		}
		
		float lightcos = dot(normal, lightVector); // -1 colinear, 1 opposite

		lightcos = min(1, lightcos);

		illum += _RenderArguments.GlobalLight.Shade;

		const float r = max(1.0, v_lambert1);

		lightcos = (lightcos + (r - 1.0)) / r; 		// do modified hemispherical lighting

		if(lightcos > 0.0)
		{
			illum -= _RenderArguments.GlobalLight.Shade * lightcos;
		}

		illum = max(0, illum);
	}

	illum = min(255, illum);

	return illum / 255.0;	// Light from 0 to 1.0
}

void main()
{
    gl_Position = _Proj * _View * _WorldAndInverse.World * vec4((_Bones[VertexBoneIndex] * vec4(vsin_Position, 1.0f)).xyz, 1.0);
	fsin_TextureCoords = TextureCoords;
	fsin_Normal = Normal;
	fsin_Illumination = StudioLighting(NormalBoneIndex, Normal);
}
