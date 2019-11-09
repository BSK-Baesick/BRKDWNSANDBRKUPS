// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unluck Software/ZippyLight2D Desktop (3xLight) Stenciled" {
	Properties {
		_LightColor("Light Color", Color) = (1, 1, 1, 1)
	}
	
	SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Pass {
			Stencil {
				Ref 1
				Comp NotEqual
				Pass Replace
		    }
			Blend DstColor One	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			uniform float4 _LightColor;

			struct VertexInput {
				float4 vertex: POSITION;
				float4 color: COLOR;
			};

			struct VertexOutput {
				float4 pos: SV_POSITION;
				float4 color: COLOR;
			};

			VertexOutput vert(VertexInput input) {
				VertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
				output.color = input.color;
				return output;
			}

			float3 frag(VertexOutput input): COLOR {
				float3 c;
				c = input.color.rgb * _LightColor.rgb;				
				return float3(c);
			}
			ENDCG
		}
		Pass {
			Stencil {
				Ref 2
				Comp NotEqual
				Pass Replace
		    }
			Blend DstColor One	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			uniform float4 _LightColor;

			struct VertexInput {
				float4 vertex: POSITION;
				float4 color: COLOR;
			};

			struct VertexOutput {
				float4 pos: SV_POSITION;
				float4 color: COLOR;
			};

			VertexOutput vert(VertexInput input) {
				VertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
				output.color = input.color;
				return output;
			}

			float3 frag(VertexOutput input): COLOR {
				float3 c;
				c = input.color.rgb * _LightColor.rgb;				
				return float3(c);
			}
			ENDCG
		}
		Pass {
			Stencil {
				Ref 3
				Comp NotEqual
				Pass Replace
		    }
			Blend DstColor One	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			uniform float4 _LightColor;

			struct VertexInput {
				float4 vertex: POSITION;
				float4 color: COLOR;
			};

			struct VertexOutput {
				float4 pos: SV_POSITION;
				float4 color: COLOR;
			};

			VertexOutput vert(VertexInput input) {
				VertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
				output.color = input.color;
				return output;
			}

			float3 frag(VertexOutput input): COLOR {
				float3 c;
				c = input.color.rgb * _LightColor.rgb;				
				return float3(c);
			}
			ENDCG
		}
	}
}