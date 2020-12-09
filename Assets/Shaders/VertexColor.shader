Shader "Custom/VertexColor" {

	Properties{
		_Plane ("Plane", Vector) = (0, 0, 0, 0)
		_Reverse("isReverse", Int) = 1
	}

	SubShader{
		Tags { "RenderType" = "Opaque" "QUEUE" = "Transparent"}
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert alpha
		#pragma target 3.0

		fixed4 _Plane;
		int _Reverse;

		struct appdata {
			float4 vertex : POSITION;
			float4 color : COLOR;
		};

		struct Input {
			float4 vertColor;
		};

		void vert(inout appdata v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			if (_Reverse * (v.vertex.x * _Plane.x + v.vertex.y * _Plane.y + v.vertex.z * _Plane.z + _Plane.w) >= 0)
				o.vertColor = v.color;
			else
				o.vertColor = fixed4(0, 0, 0, 0);
		}

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.vertColor.rgb;
			o.Alpha = IN.vertColor.a;
		}
		ENDCG
	}
		FallBack "Diffuse"
}