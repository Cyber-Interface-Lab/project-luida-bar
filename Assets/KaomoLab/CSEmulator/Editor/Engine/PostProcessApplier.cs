using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class PostProcessApplier
        : IPostProcessApplier
    {
        readonly PostProcessVolume postProcessVolume;
        readonly FogSettingsBridge fogSettings;

        public PostProcessApplier(
            GameObject root,
            FogSettingsBridge fogSettings
        )
        {
            postProcessVolume = root.GetComponentInChildren<PostProcessVolume>();
            this.fogSettings = fogSettings;
        }

        public void Apply(BloomSettings settings)
        {
            postProcessVolume.profile.TryGetSettings<Bloom>(out var bloom);
            bloom.active = settings.active;
            settings.anamorphicRatio.Apply(bloom.anamorphicRatio);
            settings.clamp.Apply(bloom.clamp);
            settings.color.Apply(bloom.color);
            settings.enabled.Apply(bloom.enabled);
            settings.intensity.Apply(bloom.intensity);
            settings.softKnee.Apply(bloom.softKnee);
            settings.threshold.Apply(bloom.threshold);
        }
        public void Apply(ChromaticAberrationSettings settings)
        {
            postProcessVolume.profile.TryGetSettings<ChromaticAberration>(out var chromaticAberration);
            chromaticAberration.active = settings.active;
            settings.enabled.Apply(chromaticAberration.enabled);
            settings.intensity.Apply(chromaticAberration.intensity);
        }
        public void Apply(ColorGradingSettings settings)
        {
            postProcessVolume.profile.TryGetSettings<ColorGrading>(out var colorGrading);
            colorGrading.gradingMode.Override(GradingMode.LowDefinitionRange);
            colorGrading.active = settings.active;
            settings.enabled.Apply(colorGrading.enabled);
            settings.brightness.Apply(colorGrading.brightness);
            settings.channelMixerBlue.blue.Apply(colorGrading.mixerBlueOutBlueIn);
            settings.channelMixerBlue.green.Apply(colorGrading.mixerBlueOutGreenIn);
            settings.channelMixerBlue.red.Apply(colorGrading.mixerBlueOutRedIn);
            settings.channelMixerGreen.blue.Apply(colorGrading.mixerGreenOutBlueIn);
            settings.channelMixerGreen.green.Apply(colorGrading.mixerGreenOutGreenIn);
            settings.channelMixerGreen.red.Apply(colorGrading.mixerGreenOutRedIn);
            settings.channelMixerRed.blue.Apply(colorGrading.mixerRedOutBlueIn);
            settings.channelMixerRed.green.Apply(colorGrading.mixerRedOutGreenIn);
            settings.channelMixerRed.red.Apply(colorGrading.mixerRedOutRedIn);
            settings.colorFilter.Apply(colorGrading.colorFilter);
            settings.contrast.Apply(colorGrading.contrast);
            settings.enabled.Apply(colorGrading.enabled);
            settings.gain.Apply(colorGrading.gain);
            settings.gamma.Apply(colorGrading.gamma);
            settings.hueShift.Apply(colorGrading.hueShift);
            settings.lift.Apply(colorGrading.lift);
            settings.saturation.Apply(colorGrading.saturation);
            settings.temperature.Apply(colorGrading.temperature);
            settings.tint.Apply(colorGrading.tint);
        }
        public void Apply(DepthOfFieldSettings settings)
        {
            postProcessVolume.profile.TryGetSettings<DepthOfField>(out var depthOfField);
            depthOfField.active = settings.active;
            settings.aperture.Apply(depthOfField.aperture);
            settings.enabled.Apply(depthOfField.enabled);
            settings.focalLength.Apply(depthOfField.focalLength);
            settings.focusDistance.Apply(depthOfField.focusDistance);
        }
        public void Apply(FogSettings settings)
        {
            //FogはRenderSettingsなので処理が特殊
            fogSettings.active = settings.active;
            settings.color.Apply(fogSettings.color);
            settings.density.Apply(fogSettings.density);
            settings.enabled.Apply(fogSettings.enabled);
            settings.end.Apply(fogSettings.end);
            settings.mode.Apply(fogSettings.mode);
            settings.start.Apply(fogSettings.start);

            fogSettings.Apply();
        }
        public void Apply(GrainSettings settings)
        {
            postProcessVolume.profile.TryGetSettings<Grain>(out var grain);
            grain.active = settings.active;
            settings.colored.Apply(grain.colored);
            settings.enabled.Apply(grain.enabled);
            settings.intensity.Apply(grain.intensity);
            settings.luminanceContribution.Apply(grain.lumContrib);
            settings.size.Apply(grain.size);
        }
        public void Apply(LensDistortionSettings settings)
        {
            postProcessVolume.profile.TryGetSettings<LensDistortion>(out var lensDistortion);
            lensDistortion.active = settings.active;
            settings.centerX.Apply(lensDistortion.centerX);
            settings.centerY.Apply(lensDistortion.centerY);
            settings.enabled.Apply(lensDistortion.enabled);
            settings.intensity.Apply(lensDistortion.intensity);
            settings.scale.Apply(lensDistortion.scale);
            settings.xMultiplier.Apply(lensDistortion.intensityX);
            settings.yMultiplier.Apply(lensDistortion.intensityY);
        }
        public void Apply(MotionBlurSettings settings)
        {
            postProcessVolume.profile.TryGetSettings<MotionBlur>(out var motionBlur);
            motionBlur.active = settings.active;
            settings.enabled.Apply(motionBlur.enabled);
            settings.sampleCount.Apply(motionBlur.sampleCount);
            settings.shutterAngle.Apply(motionBlur.shutterAngle);
        }
        public void Apply(VignetteSettings settings)
        {
            postProcessVolume.profile.TryGetSettings<Vignette>(out var vignette);
            vignette.active = settings.active;
            settings.center.Apply(vignette.center);
            settings.color.Apply(vignette.color);
            settings.enabled.Apply(vignette.enabled);
            settings.intensity.Apply(vignette.intensity);
            settings.rounded.Apply(vignette.rounded);
            settings.roundness.Apply(vignette.roundness);
            settings.smoothness.Apply(vignette.smoothness);
        }
    }
}
