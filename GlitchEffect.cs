using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Random=UnityEngine.Random;

[Serializable]
[PostProcess(typeof(GlitchRenderer), PostProcessEvent.AfterStack, "Custom/GlitchEffect")]
public class GlitchEffect : PostProcessEffectSettings
{
	public TextureParameter displacementMap = new TextureParameter {value = null};

	[Header("Glitch Effect")]

	[Range(0f, 1f)]
	public FloatParameter intensity = new FloatParameter {value = 0.5f };

	[Range(0f, 1f)]
	public FloatParameter flipIntensity = new FloatParameter {value = 0};

	[Range(0f, 1f)]
	public FloatParameter colorIntensity = new FloatParameter {value = 0};

}

public class GlitchRenderer : PostProcessEffectRenderer<GlitchEffect>
{
    private float _glitchup;
    private float _glitchdown;
    private float flicker;
    private float _glitchupTime = 0.05f;
    private float _glitchdownTime = 0.05f;
    private float _flickerTime = 0.5f;

    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/GlitchShader"));

        sheet.properties.SetFloat("_Intensity", settings.intensity);
        sheet.properties.SetFloat("_ColorIntensity", settings.colorIntensity);
        sheet.properties.SetTexture("_DispTex", settings.displacementMap);

        flicker += Time.deltaTime * settings.colorIntensity;
		if (flicker > _flickerTime)
		{
			sheet.properties.SetFloat("filterRadius", Random.Range(-3f, 3f) * settings.colorIntensity);
			sheet.properties.SetVector("direction", Quaternion.AngleAxis(Random.Range(0, 360) * settings.colorIntensity, Vector3.forward) * Vector4.one);
			flicker = 0;
			_flickerTime = Random.value;
		}

		if (settings.colorIntensity == 0)
			sheet.properties.SetFloat("filterRadius", 0);

		_glitchup += Time.deltaTime * settings.flipIntensity;
		if (_glitchup > _glitchupTime)
		{
			if (Random.value < 0.1f * settings.flipIntensity)
				sheet.properties.SetFloat("flip_up", Random.Range(0, 1f) * settings.flipIntensity);
			else
				sheet.properties.SetFloat("flip_up", 0);

			_glitchup = 0;
			_glitchupTime = Random.value / 10f;
		}

		if (settings.flipIntensity == 0)
			sheet.properties.SetFloat("flip_up", 0);

		_glitchdown += Time.deltaTime * settings.flipIntensity;
		if (_glitchdown > _glitchdownTime)
		{
			if (Random.value < 0.1f * settings.flipIntensity)
				sheet.properties.SetFloat("flip_down", 1 - Random.Range(0, 1f) * settings.flipIntensity);
			else
				sheet.properties.SetFloat("flip_down", 1);

			_glitchdown = 0;
			_glitchdownTime = Random.value / 10f;
		}

		if (settings.flipIntensity == 0)
			sheet.properties.SetFloat("flip_down", 1);

		if (Random.value < 0.05 * settings.intensity)
		{
			sheet.properties.SetFloat("displace", Random.value * settings.intensity);
			sheet.properties.SetFloat("scale", 1 - Random.value * settings.intensity);
		}
		else
			sheet.properties.SetFloat("displace", 0);

        // Blit !
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
