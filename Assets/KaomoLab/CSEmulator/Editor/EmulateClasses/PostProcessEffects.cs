using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class PostProcessEffects
    {
        public BloomSettings bloom = new BloomSettings();
        public ChromaticAberrationSettings chromaticAberration = new ChromaticAberrationSettings();
        public ColorGradingSettings colorGrading = new ColorGradingSettings();
        public DepthOfFieldSettings depthOfField = new DepthOfFieldSettings();
        public FogSettings fog = new FogSettings();
        public GrainSettings grain = new GrainSettings();
        public LensDistortionSettings lensDistortion = new LensDistortionSettings();
        public MotionBlurSettings motionBlur =  new MotionBlurSettings();
        public VignetteSettings vignette = new VignetteSettings();

        public PostProcessEffects() { }
    }
}
