Shader "Hidden/Custom/GlitchShader"
{
    HLSLINCLUDE
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

#pragma exclude_renderers d3d11_9x
        #pragma target 3.0

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_DispTex, sampler_DispTex);
        float _Intensity;
		float _ColorIntensity;

        int4 direction;

        float filterRadius;
        float flip_up, flip_down;
        float displace;
        float scale;

		struct v2f { //shim structure
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

        half4 frag (v2f i) : COLOR
		{
			half4 normal = tex2D (_DispTex, i.uv.xy * scale);

			i.uv.y -= (1 - (i.uv.y + flip_up)) * step(i.uv.y, flip_up) + (1 - (i.uv.y - flip_down)) * step(flip_down, i.uv.y);

			i.uv.xy += (normal.xy - 0.5) * displace * _Intensity;

			half4 color = tex2D(_MainTex,  i.uv.xy);
			half4 redcolor = tex2D(_MainTex, i.uv.xy + direction.xy * 0.01 * filterRadius * _ColorIntensity);
			half4 greencolor = tex2D(_MainTex,  i.uv.xy - direction.xy * 0.01 * filterRadius * _ColorIntensity);

			color += int4(redcolor.r, redcolor.b, redcolor.g, 1) *  step(filterRadius, -0.001);
			color *= 1 - 0.5 * step(filterRadius, -0.001);

			color += int4(greencolor.g, greencolor.b, greencolor.r, 1) *  step(0.001, filterRadius);
			color *= 1 - 0.5 * step(0.001, filterRadius);

			return color;
		}


    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment frag

            ENDHLSL
        }
    }
}
