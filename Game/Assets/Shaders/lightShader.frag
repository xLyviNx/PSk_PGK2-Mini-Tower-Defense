#version 410 core
out vec4 FragColor;
uniform vec3 lightcolor;
in vec3 FragPos;
in vec3 Normal;
in vec3 TexCoord;

void main()
{
    FragColor = vec4(lightcolor, 1.0);
}
