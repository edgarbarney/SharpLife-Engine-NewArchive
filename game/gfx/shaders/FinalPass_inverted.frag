#version 450

layout(set = 0, binding = 0) uniform texture2D SourceTexture;
layout(set = 0, binding = 1) uniform sampler SourceSampler;

layout(location = 0) in vec2 TexCoords;
layout(location = 0) out vec4 OutputColor;

void main()
{
    OutputColor = clamp(texture(sampler2D(SourceTexture, SourceSampler), TexCoords), 0, 1);
}
