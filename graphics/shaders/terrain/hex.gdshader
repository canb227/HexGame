shader_type spatial;

uniform float gameBoardRight;
uniform float gameBoardLeft;
uniform float widthDiv;
uniform float heightDiv;
uniform float chunkOffset;

uniform sampler2D visibilityGrid : filter_nearest;
uniform sampler2D heightMap : filter_linear;

varying vec2 hexValue;
varying vec3 hexWorldPos;

float safeMod(float x, float y) {
    return x - y * floor(x / y);
}

vec2 WrapHex(vec2 hex, float left, float right) {
    left -= floor(hex.y / 2.0);
    right -= floor(hex.y / 2.0);

    float range = right - left;
    float newQ = mod((hex.x - left), range) + left;

    if (newQ < left) {
        newQ = right + mod((newQ - left), range);
    }

    return vec2(newQ, hex.y);
}

vec2 PixelToHex(vec2 p, vec2 origin, vec2 size, mat3 orientation) {
    vec2 pt = vec2((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
    float q = orientation[0][0] * pt.x + orientation[0][1] * pt.y;
    float r = orientation[1][0] * pt.x + orientation[1][1] * pt.y;
    return vec2(round(q), round(r));
}

void vertex() {
    hexWorldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;

    float hoffset = sqrt(3) * 10.0 / 2.0;
    float voffset = 1.5 * 10.0 / 2.0;

    vec2 origin = vec2(0.0, 0.0);
    vec2 size = vec2(10.0, 10.0);
    mat3 orientation;
    orientation[0] = vec3(sqrt(3.0), sqrt(3.0) / 2.0, 0.0);
    orientation[1] = vec3(0.0, 3.0 / 2.0, 0.0);
    orientation[2] = vec3(sqrt(3.0) / 3.0, -1.0 / 3.0, 2.0 / 3.0);

    hexValue = vec2(-hexWorldPos.z/(sqrt(3)*10.0), hexWorldPos.x/(15.0));
    hexValue = WrapHex(hexValue, gameBoardLeft, gameBoardRight);

    VERTEX.y += texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.0)/widthDiv, (hexWorldPos.x+voffset-1.0)/heightDiv)).r * 10.0;// * texture(terrainInfo, hexValue.xy).r;
}

void fragment() {
    vec2 visData = texture(visibilityGrid, hexValue.xy).rg;
    float hoffset = sqrt(3) * 10.0 / 2.0;
    float voffset = 1.5 * 10.0 / 2.0;

    ALBEDO = vec3(hexValue.x/gameBoardRight, hexValue.y/20.0, 0.0);//texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.0)/widthDiv, (hexWorldPos.x+voffset-1.0)/heightDiv)).rgb;
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

