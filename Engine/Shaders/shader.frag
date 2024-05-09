#version 330 core

out vec4 outputColor;

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

struct Light {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform Material material;
uniform vec3 viewPos;

#define MAX_LIGHTS 4
uniform int numLights;
uniform Light lights[MAX_LIGHTS];

in vec3 FragPos;
in vec3 Normal;

void main()
{
    vec3 ambient = material.ambient;

    vec3 diffuse = material.diffuse;
    vec3 specular = material.specular;
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 ambient2 = vec3(0.0);
    vec3 diffuse2 = vec3(0.0);
    vec3 specular2 = vec3(0.0);
    for (int i = 0; i < numLights; ++i) {
        // Ambient
        ambient2 += lights[i].ambient * material.ambient;

        // Diffuse
        vec3 lightDir = normalize(lights[i].position - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        diffuse2 += lights[i].diffuse * (diff * material.diffuse);

        // Specular
        vec3 reflectDir = reflect(-lightDir, norm);  
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
        specular = (spec * material.specular);
        specular2 += lights[i].specular * (spec * material.specular);
    }
    vec3 resLights = ambient2+diffuse2+specular2;
    vec3 result = ambient + diffuse + specular;
    outputColor = vec4(result, 1.0);
}