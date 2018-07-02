Shader "Unlit/OpticalFlow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FlowTex( "FlowTexture", 2D) = "white" {}
		_Index("Index", int) = 0
		_Divide("Divide", vector) = ( 1, 1, 0, 0 )
		_BlendWeight("BlendWeight", Range(0.0, 1.0) ) = 0.0
		_FlowStrength("FlowStrength", float) = 1

		//これがあるとスクリプトからいじれない？
		//[KeywordEnum(FLOW,LINEAR)]
		//_FRAGMENT("FRAGMENT KEYWORD", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _FRAGMENT_FLOW _FRAGMENT_LINEAR

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _FlowTex;
			uint _Index;
			float4 _Divide;
			float _BlendWeight;
			float _FlowStrength;

			float2 GetUv(float2 uv, float2 size, uint index)
			{
				uint x = index % (uint)_Divide.x;
				uint y = ((uint)_Divide.y - 1) - (index / (uint)_Divide.y);

				float2 outUv = uv * size.xy;
				outUv.x += x * size.x;
				outUv.y += y * size.y;
				return outUv;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos( v.vertex );

				float2 size = _Divide.zw;
				o.uv0 = GetUv( v.uv, size, _Index );
				o.uv1 = GetUv( v.uv, size, _Index + 1 );
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
#ifdef _FRAGMENT_FLOW
				float2 flow0 = tex2D( _FlowTex, i.uv0 ).xy;
				flow0 = flow0 * 2.0 - 1.0;
				float2 flow1 = tex2D(_FlowTex, i.uv1).xy;
				flow1 = flow1 * 2.0 - 1.0;
				float2 size = _Divide.zw;
				fixed4 col0 = tex2D(_MainTex, i.uv0 + -flow0 * _FlowStrength * _BlendWeight );
				fixed4 col1 = tex2D(_MainTex, i.uv1 + flow1 * _FlowStrength * (1.0 - _BlendWeight) );
				fixed4 col = lerp(col0, col1, _BlendWeight);
#elif _FRAGMENT_LINEAR
				fixed4 col0 = tex2D(_MainTex, i.uv0);
				fixed4 col1 = tex2D(_MainTex, i.uv1);
				fixed4 col = lerp(col0, col1, _BlendWeight);
#endif
				return col;
			}
			ENDCG
		}
	}
}
