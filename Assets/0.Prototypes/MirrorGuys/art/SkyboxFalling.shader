// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skybox/Skybox Falling" {
	Properties{
		_Cube("Environment Map", Cube) = "white" {}
		_OffsetY("Y Offset", Float) = 0
		_Rotation("Rotation", Range(0,360)) = 0
	}

		SubShader{
		Tags{ "Queue" = "Background" }

		Pass{
		ZWrite Off
		Cull Off

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

		// User-specified uniforms
		samplerCUBE _Cube;
		float _OffsetY;
		float _Rotation;

	struct vertexInput {
		float4 vertex : POSITION;
		float3 texcoord : TEXCOORD0;
	};

	struct vertexOutput {
		float4 vertex : SV_POSITION;
		float3 texcoord : TEXCOORD0;
	};

	vertexOutput vert(vertexInput input)
	{
		vertexOutput output;
		output.vertex = UnityObjectToClipPos(input.vertex);
//		Y Rotation matrix
		float deg2rad = 3.14159265359 / 180;
		float4x4 yRotMatrix = {
		{cos(-_Rotation * deg2rad), 0, sin(-_Rotation * deg2rad), 0},
		{0, 1, 0, 0},
		{-sin(-_Rotation * deg2rad), 0, cos(-_Rotation * deg2rad), 0},
		{0, 0, 0, 1}
		};


		output.texcoord = mul(yRotMatrix,  input.texcoord + float4(0, _OffsetY, 0, 1));
		return output;
	}

	fixed4 frag(vertexOutput input) : COLOR
	{
		return texCUBE(_Cube, input.texcoord);
	}
		ENDCG
	}
	}
}