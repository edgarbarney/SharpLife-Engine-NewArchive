#version 450

layout(location = 0) in vec3 Color;
layout(location = 0) out vec4 OutputColor;

void main()
{
    OutputColor = vec4(Color, 1.0f);
}
