shader_type spatial;


uniform float widthDiv;
uniform float heightDiv;
uniform float chunkOffset;

uniform sampler2D visibilityGrid : filter_nearest;
uniform sampler2D heightMap : filter_linear;

varying flat vec2 hexValue;
varying vec3 hexWorldPos;

void vertex() {
    hexWorldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    float hoffset = sqrt(3) * 10.0 / 2.0;
    float voffset = 1.5 * 10.0 / 2.0;
    VERTEX.y += texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.0)/widthDiv, (hexWorldPos.x+voffset-1.0)/heightDiv)).r * 10.0;// * texture(terrainInfo, hexValue.xy).r;
}

void fragment() {
    vec2 visData = texture(visibilityGrid, hexValue.xy).rg;
    float hoffset = sqrt(3) * 10.0 / 2.0;
    float voffset = 1.5 * 10.0 / 2.0;

    ALBEDO = texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.0)/widthDiv, (hexWorldPos.x+voffset-1.0)/heightDiv)).rgb;
    // Calculate Visibility Changes
    //float grayscaleFactor = dot(blendedTerrain.rgb, vec3(0.299, 0.587, 0.114)) * 0.5;
    //
    //vec3 grayscaleColor = vec3(grayscaleFactor);
    //vec3 visibleColor = blendedTerrain.rgb;
    //vec3 hiddenColor = vec3(0.0); // Fully hidden
    //
    //ALBEDO = hiddenColor * (1.0 - visData.r) * (1.0 - visData.g)  // Hidden
    //       + visibleColor * visData.r                             // Fully visible
    //       + grayscaleColor * visData.g;                          // Grayscale if seen
}
