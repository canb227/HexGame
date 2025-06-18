shader_type spatial;


uniform float gameBoardWidth;
uniform float gameBoardHeight;
uniform float widthDiv;
uniform float heightDiv;
uniform float chunkOffset;

uniform sampler2D visibilityGrid : filter_nearest;
uniform sampler2D terrainInfo : filter_nearest;
uniform sampler2D heightMap : filter_linear;

uniform sampler2D grassTex : filter_linear;
uniform sampler2D hillTex : filter_linear;
uniform sampler2D rockTex : filter_linear;
uniform sampler2D sandTex : filter_linear;
uniform sampler2D snowTex : filter_linear;
uniform sampler2D waterTex : filter_linear;

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



    //texture data
    terrainData = texture(terrainInfo, vec2((hexValue.x + floor(hexValue.y/2.0) + 0.5)/gameBoardWidth, (hexValue.y+ 0.5)/gameBoardHeight )).rg; //r is temp g is type
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

    vec4 roughTexture = texture(hillTex, UV);//vec4 roughTexture = vec4(0.74117, 0.71764, 0.419607, 1.0); // rgb(189,183,107)
    vec4 mountainTexture = texture(rockTex, UV);//vec4 mountainTexture = vec4(0.4, 0.4, 0.4, 1.0); //rgb(105,105,105)
    vec4 coastalTexture = texture(waterTex, UV);//vec4 coastalTexture = vec4(0.529411, 0.807843, 0.980392, 1.0); // rgb(135,206,250)
    vec4 oceanTexture = texture(waterTex, UV);//vec4 oceanTexture = vec4(0.0, 0.0, 0.54509, 1.0); // rgb(0,0,139)
    oceanTexture = vec4(oceanTexture.r*0.5, oceanTexture.g*0.5, oceanTexture.b+1.0, oceanTexture.a);

    if(terrainData.g > 0.0) //r is temp g is type  .1 desert .2 plains .3 grass .4 tundra .5 artic
    {
        if(terrainData.r > 0.0) //desert
        {
            terrainColor = texture(sandTex, UV).rgb;
        }
        if(terrainData.r > 0.15) //plains
        {
            float blendfactor = 0.5;
            terrainColor = mix(texture(sandTex, UV), texture(grassTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.25) //grasslands
        {
            terrainColor = texture(grassTex, UV).rgb;
        }
        if(terrainData.r > 0.35) //tundra
        {
            float blendfactor = 0.5;
            terrainColor = mix(texture(snowTex, UV), texture(grassTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.45) //artic
        {
            terrainColor = texture(snowTex, UV).rgb;
        }
    }
    if(terrainData.g > 0.19)
    {
        if(terrainData.r > 0.0) //desert
        {
            float blendfactor = 0.5;
            terrainColor = mix(texture(snowTex, UV), texture(hillTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.15) //plains
        {
            float blendfactor = 0.7;
            terrainColor = mix(texture(sandTex, UV), texture(hillTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.25) //grasslands
        {
            terrainColor = texture(hillTex, UV).rgb;
        }
        if(terrainData.r > 0.35) //tundra
        {
            float blendfactor = 0.7;
            terrainColor = mix(texture(snowTex, UV), texture(hillTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.45) //artic
        {
            float blendfactor = 0.5;
            terrainColor = mix(texture(snowTex, UV), texture(hillTex, UV), blendfactor).rgb;
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
