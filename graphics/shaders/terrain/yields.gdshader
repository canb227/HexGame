shader_type spatial;

uniform sampler2D yieldAtlas; // Texture atlas
uniform sampler2D digitAtlas;
const vec2 overlay_pos = vec2(0.55, 0.0);      // starting position (in [0,1])
const vec2 overlay_size = vec2(0.45, 0.45);      // size of the overlay region

uniform vec2 iconSize = vec2(0.25, 0.5); // UV step size for grid
uniform vec2 digit_grid = vec2(0.1, 1.0); // The digit atlas is assumed to have 10 cells horizontally (for digits 0-9).
uniform vec2 gridSize = vec2(0.33, 0.33); // Grid placement spacing on the mesh

uniform float gameBoardWidth;
uniform float gameBoardHeight;
uniform sampler2D visibilityGrid : filter_nearest;


varying flat vec2 visData;
varying vec2 instance_data;

void vertex() {
    instance_data = INSTANCE_CUSTOM.rg;
    vec2 hexValue = vec2((INSTANCE_CUSTOM.ba * 255.0));
    visData = texture(visibilityGrid, vec2((hexValue.x + floor(hexValue.y/2.0) + 0.5)/gameBoardWidth, (hexValue.y+ 0.5)/gameBoardHeight )).rg;
}


void fragment() {
    vec2 yieldData = instance_data.rg;

    // Extract yield type (mapped from 0 to 7)
    int yieldType = int(round(yieldData.r * 7.0));

    // Extract yield value (mapped from 0 to 100)
    int yieldValue = int(round(yieldData.g * 100.0));

    // Hide the yield icon if the value is 0
    if (yieldValue == 0 || (visData.r == 0.0 && visData.g == 0.0)) {
        discard;
    }

    // UV offsets in the atlas based on yield type
    vec2 uvOffsets[7] = vec2[7](
        vec2(0.0, 0.0), vec2(0.25, 0.0), vec2(0.50, 0.0),
        vec2(0.75, 0.0), vec2(0.0, 0.5), vec2(0.25, 0.5), vec2(0.50, 0.5)
    );

    // Select correct icon UV
    vec2 iconUV = uvOffsets[yieldType] + (UV * iconSize);

    // Sample texture
    vec4 yieldIcon = texture(yieldAtlas, iconUV);

    // Overlay digit logic remains unchanged
    vec2 local_uv = (UV - overlay_pos) / overlay_size;
    vec4 digit_tex = vec4(0.0);
    float amount = float(yieldValue);
    if (all(greaterThanEqual(local_uv, vec2(0.0))) && all(lessThanEqual(local_uv, vec2(1.0)))) {
        if (amount < 10.0) {
            float digit = amount;
            vec2 digit_uv = local_uv * digit_grid + vec2(digit * digit_grid.x, 0.0);
            digit_tex = texture(digitAtlas, digit_uv);
        } else {
            if (local_uv.x < 0.5) {
                vec2 new_uv = vec2(local_uv.x / 0.5, local_uv.y);
                float tens = floor(amount / 10.0);
                vec2 digit_uv = new_uv * digit_grid + vec2(tens * digit_grid.x, 0.0);
                digit_tex = texture(digitAtlas, digit_uv);
            } else {
                vec2 new_uv = vec2((local_uv.x - 0.5) / 0.5, local_uv.y);
                float ones = amount - floor(amount / 10.0) * 10.0;
                vec2 digit_uv = new_uv * digit_grid + vec2(ones * digit_grid.x, 0.0);
                digit_tex = texture(digitAtlas, digit_uv);
            }
        }
    }

    vec4 final_color = mix(yieldIcon, digit_tex, digit_tex.a);
    //ALBEDO = final_color.rgb;
    // Apply visibility effects
    vec3 grayscaleFactor = final_color.rgb * 0.5;
    vec3 grayscaleColor = vec3(grayscaleFactor);
    vec3 visibleColor = final_color.rgb;
    vec3 hiddenColor = vec3(0.0);

    ALBEDO = hiddenColor * (1.0 - visData.r) * (1.0 - visData.g)  // Hidden
           + visibleColor * visData.r                             // Fully visible
           + grayscaleColor * visData.g;                          // Grayscale if seen
}



