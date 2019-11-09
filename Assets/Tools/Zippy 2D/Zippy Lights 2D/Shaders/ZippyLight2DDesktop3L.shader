Shader "Unluck Software/ZippyLight2D Desktop (3xLight)" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}
			
		_LightColor("Light Color", Color) = (1, 1, 1, 1)
		_Contrast("Light Contrast", Float) = 1.0
	}
	
	SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		UsePass "Unluck Software/ZippyLight2D Desktop (1xLight)/LIGHT"
		UsePass "Unluck Software/ZippyLight2D Desktop (1xLight)/LIGHT"
		UsePass "Unluck Software/ZippyLight2D Desktop (1xLight)/LIGHT"
	}
}