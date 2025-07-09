/*
 * CameraMirrorShader v1.0
 *
 * Copyright (c) 2019 ほびわん
 *
 * Released under the MIT license.
 * see https://opensource.org/licenses/MIT
*/

Shader "HobionePlanet/Unlit/CameraMirror"
{
    Properties
    {
		[Header(CameraTexture)]
		_MainTex("Texture", 2D) = "white" {}

		[Header(Effect)]
		_Saturation("Color Saturation", Range(0,1)) = 1
		_MixColor("Mix Color", Color) = (1, 1, 1, 1)
		_Brightness("Brightness", Float) = 1


		[Header(Mosaic Effect)]
		[Toggle(ENABLE_MOSAIC)]_EnableMosaic("Enable Mosaic", Float) = 0
		_Mosaic("Mosaic", Range(1, 200)) = 100
		_Aspect("Aspect X/Y", Range(0.01, 20)) = 1.77
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
            #pragma multi_compile_fog

			#pragma shader_feature ENABLE_MOSAIC

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _MixColor;
			float _Saturation, _Brightness, _Mosaic, _Aspect;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			fixed4 luminous(fixed4 c) {
				float3 l = (c.r + c.g + c.b) * 0.33333333;
				return float4(l, c.a);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = float2(1-i.uv.x, i.uv.y);

			#ifdef ENABLE_MOSAIC
				float2 mosaic = _Mosaic * float2(_Aspect, 1);
				uv = (floor(uv * mosaic) + 0.5) / mosaic;	// モザイク化
			#endif

                fixed4 col = tex2D(_MainTex, uv);
				col = lerp(luminous(col) * _MixColor, col, _Saturation);
				col.rgb *= _Brightness;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
