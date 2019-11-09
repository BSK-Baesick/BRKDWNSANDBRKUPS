Shader "Unluck Software/ZippyLight2D Desktop (1xLight + Color)" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}

		_TintColor("Tint Color", Color) = (1, 1, 1, 1)
		_ColorMultiply("Multiply Color", Float) = 0.5
			
		_LightColor("Light Color", Color) = (1, 1, 1, 1)
		_Contrast("Light Contrast", Float) = 1.0
	}
	
	SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		UsePass "Unluck Software/ZippyLight2D Desktop (1xLight)/LIGHT"
		UsePass "Unluck Software/ZippyLight2D Desktop (Color)/COLOR"
	}
}