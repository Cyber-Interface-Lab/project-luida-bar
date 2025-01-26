using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class FogSettingsBridge
    {
        public bool active;

        public ParameterOverride<Color> color { get; } = new ParameterOverride<Color>();
        public ParameterOverride<float> density { get; } = new ParameterOverride<float>();
        public ParameterOverride<bool> enabled { get; } = new ParameterOverride<bool>();
        public ParameterOverride<float> end { get; } = new ParameterOverride<float>();
        public ParameterOverride<string> mode { get; } = new ParameterOverride<string>();
        public ParameterOverride<float> start { get; } = new ParameterOverride<float>();

        Color _color;
        float _density;
        bool _enabled;
        float _end;
        FogMode _mode;
        float _start;

        public void Apply()
        {
            //CCK2.9.0 デフォでOFFの場合は一切適用されない。
            if (!_enabled)
            {
                RenderSettings.fog = false;
                return;
            }

            if (this.active)
            {
                //CSETODO 2.8.0.1 fogModeを変えると設定が無視される模様
                if (this.mode.overrideState && RenderSettings.fogMode != FogModeToEnum(this.mode.value, _mode))
                {
                    RenderSettings.fog = false;
                    RenderSettings.fogMode = this.mode.overrideState ? FogModeToEnum(this.mode.value, _mode) : _mode;
                }
                else
                {
                    RenderSettings.fog = this.enabled.overrideState ? this.enabled.value : _enabled;
                    RenderSettings.fogColor = this.color.overrideState ? this.color.value : _color;
                    RenderSettings.fogDensity = this.density.overrideState ? this.density.value : _density;
                    RenderSettings.fogEndDistance = this.end.overrideState ? this.end.value : _end;
                    RenderSettings.fogMode = this.mode.overrideState ? FogModeToEnum(this.mode.value, _mode) : _mode;
                    RenderSettings.fogStartDistance = this.start.overrideState ? this.start.value : _start;
                }
            }
            else
            {
                RenderSettings.fog = _enabled;
                RenderSettings.fogColor = _color;
                RenderSettings.fogDensity = _density;
                RenderSettings.fogEndDistance = _end;
                RenderSettings.fogMode = _mode;
                RenderSettings.fogStartDistance = _start;
            }
        }

        public void Start()
        {
            //_color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            //_density = 0f;
            //_end = 300f;
            //_mode = FogMode.ExponentialSquared;
            //_start = 0f;
            _enabled = RenderSettings.fog;
            _color = RenderSettings.fogColor;
            _density = RenderSettings.fogDensity;
            _end = RenderSettings.fogEndDistance;
            _mode = RenderSettings.fogMode;
            _start = RenderSettings.fogStartDistance;
        }

        string FogModeToString(FogMode mode)
        {
            return mode switch
            {
                FogMode.Linear => "Linear",
                FogMode.Exponential => "Exponential",
                FogMode.ExponentialSquared => "ExponentialSquared",
                _ => throw new ArgumentOutOfRangeException(mode.ToString())
            };
        }
        FogMode FogModeToEnum(string mode, FogMode other)
        {
            return mode switch
            {
                "Linear" => FogMode.Linear,
                "Exponential" => FogMode.Exponential,
                "ExponentialSquared" => FogMode.ExponentialSquared,
                _ => other
            };
        }
    }
}
