#version 410 core
out vec4 FragColor;

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

#define MAX_LIGHTS 8
uniform int numLights;
uniform Light lights[MAX_LIGHTS];
    
in vec3 FragPos;
in vec3 Normal;
in vec3 TexCoord;

void main()
{
    vec3 ambient = vec3(0.0);
    vec3 diffuse = vec3(0.0);
    vec3 specular = vec3(0.0);

    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    for (int i = 0; i < numLights; ++i) {
        // Ambient
        ambient += lights[i].ambient * material.ambient;

        vec3 lightDir = normalize(lights[i].position - FragPos);  
        float diff = max(dot(norm, lightDir), 0.0);
        diffuse += lights[i].diffuse * (diff * material.diffuse);

        // Specular
        vec3 reflectDir = reflect(-lightDir, norm);  
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
        specular += lights[i].specular * (spec * material.specular);
    }
    vec3 result = ambient + diffuse + specular;
     //result = specular;
    FragColor = vec4(result, 1.0);
}
