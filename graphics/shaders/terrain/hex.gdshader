shader_type spatial;

uniform sampler2D visibilityGrid : filter_nearest;
uniform sampler2D grassTexture;
uniform float gameBoardWidth;
uniform float gameBoardHeight;

varying flat vec2 hexValue;

float rand(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void vertex() {
    hexValue = vec2((INSTANCE_CUSTOM.rg * 255.0) / vec2(gameBoardWidth, gameBoardHeight));
}

void fragment() {
    vec2 visData = texture(visibilityGrid, hexValue.xy).rg;
    vec4 grassVal = texture(grassTexture, UV);

    // Compute grayscale conversion
    float grayscaleFactor = dot(grassVal.rgb, vec3(0.299, 0.587, 0.114)) * 0.5;
    
    vec3 grayscaleColor = vec3(grayscaleFactor);
    vec3 visibleColor = grassVal.rgb;
    vec3 hiddenColor = vec3(0.0); // Fully hidden

    ALBEDO = hiddenColor * (1.0 - visData.r) * (1.0 - visData.g)  // Hidden
           + visibleColor * visData.r                             // Fully visible
           + grayscaleColor * visData.g;                          // Grayscale if seen
}
