#version 410 core
out vec4 FragColor;
uniform vec4 outlinecolor;
in vec3 FragPos;

void main()
{
    FragColor = outlinecolor;
}
