shader_type spatial;

uniform sampler2D yieldAtlas; // Texture atlas
uniform sampler2D digit_atlas;
const vec2 overlay_pos = vec2(0.55, 0.0);      // starting position (in [0,1])
const vec2 overlay_size = vec2(0.45, 0.45);      // size of the overlay region

uniform vec2 iconSize = vec2(0.25, 0.5); // UV step size for grid
uniform vec2 digit_grid = vec2(0.1, 1.0); // The digit atlas is assumed to have 10 cells horizontally (for digits 0-9).
uniform vec2 gridSize = vec2(0.33, 0.33); // Grid placement spacing on the mesh

varying vec4 instance_data;

void vertex() {
    instance_data = INSTANCE_CUSTOM;
}


void fragment() {
    vec3 yieldData = instance_data.rgb;

    // Extract yield type (mapped from 0 to 7)
    int yieldType = int(round(yieldData.r * 7.0));

    // Extract yield value (mapped from 0 to 100)
    int yieldValue = int(round(yieldData.g * 100.0));

    // UV offsets in the atlas based on yield type
    vec2 uvOffsets[7] = vec2[7](
        vec2(0.0, 0.0), vec2(0.25, 0.0), vec2(0.50, 0.0),
        vec2(0.75, 0.0), vec2(0.0, 0.5), vec2(0.25, 0.5), vec2(0.50, 0.5)
    );

    // Select correct icon UV
    vec2 iconUV = uvOffsets[yieldType] + (UV * iconSize);

    // Sample texture if yield value > 0.1
    vec4 yieldIcon = texture(yieldAtlas, iconUV);// * step(1.0, float(yieldValue));

    // Apply the icon to the mesh
    //ALBEDO = yieldIcon.rgb;

    // We want to overlay the exact count on top of the "5+" icon.
    // First, determine if the current fragment lies in the overlay region.
    vec2 local_uv = (UV - overlay_pos) / overlay_size;
    // local_uv now runs from 0 to 1 in the designated overlay area.
    vec4 digit_tex = vec4(0.0);
    // Only overlay if we're inside the region.
    float amount = float(yieldValue);
    if (all(greaterThanEqual(local_uv, vec2(0.0))) && all(lessThanEqual(local_uv, vec2(1.0)))) {
        if (amount < 10.0) {
            // For one-digit numbers (6-9), use the entire overlay region for one digit.
            float digit = amount; // The single digit to display.
            vec2 digit_uv = local_uv * digit_grid + vec2(digit * digit_grid.x, 0.0);
            digit_tex = texture(digit_atlas, digit_uv);
        } else {
            // For two-digit numbers (amount >= 10), split overlay into two halves.
            // Left half for the tens digit, right half for the ones digit.
            if (local_uv.x < 0.5) {
                // Left digit region.
                vec2 new_uv = vec2(local_uv.x / 0.5, local_uv.y);
                float tens = floor(amount / 10.0);
                vec2 digit_uv = new_uv * digit_grid + vec2(tens * digit_grid.x, 0.0);
                digit_tex = texture(digit_atlas, digit_uv);
            } else {
                // Right digit region.
                vec2 new_uv = vec2((local_uv.x - 0.5) / 0.5, local_uv.y);
                float ones = amount - floor(amount / 10.0) * 10.0;
                vec2 digit_uv = new_uv * digit_grid + vec2(ones * digit_grid.x, 0.0);
                digit_tex = texture(digit_atlas, digit_uv);
            }
        }
    }

    vec4 final_color = mix(yieldIcon, digit_tex, digit_tex.a);
    ALBEDO = final_color.rgb;
}


