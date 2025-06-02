shader_type spatial;


uniform float gameBoardWidth;
uniform float gameBoardHeight;

uniform sampler2D visibilityGrid : filter_nearest;
uniform sampler2D terrainInfo : filter_nearest;
uniform sampler2D terrainNoise: filter_linear;

uniform sampler2D flatArctic; 
uniform sampler2D flatTundra; 
uniform sampler2D flatGrassland; 
uniform sampler2D flatPlain; 
uniform sampler2D flatDesert;
uniform sampler2D roughArctic;
uniform sampler2D roughTundra;
uniform sampler2D roughGrassland;
uniform sampler2D roughPlain;
uniform sampler2D roughDesert;
uniform sampler2D mountainArctic;
uniform sampler2D mountainTundra;
uniform sampler2D mountainGrassland;
uniform sampler2D mountainPlain;
uniform sampler2D mountainDesert;



varying flat vec2 hexValue;
varying vec3 hexWorldPos;

void vertex() {
    hexValue = vec2((INSTANCE_CUSTOM.rg * 255.0) / vec2(gameBoardWidth, gameBoardHeight));
    hexWorldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    //VERTEX.y += (texture(terrainNoise, hexWorldPos.xz/1000.0).r * 100.0) * texture(terrainInfo, hexValue.xy).g;
}

void fragment() {
    vec2 visData = texture(visibilityGrid, hexValue.xy).rg;
    float terrainTemp = texture(terrainInfo, hexValue.xy).r;
    float vSlope = texture(terrainInfo, hexValue.xy).g;

    // Flat terrain blending
    vec4 flatTexture = texture(flatArctic, UV);
    vec4 roughTexture = texture(roughArctic, UV);
    vec4 mountainTexture = texture(mountainArctic, UV);

    if(terrainTemp > 0.0)
    {
        flatTexture = texture(flatTundra, UV);
        roughTexture = texture(roughTundra, UV);
        mountainTexture = texture(mountainTundra, UV);
    }
    if(terrainTemp > 0.26)
    {
        flatTexture = texture(flatGrassland, UV);
        roughTexture = texture(roughGrassland, UV);
        mountainTexture = texture(mountainGrassland, UV);
    }
    if(terrainTemp > 0.51)
    {
        flatTexture = texture(flatPlain, UV);
        roughTexture = texture(roughPlain, UV);
        mountainTexture = texture(mountainPlain, UV);
    }
    if(terrainTemp > 0.76)
    {
        flatTexture = texture(flatDesert, UV);
        roughTexture = texture(roughDesert, UV);
        mountainTexture = texture(mountainDesert, UV);
    }

    vec4 blendedTerrain = flatTexture;
    // Blend terrain type based on slope
    if(vSlope > 0.0 && vSlope < 0.9)
    {
        blendedTerrain = roughTexture;
    }
    if(vSlope > 0.9)
    {
        blendedTerrain = mountainTexture;
    }

    // Final terrain assignment
    //ALBEDO = vec3(texture(terrainInfo, hexValue.xy).r, texture(terrainInfo, hexValue.xy).g, 0.0);
    ALBEDO = blendedTerrain.rgb;

    // Calculate Visibility Changes
    float grayscaleFactor = dot(blendedTerrain.rgb, vec3(0.299, 0.587, 0.114)) * 0.5;
    
    vec3 grayscaleColor = vec3(grayscaleFactor);
    vec3 visibleColor = blendedTerrain.rgb;
    vec3 hiddenColor = vec3(0.0); // Fully hidden

    ALBEDO = hiddenColor * (1.0 - visData.r) * (1.0 - visData.g)  // Hidden
           + visibleColor * visData.r                             // Fully visible
           + grayscaleColor * visData.g;                          // Grayscale if seen
}
