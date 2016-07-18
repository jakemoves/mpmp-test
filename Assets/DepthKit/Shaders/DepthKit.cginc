
// James George 
// james@simile.systems
// (C) 2016 Simile
// 
// This script and demonstration data is provided for 
// prototyping and educational purposes only

// Unity Plugin Version 0.1.2

static const float PI = 3.14159265f;
static const float epsilon = .01;
static const float2 filters = float2(.3, .5);


fixed3 rgb2hsv(fixed3 c)
{
	fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
	fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));

	float d = q.x - min(q.w, q.y);
	return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + epsilon)), d / (q.x + epsilon), q.x);
}

float depthForPoint(float2 texturePoint){

	float4 textureSample = float4(texturePoint.x, texturePoint.y, 0.0, 0.0);
	fixed4 depthsample = tex2Dlod(_MainTex, textureSample);
	fixed3 depthsamplehsv = rgb2hsv(depthsample.rgb);

	return depthsamplehsv.g > filters.x && depthsamplehsv.b > filters.y ? depthsamplehsv.r : 0.0;
	//return depthsamplehsv.r;
}
