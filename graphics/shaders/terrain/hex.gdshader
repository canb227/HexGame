shader_type spatial;

global uniform float time;

uniform float gameBoardWidth;
uniform float gameBoardHeight;
uniform float widthDiv;
uniform float heightDiv;
//uniform float chunkOffset;
uniform float chunkCount;

uniform sampler2D visibilityGrid : filter_nearest;
uniform sampler2D terrainInfo : filter_nearest;
uniform sampler2D territoryMap : filter_nearest;
uniform sampler2D heightMap;

uniform sampler2D grassTex : source_color, filter_linear_mipmap;
uniform sampler2D grassNorm : hint_normal, filter_linear_mipmap;

uniform sampler2D hillTex : source_color, filter_linear_mipmap;
uniform sampler2D hillNorm : hint_normal, filter_linear_mipmap;

uniform sampler2D rockTex : source_color, filter_linear_mipmap;
uniform sampler2D rockNorm : hint_normal, filter_linear_mipmap;

uniform sampler2D sandTex : source_color, filter_linear_mipmap;
uniform sampler2D sandNorm : hint_normal, filter_linear_mipmap;

uniform sampler2D snowTex : source_color, filter_linear_mipmap;
uniform sampler2D snowNorm : hint_normal, filter_linear_mipmap;

uniform sampler2D waterTex : source_color, filter_linear_mipmap;
uniform sampler2D waterNorm : hint_normal, filter_linear_mipmap;

varying flat vec2 visData;
varying flat vec3 territoryData;
varying flat vec2 terrainData;
varying flat vec2 hexValue;
varying vec3 hexWorldPos;

void vertex() {
    hexValue = vec2((INSTANCE_CUSTOM.rg * 255.0));
    hexWorldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    //UV = vec2(VERTEX.x/4.0 + 16.0, VERTEX.z/4.0 + 16.0) / vec2(widthDiv,heightDiv);
    
    //visibility data
    visData = texture(visibilityGrid, vec2((hexValue.x + floor(hexValue.y/2.0) + 0.5)/gameBoardWidth, (hexValue.y+ 0.5)/gameBoardHeight )).rg;
    //territory data
    territoryData = texture(territoryMap, vec2((hexValue.x + floor(hexValue.y/2.0) + 0.5)/gameBoardWidth, (hexValue.y+ 0.5)/gameBoardHeight )).rgb;
    //texture data
    terrainData = texture(terrainInfo, vec2((hexValue.x + floor(hexValue.y/2.0) + 0.5)/gameBoardWidth, (hexValue.y+ 0.5)/gameBoardHeight )).rg; //r is temp g is type

    //heightmap data
    VERTEX.y += max(visData.r, visData.g) * (texture(heightMap, vec2( ((-hexWorldPos.z)/widthDiv/chunkCount), (hexWorldPos.x)/heightDiv)).r * 20.0);// * texture(terrainInfo, hexValue.xy).r;

    //normal calculation

    
}

vec3 get_territory_at(vec2 coord) {
	vec2 uv = vec2((coord.x + floor(coord.y/2.0) + 0.5)/gameBoardWidth, (coord.y + 0.5)/gameBoardHeight);
	return texture(territoryMap, uv).rgb;
}

float calcHexDistance(vec2 p)
{
     const float hexHalfWidth = .5;
     // 1.7320508 is sqrt(3)
     const vec2 s = vec2(1, 1.7320508);
     p = abs(p);
     return max(dot(p, s * .5), p.x) - hexHalfWidth;
}

