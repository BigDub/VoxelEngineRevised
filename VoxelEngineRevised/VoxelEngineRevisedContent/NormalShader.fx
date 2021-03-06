float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4 FogColor;
float2 FogDistance;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Normal : TEXCOORD0;
	float3 WorldPos : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = input.Normal;
	output.WorldPos = worldPosition.xyz;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 output = input.Normal / 2;
	output.x += .5;
	output.y += .5;
	output.z += .5;
	output.w = 1;
	float dis = distance(input.WorldPos, CameraPosition);
	if(dis <= FogDistance.x)
		return output;
	return lerp(output, FogColor, saturate((dis - FogDistance.x) / FogDistance.y));
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
