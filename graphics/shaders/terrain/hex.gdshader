shader_type spatial;


uniform float gameBoardWidth;
uniform float gameBoardHeight;
uniform float widthDiv;
uniform float heightDiv;
uniform float chunkOffset;

uniform sampler2D visibilityGrid : filter_nearest;
uniform sampler2D terrainInfo : filter_nearest;
uniform sampler2D heightMap : filter_linear;

varying flat vec2 visData;
varying flat vec2 terrainData;
varying vec3 hexWorldPos;

void vertex() {
    vec2 hexValue = vec2((INSTANCE_CUSTOM.rg * 255.0));
    hexWorldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    //UV = vec2(VERTEX.x/4.0 + 16.0, VERTEX.z/4.0 + 16.0) / vec2(widthDiv,heightDiv);
    float hoffset = sqrt(3) * 10.0 / 2.0;
    float voffset = 1.5 * 10.0 / 2.0;
    
    //visibility data
    visData = texture(visibilityGrid, vec2((hexValue.x + floor(hexValue.y/2.0) + 0.5)/gameBoardWidth, (hexValue.y+ 0.5)/gameBoardHeight )).rg;
    VERTEX.y += max(visData.r, visData.g) * (texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.0)/widthDiv, (hexWorldPos.x+voffset-1.0)/heightDiv)).r * 10.0);// * texture(terrainInfo, hexValue.xy).r;

    //normal calculation
    float heightCenter = texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.0)/widthDiv, (hexWorldPos.x+voffset-1.0)/heightDiv)).r * 10.0;
    float heightRight  = texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.01)/widthDiv, (hexWorldPos.x+voffset-1.0)/heightDiv)).r * 10.0;
    float heightUp     = texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.0)/widthDiv, (hexWorldPos.x+voffset-0.99)/heightDiv)).r * 10.0;
    vec3 tangentX = vec3(1.0, heightRight - heightCenter, 0.0);
    vec3 tangentZ = vec3(0.0, heightUp - heightCenter, 1.0);
    NORMAL = normalize(cross(tangentZ, tangentX));


    //texture data
    terrainData = texture(terrainInfo, vec2((hexValue.x + floor(hexValue.y/2.0) + 0.5)/gameBoardWidth, (hexValue.y+ 0.5)/gameBoardHeight )).rg;
}

void fragment() {
    if ((visData.r == 0.0 && visData.g == 0.0)) {
        discard;
    }
    float hoffset = sqrt(3) * 10.0 / 2.0;
    float voffset = 1.5 * 10.0 / 2.0;
    float terrainHeight = texture(heightMap, vec2((-hexWorldPos.z+hoffset-chunkOffset+1.0)/widthDiv, (hexWorldPos.x+voffset-1.0)/heightDiv)).r;

    //terrainColor setting
    vec3 terrainColor = vec3(0.0, 0.0, 0.0);

    vec4 flatTexture = vec4(0.60392, 0.803921, 0.196078, 1.0); //rgb(154,205,50)
    vec4 roughTexture = vec4(0.74117, 0.71764, 0.419607, 1.0); // rgb(189,183,107)
    vec4 mountainTexture = vec4(0.4, 0.4, 0.4, 1.0); //rgb(105,105,105)
    vec4 coastalTexture = vec4(0.529411, 0.807843, 0.980392, 1.0); // rgb(135,206,250)
    vec4 oceanTexture = vec4(0.0, 0.0, 0.54509, 1.0); // rgb(0,0,139)

    if(terrainData.g > 0.0)
    {
        if(terrainData.r > 0.0)
        {
            terrainColor = flatTexture.rgb;
        }
    }
    if(terrainData.g > 0.19)
    {
        if(terrainData.r > 0.0)
        {
            terrainColor = roughTexture.rgb;
        }
    }
    if(terrainData.g > 0.29)
    {
        if(terrainData.r > 0.0)
        {
            terrainColor = mountainTexture.rgb;
        }
    }
    if(terrainData.g > 0.39)
    {
        if(terrainData.r > 0.0)
        {
            terrainColor = coastalTexture.rgb;
        }
    }
    if(terrainData.g > 0.49)
    {
        if(terrainData.r > 0.0)
        {
            terrainColor = oceanTexture.rgb;
        }
        if(terrainData.r > 0.19)
        {
            terrainColor = oceanTexture.rgb;
        }
    }


    terrainColor = terrainColor.rgb * (1.0-terrainHeight);

    // Calculate Visibility Changes
    vec3 grayscaleFactor = terrainColor.rgb * 0.5;//= dot(terrainColor.rgb, vec3(0.299, 0.587, 0.114)) * 0.5;
    
    vec3 grayscaleColor = vec3(grayscaleFactor);
    vec3 visibleColor = terrainColor.rgb;
    vec3 hiddenColor = vec3(0.0); // Fully hidden
    
    ALBEDO = hiddenColor * (1.0 - visData.r) * (1.0 - visData.g)  // Hidden
           + visibleColor * visData.r                             // Fully visible
           + grayscaleColor * visData.g;                          // Grayscale if seen

    //ALBEDO = vec3(visData, 0.0);
    //ALBEDO = vec3((hexValue.x+(floor(hexValue.y/2.0)))/gameBoardWidth, hexValue.y/gameBoardHeight, 0.0);
}