void fragment() {
    //if ((visData.r == 0.0 && visData.g == 0.0)) {
    //    discard;
    //}
    float hoffset = sqrt(3) * 10.0 / 2.0;
    float voffset = 1.5 * 10.0 / 2.0;
    float terrainHeight = texture(heightMap, vec2((-hexWorldPos.z)/widthDiv/chunkCount, (hexWorldPos.x)/heightDiv)).r;

    //terrainColor setting
    vec3 terrainColor = vec3(0.0, 0.0, 0.0);


    vec4 roughTexture = texture(hillTex, UV);//vec4 roughTexture = vec4(0.74117, 0.71764, 0.419607, 1.0); // rgb(189,183,107)
    vec4 mountainTexture = texture(rockTex, UV);//vec4 mountainTexture = vec4(0.4, 0.4, 0.4, 1.0); //rgb(105,105,105)
    vec4 coastalTexture = texture(waterTex, UV+time*0.01);//vec4 coastalTexture = vec4(0.529411, 0.807843, 0.980392, 1.0); // rgb(135,206,250)
    vec4 oceanTexture = texture(waterTex, UV+time*0.01);//vec4 oceanTexture = vec4(0.0, 0.0, 0.54509, 1.0); // rgb(0,0,139)
    oceanTexture = vec4(oceanTexture.r*0.5, oceanTexture.g*0.5, oceanTexture.b+1.0, oceanTexture.a);

    if(terrainData.g > 0.0) //r is temp g is type  .1 desert .2 plains .3 grass .4 tundra .5 artic
    {
        if(terrainData.r > 0.0) //desert
        {
            NORMAL = texture(sandNorm, UV).rgb;
            terrainColor = texture(sandTex, UV).rgb;
        }
        if(terrainData.r > 0.15) //plains
        {
            float blendfactor = 0.5;
            NORMAL = mix(texture(sandNorm, UV), texture(grassNorm, UV), blendfactor).rgb;
            terrainColor = mix(texture(sandTex, UV), texture(grassTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.25) //grasslands
        {
            NORMAL = texture(grassNorm, UV).rgb;
            terrainColor = texture(grassTex, UV).rgb;
        }
        if(terrainData.r > 0.35) //tundra
        {
            float blendfactor = 0.5;
            NORMAL = mix(texture(snowNorm, UV), texture(grassNorm, UV), blendfactor).rgb;
            terrainColor = mix(texture(snowTex, UV), texture(grassTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.45) //artic
        {
            NORMAL = texture(snowNorm, UV).rgb;
            terrainColor = texture(snowTex, UV).rgb;
        }
    }
    if(terrainData.g > 0.19)
    {
        if(terrainData.r > 0.0) //desert
        {
            float blendfactor = 0.5;
            NORMAL = mix(texture(sandNorm, UV), texture(hillNorm, UV), blendfactor).rgb;
            terrainColor = mix(texture(sandTex, UV), texture(hillTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.15) //plains
        {
            float blendfactor = 0.7;
            NORMAL = mix(texture(sandNorm, UV), texture(hillNorm, UV), blendfactor).rgb;
            terrainColor = mix(texture(sandTex, UV), texture(hillTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.25) //grasslands
        {
            NORMAL = texture(hillNorm, UV).rgb;
            terrainColor = texture(hillTex, UV).rgb;
        }
        if(terrainData.r > 0.35) //tundra
        {
            float blendfactor = 0.7;
            NORMAL = mix(texture(snowNorm, UV), texture(hillNorm, UV), blendfactor).rgb;
            terrainColor = mix(texture(snowTex, UV), texture(hillTex, UV), blendfactor).rgb;
        }
        if(terrainData.r > 0.45) //artic
        {
            float blendfactor = 0.5;
            NORMAL = mix(texture(snowNorm, UV), texture(hillNorm, UV), blendfactor).rgb;
            terrainColor = mix(texture(snowTex, UV), texture(hillTex, UV), blendfactor).rgb;
        }
    }
    if(terrainData.g > 0.29)
    {
        if(terrainData.r > 0.0)
        {
            NORMAL = texture(rockNorm, UV).rgb;
            terrainColor = mountainTexture.rgb;
        }
    }
    if(terrainData.g > 0.39)
    {
        NORMAL = texture(waterNorm, UV+time*0.01).rgb;
        terrainColor = coastalTexture.rgb;
    }
    if(terrainData.g > 0.49)
    {
        NORMAL = texture(waterNorm, UV+time*0.01).rgb;
        terrainColor = oceanTexture.rgb;
    }

	vec2 p = UV - vec2(0.5);
	p.y *= 1.1547;
	float dist = calcHexDistance(p);
	float outline_thickness = 0.05; // Adjust for thicker/thinner lines
	float sdf_border = 1.0 - smoothstep(0.0, outline_thickness, abs(dist));

    // D. Determine which edge we are on by checking the angle.
    float angle = atan(p.y, p.x); // 'p' is the centered UV coordinate
    float pi = 3.14159265;

    vec2 neighbor_offset;
    float border = 0.0;
    // Select the correct neighbor offset based on the angle
    if (angle > -pi/6.0 && angle <= pi/6.0) {
        neighbor_offset = vec2(1.0, 0.0); // Right
    } else if (angle > pi/6.0 && angle <= pi/2.0) {
        neighbor_offset = vec2(1.0, -1.0); // Top-Right
    } else if (angle > pi/2.0 && angle <= 5.0*pi/6.0) {
        neighbor_offset = vec2(0.0, -1.0); // Top-Left
    } else if (angle > 5.0*pi/6.0 || angle <= -5.0*pi/6.0) {
        neighbor_offset = vec2(-1.0, 0.0); // Left
    } else if (angle > -5.0*pi/6.0 && angle <= -pi/2.0) {
        neighbor_offset = vec2(-1.0, 1.0); // Bottom-Left
    } else { // -pi/2.0 to -pi/6.0
        neighbor_offset = vec2(0.0, 1.0); // Bottom-Right
    }

    // E. Get the territory color of the correct neighbor
    vec3 neighbor_territory = get_territory_at(hexValue + neighbor_offset);

    // F. If the neighbor's color is different, activate the border
    if (distance(territoryData, neighbor_territory) > 0.1) {
        border = sdf_border; // Use the smooth value from the SDF
    }

    terrainColor = terrainColor.rgb * (1.0-terrainHeight);

    terrainColor += territoryData * border;
    // Calculate Visibility Changes
    vec3 grayscaleFactor = terrainColor.rgb * 0.5;//= dot(terrainColor.rgb, vec3(0.299, 0.587, 0.114)) * 0.5;
    
    vec3 grayscaleColor = vec3(grayscaleFactor);
    vec3 visibleColor = terrainColor.rgb;
    vec3 hiddenColor = vec3(0.0); // Fully hidden
    
    ALBEDO = hiddenColor * (1.0 - visData.r) * (1.0 - visData.g)  // Hidden
           + visibleColor * visData.r                             // Fully visible
           + grayscaleColor * visData.g;                          // Grayscale if seen

}
