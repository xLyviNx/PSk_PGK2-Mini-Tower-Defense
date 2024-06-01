#version 410 core
out vec4 FragColor;

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
    float transparency;
};

struct Light {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform Material material;
uniform vec3 viewPos;
uniform int specularAlwaysVisible;
#define MAX_LIGHTS 8
uniform int numLights;
uniform Light lights[MAX_LIGHTS];
uniform float gamma = 1.0;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;
uniform sampler2D texture_diffuse1;

vec3 gammaCorrect(vec3 color)
{
    return pow(color, vec3(1.0 / gamma));
}

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

        // Diffuse
        vec3 lightDir = normalize(lights[i].position - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        diffuse += lights[i].diffuse * (diff * material.diffuse * lights[i].diffuse);

        // Specular (Blinn-Phong)
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float spec = pow(max(dot(norm, halfwayDir), 0.0), material.shininess);
        specular += lights[i].specular * (spec * material.specular * lights[i].specular);
    }

    vec3 result = ambient + diffuse + specular;
    float specularIntensity = length(specular);
    float specularAlpha = specularIntensity > 0.0 ? specularIntensity : 0.0;
    float transparency = (specularAlwaysVisible == 1 ? specularAlpha : 0.0) + (texture(texture_diffuse1, TexCoord).a * material.transparency);

    if (transparency < 0.1)
        discard;

    // Apply gamma correction
    result = gammaCorrect(result);
    specular = gammaCorrect(specular);

    if (specularAlwaysVisible == 1)
        FragColor = vec4(result, transparency) + vec4(specular, specularAlpha);
    else
        FragColor = vec4(result + specular, transparency);
}
