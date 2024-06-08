#version 410 core
out vec4 FragColor;

uniform vec3 gridColor;
uniform float gridSpacing;
uniform float gridWidth;

in vec3 FragPos;

float gridPattern(vec3 worldPos, float gridSpacing, float gridWidth)
{
    vec3 coord = fract(worldPos / gridSpacing);
    float edge = gridWidth / gridSpacing; // Normalize gridWidth relative to gridSpacing
    bool isGridLine = (coord.x < edge) || (coord.y < edge) || (coord.z < edge);
    return isGridLine ? 1.0 : 0.0;
}

void main()
{
    float grid = gridPattern(FragPos, gridSpacing, gridWidth);
    vec3 color = gridColor * grid;
    float alpha = grid; // Alpha is 1.0 for grid lines and 0.0 for non-grid areas
    FragColor = vec4(color, alpha);
}
