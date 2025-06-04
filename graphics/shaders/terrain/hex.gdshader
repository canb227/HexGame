shader_type spatial;


uniform float gameBoardWidth;
uniform float gameBoardHeight;

//uniform sampler2D visibilityGrid : filter_nearest;
uniform sampler2D heightMap : filter_linear;

varying flat vec2 hexValue;
varying vec3 hexWorldPos;

void vertex() {
    hexWorldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    //VERTEX.y += (texture(terrainNoise, hexWorldPos.xz/1000.0).r * 100.0) * texture(terrainInfo, hexValue.xy).g;
}

void fragment() {
    //vec2 visData = texture(visibilityGrid, hexValue.xy).rg;

    ALBEDO = texture(heightMap, hexWorldPos.xz/400.0).rgb;

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
