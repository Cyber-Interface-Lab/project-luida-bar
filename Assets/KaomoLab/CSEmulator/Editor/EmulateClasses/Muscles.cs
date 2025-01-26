using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class Muscles
    {
        public float[] muscles = new float[95].Select(n => 0.0f).ToArray();
        public bool[] changed = new bool[95].Select(n => false).ToArray();

        public static float[] TPOSE = new float[] {
            //0-9
            0,0,0,0,0,
            0,0,0,0,0,
            //10-19
            0,0,0,0,0,
            0,0,0,0,0,
            //20-29
            0,0.6001087f,0,0,0.9999163f,
            0,0,0,0,0.6001087f,
            //30-39
            0,0,0.9999163f,0,0,
            0,0,0,0,0.40003f,
            //40-49
            0.3000544f,0,0.9999161f,0,0,
            0,0,0,0.40003f,0.3000544f,
            //50-59
            0,0.999916f,0,0,0,
            -0.6632511f,0.4507931f,0.6462819f,0.6462819f,0.6679698f,
            //60-69
            -0.4573916f,0.8116841f,0.8116841f,0.6679698f,-0.6108312f,
            0.8116841f,0.8116841f,0.6679698f,-0.6108312f,0.8116841f,
            //70-79
            0.8116841f,0.6679698f,-0.4573916f,0.8116841f,0.8116841f,
            -0.663251f,0.4507929f,0.6462819f,0.6462819f,0.6679698f,
            //80-89
            -0.4573917f,0.8116841f,0.8116841f,0.6679698f,-0.6108311f,
            0.8116841f,0.8116841f,0.6679698f,-0.6108311f,0.8116841f,
            //90-94
            0.8116841f,0.6679698f,-0.4573917f,0.8116841f,0.8116841f
        };
        public static float[] SPOSE = new float[] {
            //0-9
            0,0,0,0,0,
            0,0,0,0,0,
            //10-19
            0,0,0,0,0,
            0,0,0,0,0,
            //20-29
            0,0.6001087f,0,0,0.9999163f,
            0,0,0,0,0.6001087f,
            //30-39
            0,0,0.9999163f,0,0,
            0,0,0,0,-0.68322f,
            //40-49
            0.163216f,0.2130028f,0.9999161f,0,0,
            0,0,0,-0.68322f,0.163216f,
            //50-59
            0.2130028f,0.999916f,0,0,0,
            -0.6632511f,0.4507931f,0.6462819f,0.6462819f,0.6679698f,
            //60-69
            -0.4573916f,0.8116841f,0.8116841f,0.6679698f,-0.6108312f,
            0.8116841f,0.8116841f,0.6679698f,-0.6108312f,0.8116841f,
            //70-79
            0.8116841f,0.6679698f,-0.4573916f,0.8116841f,0.8116841f,
            -0.663251f,0.4507929f,0.6462819f,0.6462819f,0.6679698f,
            //80-89
            -0.4573917f,0.8116841f,0.8116841f,0.6679698f,-0.6108311f,
            0.8116841f,0.8116841f,0.6679698f,-0.6108311f,0.8116841f,
            //90-94
            0.8116841f,0.6679698f,-0.4573917f,0.8116841f,0.8116841f
        };

        object _chestFrontBack = Jint.Native.JsValue.Undefined;
        public object chestFrontBack
        {
            get => _chestFrontBack;
            set => _chestFrontBack = NormalizeAndSet(value, 3);
        }

        object _chestLeftRight = Jint.Native.JsValue.Undefined;
        public object chestLeftRight
        {
            get => _chestLeftRight;
            set => _chestLeftRight = NormalizeAndSet(value, 4);
        }

        object _chestTwistLeftRight = Jint.Native.JsValue.Undefined;
        public object chestTwistLeftRight
        {
            get => _chestTwistLeftRight;
            set => _chestTwistLeftRight = NormalizeAndSet(value, 5);
        }

        object _headNodDownUp = Jint.Native.JsValue.Undefined;
        public object headNodDownUp
        {
            get => _headNodDownUp;
            set => _headNodDownUp = NormalizeAndSet(value, 12);
        }

        object _headTiltLeftRight = Jint.Native.JsValue.Undefined;
        public object headTiltLeftRight
        {
            get => _headTiltLeftRight;
            set => _headTiltLeftRight = NormalizeAndSet(value, 13);
        }

        object _headTurnLeftRight = Jint.Native.JsValue.Undefined;
        public object headTurnLeftRight
        {
            get => _headTurnLeftRight;
            set => _headTurnLeftRight = NormalizeAndSet(value, 14);
        }

        object _jawClose = Jint.Native.JsValue.Undefined;
        public object jawClose
        {
            get => _jawClose;
            set => _jawClose = NormalizeAndSet(value, 19);
        }

        object _jawLeftRight = Jint.Native.JsValue.Undefined;
        public object jawLeftRight
        {
            get => _jawLeftRight;
            set => _jawLeftRight = NormalizeAndSet(value, 20);
        }

        object _leftArmDownUp = Jint.Native.JsValue.Undefined;
        public object leftArmDownUp
        {
            get => _leftArmDownUp;
            set => _leftArmDownUp = NormalizeAndSet(value, 39);
        }

        object _leftArmFrontBack = Jint.Native.JsValue.Undefined;
        public object leftArmFrontBack
        {
            get => _leftArmFrontBack;
            set => _leftArmFrontBack = NormalizeAndSet(value, 40);
        }

        object _leftArmTwistInOut = Jint.Native.JsValue.Undefined;
        public object leftArmTwistInOut
        {
            get => _leftArmTwistInOut;
            set => _leftArmTwistInOut = NormalizeAndSet(value, 41);
        }

        object _leftEyeDownUp = Jint.Native.JsValue.Undefined;
        public object leftEyeDownUp
        {
            get => _leftEyeDownUp;
            set => _leftEyeDownUp = NormalizeAndSet(value, 15);
        }

        object _leftEyeInOut = Jint.Native.JsValue.Undefined;
        public object leftEyeInOut
        {
            get => _leftEyeInOut;
            set => _leftEyeInOut = NormalizeAndSet(value, 16);
        }

        object _leftFootTwistInOut = Jint.Native.JsValue.Undefined;
        public object leftFootTwistInOut
        {
            get => _leftFootTwistInOut;
            set => _leftFootTwistInOut = NormalizeAndSet(value, 27);
        }

        object _leftFootUpDown = Jint.Native.JsValue.Undefined;
        public object leftFootUpDown
        {
            get => _leftFootUpDown;
            set => _leftFootUpDown = NormalizeAndSet(value, 26);
        }

        object _leftForearmStretch = Jint.Native.JsValue.Undefined;
        public object leftForearmStretch
        {
            get => _leftForearmStretch;
            set => _leftForearmStretch = NormalizeAndSet(value, 42);
        }

        object _leftForearmTwistInOut = Jint.Native.JsValue.Undefined;
        public object leftForearmTwistInOut
        {
            get => _leftForearmTwistInOut;
            set => _leftForearmTwistInOut = NormalizeAndSet(value, 43);
        }

        object _leftHandDownUp = Jint.Native.JsValue.Undefined;
        public object leftHandDownUp
        {
            get => _leftHandDownUp;
            set => _leftHandDownUp = NormalizeAndSet(value, 44);
        }

        object _leftHandInOut = Jint.Native.JsValue.Undefined;
        public object leftHandInOut
        {
            get => _leftHandInOut;
            set => _leftHandInOut = NormalizeAndSet(value, 45);
        }

        object _leftIndex1Stretched = Jint.Native.JsValue.Undefined;
        public object leftIndex1Stretched
        {
            get => _leftIndex1Stretched;
            set => _leftIndex1Stretched = NormalizeAndSet(value, 59);
        }

        object _leftIndex2Stretched = Jint.Native.JsValue.Undefined;
        public object leftIndex2Stretched
        {
            get => _leftIndex2Stretched;
            set => _leftIndex2Stretched = NormalizeAndSet(value, 61);
        }

        object _leftIndex3Stretched = Jint.Native.JsValue.Undefined;
        public object leftIndex3Stretched
        {
            get => _leftIndex3Stretched;
            set => _leftIndex3Stretched = NormalizeAndSet(value, 62);
        }

        object _leftIndexSpread = Jint.Native.JsValue.Undefined;
        public object leftIndexSpread
        {
            get => _leftIndexSpread;
            set => _leftIndexSpread = NormalizeAndSet(value, 60);
        }

        object _leftLittle1Stretched = Jint.Native.JsValue.Undefined;
        public object leftLittle1Stretched
        {
            get => _leftLittle1Stretched;
            set => _leftLittle1Stretched = NormalizeAndSet(value, 71);
        }

        object _leftLittle2Stretched = Jint.Native.JsValue.Undefined;
        public object leftLittle2Stretched
        {
            get => _leftLittle2Stretched;
            set => _leftLittle2Stretched = NormalizeAndSet(value, 73);
        }

        object _leftLittle3Stretched = Jint.Native.JsValue.Undefined;
        public object leftLittle3Stretched
        {
            get => _leftLittle3Stretched;
            set => _leftLittle3Stretched = NormalizeAndSet(value, 74);
        }

        object _leftLittleSpread = Jint.Native.JsValue.Undefined;
        public object leftLittleSpread
        {
            get => _leftLittleSpread;
            set => _leftLittleSpread = NormalizeAndSet(value, 72);
        }

        object _leftLowerLegStretch = Jint.Native.JsValue.Undefined;
        public object leftLowerLegStretch
        {
            get => _leftLowerLegStretch;
            set => _leftLowerLegStretch = NormalizeAndSet(value, 24);
        }

        object _leftLowerLegTwistInOut = Jint.Native.JsValue.Undefined;
        public object leftLowerLegTwistInOut
        {
            get => _leftLowerLegTwistInOut;
            set => _leftLowerLegTwistInOut = NormalizeAndSet(value, 25);
        }

        object _leftMiddle1Stretched = Jint.Native.JsValue.Undefined;
        public object leftMiddle1Stretched
        {
            get => _leftMiddle1Stretched;
            set => _leftMiddle1Stretched = NormalizeAndSet(value, 63);
        }

        object _leftMiddle2Stretched = Jint.Native.JsValue.Undefined;
        public object leftMiddle2Stretched
        {
            get => _leftMiddle2Stretched;
            set => _leftMiddle2Stretched = NormalizeAndSet(value, 65);
        }

        object _leftMiddle3Stretched = Jint.Native.JsValue.Undefined;
        public object leftMiddle3Stretched
        {
            get => _leftMiddle3Stretched;
            set => _leftMiddle3Stretched = NormalizeAndSet(value, 66);
        }

        object _leftMiddleSpread = Jint.Native.JsValue.Undefined;
        public object leftMiddleSpread
        {
            get => _leftMiddleSpread;
            set => _leftMiddleSpread = NormalizeAndSet(value, 64);
        }

        object _leftRing1Stretched = Jint.Native.JsValue.Undefined;
        public object leftRing1Stretched
        {
            get => _leftRing1Stretched;
            set => _leftRing1Stretched = NormalizeAndSet(value, 67);
        }

        object _leftRing2Stretched = Jint.Native.JsValue.Undefined;
        public object leftRing2Stretched
        {
            get => _leftRing2Stretched;
            set => _leftRing2Stretched = NormalizeAndSet(value, 69);
        }

        object _leftRing3Stretched = Jint.Native.JsValue.Undefined;
        public object leftRing3Stretched
        {
            get => _leftRing3Stretched;
            set => _leftRing3Stretched = NormalizeAndSet(value, 70);
        }

        object _leftRingSpread = Jint.Native.JsValue.Undefined;
        public object leftRingSpread
        {
            get => _leftRingSpread;
            set => _leftRingSpread = NormalizeAndSet(value, 68);
        }

        object _leftShoulderDownUp = Jint.Native.JsValue.Undefined;
        public object leftShoulderDownUp
        {
            get => _leftShoulderDownUp;
            set => _leftShoulderDownUp = NormalizeAndSet(value, 37);
        }

        object _leftShoulderFrontBack = Jint.Native.JsValue.Undefined;
        public object leftShoulderFrontBack
        {
            get => _leftShoulderFrontBack;
            set => _leftShoulderFrontBack = NormalizeAndSet(value, 38);
        }

        object _leftThumb1Stretched = Jint.Native.JsValue.Undefined;
        public object leftThumb1Stretched
        {
            get => _leftThumb1Stretched;
            set => _leftThumb1Stretched = NormalizeAndSet(value, 55);
        }

        object _leftThumb2Stretched = Jint.Native.JsValue.Undefined;
        public object leftThumb2Stretched
        {
            get => _leftThumb2Stretched;
            set => _leftThumb2Stretched = NormalizeAndSet(value, 57);
        }

        object _leftThumb3Stretched = Jint.Native.JsValue.Undefined;
        public object leftThumb3Stretched
        {
            get => _leftThumb3Stretched;
            set => _leftThumb3Stretched = NormalizeAndSet(value, 58);
        }

        object _leftThumbSpread = Jint.Native.JsValue.Undefined;
        public object leftThumbSpread
        {
            get => _leftThumbSpread;
            set => _leftThumbSpread = NormalizeAndSet(value, 56);
        }

        object _leftToesUpDown = Jint.Native.JsValue.Undefined;
        public object leftToesUpDown
        {
            get => _leftToesUpDown;
            set => _leftToesUpDown = NormalizeAndSet(value, 28);
        }

        object _leftUpperLegFrontBack = Jint.Native.JsValue.Undefined;
        public object leftUpperLegFrontBack
        {
            get => _leftUpperLegFrontBack;
            set => _leftUpperLegFrontBack = NormalizeAndSet(value, 21);
        }

        object _leftUpperLegInOut = Jint.Native.JsValue.Undefined;
        public object leftUpperLegInOut
        {
            get => _leftUpperLegInOut;
            set => _leftUpperLegInOut = NormalizeAndSet(value, 22);
        }

        object _leftUpperLegTwistInOut = Jint.Native.JsValue.Undefined;
        public object leftUpperLegTwistInOut
        {
            get => _leftUpperLegTwistInOut;
            set => _leftUpperLegTwistInOut = NormalizeAndSet(value, 23);
        }

        object _neckNodDownUp = Jint.Native.JsValue.Undefined;
        public object neckNodDownUp
        {
            get => _neckNodDownUp;
            set => _neckNodDownUp = NormalizeAndSet(value, 9);
        }

        object _neckTiltLeftRight = Jint.Native.JsValue.Undefined;
        public object neckTiltLeftRight
        {
            get => _neckTiltLeftRight;
            set => _neckTiltLeftRight = NormalizeAndSet(value, 10);
        }

        object _neckTurnLeftRight = Jint.Native.JsValue.Undefined;
        public object neckTurnLeftRight
        {
            get => _neckTurnLeftRight;
            set => _neckTurnLeftRight = NormalizeAndSet(value, 11);
        }

        object _rightArmDownUp = Jint.Native.JsValue.Undefined;
        public object rightArmDownUp
        {
            get => _rightArmDownUp;
            set => _rightArmDownUp = NormalizeAndSet(value, 48);
        }

        object _rightArmFrontBack = Jint.Native.JsValue.Undefined;
        public object rightArmFrontBack
        {
            get => _rightArmFrontBack;
            set => _rightArmFrontBack = NormalizeAndSet(value, 49);
        }

        object _rightArmTwistInOut = Jint.Native.JsValue.Undefined;
        public object rightArmTwistInOut
        {
            get => _rightArmTwistInOut;
            set => _rightArmTwistInOut = NormalizeAndSet(value, 50);
        }

        object _rightEyeDownUp = Jint.Native.JsValue.Undefined;
        public object rightEyeDownUp
        {
            get => _rightEyeDownUp;
            set => _rightEyeDownUp = NormalizeAndSet(value, 17);
        }

        object _rightEyeInOut = Jint.Native.JsValue.Undefined;
        public object rightEyeInOut
        {
            get => _rightEyeInOut;
            set => _rightEyeInOut = NormalizeAndSet(value, 18);
        }

        object _rightFootTwistInOut = Jint.Native.JsValue.Undefined;
        public object rightFootTwistInOut
        {
            get => _rightFootTwistInOut;
            set => _rightFootTwistInOut = NormalizeAndSet(value, 35);
        }

        object _rightFootUpDown = Jint.Native.JsValue.Undefined;
        public object rightFootUpDown
        {
            get => _rightFootUpDown;
            set => _rightFootUpDown = NormalizeAndSet(value, 34);
        }

        object _rightForearmStretch = Jint.Native.JsValue.Undefined;
        public object rightForearmStretch
        {
            get => _rightForearmStretch;
            set => _rightForearmStretch = NormalizeAndSet(value, 51);
        }

        object _rightForearmTwistInOut = Jint.Native.JsValue.Undefined;
        public object rightForearmTwistInOut
        {
            get => _rightForearmTwistInOut;
            set => _rightForearmTwistInOut = NormalizeAndSet(value, 52);
        }

        object _rightHandDownUp = Jint.Native.JsValue.Undefined;
        public object rightHandDownUp
        {
            get => _rightHandDownUp;
            set => _rightHandDownUp = NormalizeAndSet(value, 53);
        }

        object _rightHandInOut = Jint.Native.JsValue.Undefined;
        public object rightHandInOut
        {
            get => _rightHandInOut;
            set => _rightHandInOut = NormalizeAndSet(value, 54);
        }

        object _rightIndex1Stretched = Jint.Native.JsValue.Undefined;
        public object rightIndex1Stretched
        {
            get => _rightIndex1Stretched;
            set => _rightIndex1Stretched = NormalizeAndSet(value, 79);
        }

        object _rightIndex2Stretched = Jint.Native.JsValue.Undefined;
        public object rightIndex2Stretched
        {
            get => _rightIndex2Stretched;
            set => _rightIndex2Stretched = NormalizeAndSet(value, 81);
        }

        object _rightIndex3Stretched = Jint.Native.JsValue.Undefined;
        public object rightIndex3Stretched
        {
            get => _rightIndex3Stretched;
            set => _rightIndex3Stretched = NormalizeAndSet(value, 82);
        }

        object _rightIndexSpread = Jint.Native.JsValue.Undefined;
        public object rightIndexSpread
        {
            get => _rightIndexSpread;
            set => _rightIndexSpread = NormalizeAndSet(value, 80);
        }

        object _rightLittle1Stretched = Jint.Native.JsValue.Undefined;
        public object rightLittle1Stretched
        {
            get => _rightLittle1Stretched;
            set => _rightLittle1Stretched = NormalizeAndSet(value, 91);
        }

        object _rightLittle2Stretched = Jint.Native.JsValue.Undefined;
        public object rightLittle2Stretched
        {
            get => _rightLittle2Stretched;
            set => _rightLittle2Stretched = NormalizeAndSet(value, 93);
        }

        object _rightLittle3Stretched = Jint.Native.JsValue.Undefined;
        public object rightLittle3Stretched
        {
            get => _rightLittle3Stretched;
            set => _rightLittle3Stretched = NormalizeAndSet(value, 94);
        }

        object _rightLittleSpread = Jint.Native.JsValue.Undefined;
        public object rightLittleSpread
        {
            get => _rightLittleSpread;
            set => _rightLittleSpread = NormalizeAndSet(value, 92);
        }

        object _rightLowerLegStretch = Jint.Native.JsValue.Undefined;
        public object rightLowerLegStretch
        {
            get => _rightLowerLegStretch;
            set => _rightLowerLegStretch = NormalizeAndSet(value, 32);
        }

        object _rightLowerLegTwistInOut = Jint.Native.JsValue.Undefined;
        public object rightLowerLegTwistInOut
        {
            get => _rightLowerLegTwistInOut;
            set => _rightLowerLegTwistInOut = NormalizeAndSet(value, 33);
        }

        object _rightMiddle1Stretched = Jint.Native.JsValue.Undefined;
        public object rightMiddle1Stretched
        {
            get => _rightMiddle1Stretched;
            set => _rightMiddle1Stretched = NormalizeAndSet(value, 83);
        }

        object _rightMiddle2Stretched = Jint.Native.JsValue.Undefined;
        public object rightMiddle2Stretched
        {
            get => _rightMiddle2Stretched;
            set => _rightMiddle2Stretched = NormalizeAndSet(value, 85);
        }

        object _rightMiddle3Stretched = Jint.Native.JsValue.Undefined;
        public object rightMiddle3Stretched
        {
            get => _rightMiddle3Stretched;
            set => _rightMiddle3Stretched = NormalizeAndSet(value, 86);
        }

        object _rightMiddleSpread = Jint.Native.JsValue.Undefined;
        public object rightMiddleSpread
        {
            get => _rightMiddleSpread;
            set => _rightMiddleSpread = NormalizeAndSet(value, 84);
        }

        object _rightRing1Stretched = Jint.Native.JsValue.Undefined;
        public object rightRing1Stretched
        {
            get => _rightRing1Stretched;
            set => _rightRing1Stretched = NormalizeAndSet(value, 87);
        }

        object _rightRing2Stretched = Jint.Native.JsValue.Undefined;
        public object rightRing2Stretched
        {
            get => _rightRing2Stretched;
            set => _rightRing2Stretched = NormalizeAndSet(value, 89);
        }

        object _rightRing3Stretched = Jint.Native.JsValue.Undefined;
        public object rightRing3Stretched
        {
            get => _rightRing3Stretched;
            set => _rightRing3Stretched = NormalizeAndSet(value, 90);
        }

        object _rightRingSpread = Jint.Native.JsValue.Undefined;
        public object rightRingSpread
        {
            get => _rightRingSpread;
            set => _rightRingSpread = NormalizeAndSet(value, 88);
        }

        object _rightShoulderDownUp = Jint.Native.JsValue.Undefined;
        public object rightShoulderDownUp
        {
            get => _rightShoulderDownUp;
            set => _rightShoulderDownUp = NormalizeAndSet(value, 46);
        }

        object _rightShoulderFrontBack = Jint.Native.JsValue.Undefined;
        public object rightShoulderFrontBack
        {
            get => _rightShoulderFrontBack;
            set => _rightShoulderFrontBack = NormalizeAndSet(value, 47);
        }

        object _rightThumb1Stretched = Jint.Native.JsValue.Undefined;
        public object rightThumb1Stretched
        {
            get => _rightThumb1Stretched;
            set => _rightThumb1Stretched = NormalizeAndSet(value, 75);
        }

        object _rightThumb2Stretched = Jint.Native.JsValue.Undefined;
        public object rightThumb2Stretched
        {
            get => _rightThumb2Stretched;
            set => _rightThumb2Stretched = NormalizeAndSet(value, 77);
        }

        object _rightThumb3Stretched = Jint.Native.JsValue.Undefined;
        public object rightThumb3Stretched
        {
            get => _rightThumb3Stretched;
            set => _rightThumb3Stretched = NormalizeAndSet(value, 78);
        }

        object _rightThumbSpread = Jint.Native.JsValue.Undefined;
        public object rightThumbSpread
        {
            get => _rightThumbSpread;
            set => _rightThumbSpread = NormalizeAndSet(value, 76);
        }

        object _rightToesUpDown = Jint.Native.JsValue.Undefined;
        public object rightToesUpDown
        {
            get => _rightToesUpDown;
            set => _rightToesUpDown = NormalizeAndSet(value, 36);
        }

        object _rightUpperLegFrontBack = Jint.Native.JsValue.Undefined;
        public object rightUpperLegFrontBack
        {
            get => _rightUpperLegFrontBack;
            set => _rightUpperLegFrontBack = NormalizeAndSet(value, 29);
        }

        object _rightUpperLegInOut = Jint.Native.JsValue.Undefined;
        public object rightUpperLegInOut
        {
            get => _rightUpperLegInOut;
            set => _rightUpperLegInOut = NormalizeAndSet(value, 30);
        }

        object _rightUpperLegTwistInOut = Jint.Native.JsValue.Undefined;
        public object rightUpperLegTwistInOut
        {
            get => _rightUpperLegTwistInOut;
            set => _rightUpperLegTwistInOut = NormalizeAndSet(value, 31);
        }

        object _spineFrontBack = Jint.Native.JsValue.Undefined;
        public object spineFrontBack
        {
            get => _spineFrontBack;
            set => _spineFrontBack = NormalizeAndSet(value, 0);
        }

        object _spineLeftRight = Jint.Native.JsValue.Undefined;
        public object spineLeftRight
        {
            get => _spineLeftRight;
            set => _spineLeftRight = NormalizeAndSet(value, 1);
        }

        object _spineTwistLeftRight = Jint.Native.JsValue.Undefined;
        public object spineTwistLeftRight
        {
            get => _spineTwistLeftRight;
            set => _spineTwistLeftRight = NormalizeAndSet(value, 2);
        }

        object _upperChestFrontBack = Jint.Native.JsValue.Undefined;
        public object upperChestFrontBack
        {
            get => _upperChestFrontBack;
            set => _upperChestFrontBack = NormalizeAndSet(value, 6);
        }

        object _upperChestLeftRight = Jint.Native.JsValue.Undefined;
        public object upperChestLeftRight
        {
            get => _upperChestLeftRight;
            set => _upperChestLeftRight = NormalizeAndSet(value, 7);
        }

        object _upperChestTwistLeftRight = Jint.Native.JsValue.Undefined;
        public object upperChestTwistLeftRight
        {
            get => _upperChestTwistLeftRight;
            set => _upperChestTwistLeftRight = NormalizeAndSet(value, 8);
        }

        object NormalizeAndSet(object value, int index)
        {
            var ret = Normalize(value);
            muscles[index] = 0;
            changed[index] = false;
            if (ret is float f)
            {
                muscles[index] = f;
                changed[index] = true;
            }
            return ret;
        }

        object Normalize(object value)
        {
            if (value == null) return Jint.Native.JsValue.Undefined;
            if (value is float f) return f;
            if (value is double d) return (float)d;
            if (value is Jint.Native.JsNumber jn) return Convert.ToSingle(jn.ToObject());

            UnityEngine.Debug.LogWarning(String.Format("need float value.[{0}][{1}]", value.GetType(), value));

            return Jint.Native.JsValue.Undefined;
        }

        public void Set(int index, object value)
        {
            switch (index)
            {
                case 3: chestFrontBack = value; break;
                case 4: chestLeftRight = value; break;
                case 5: chestTwistLeftRight = value; break;
                case 12: headNodDownUp = value; break;
                case 13: headTiltLeftRight = value; break;
                case 14: headTurnLeftRight = value; break;
                case 19: jawClose = value; break;
                case 20: jawLeftRight = value; break;
                case 39: leftArmDownUp = value; break;
                case 40: leftArmFrontBack = value; break;
                case 41: leftArmTwistInOut = value; break;
                case 15: leftEyeDownUp = value; break;
                case 16: leftEyeInOut = value; break;
                case 27: leftFootTwistInOut = value; break;
                case 26: leftFootUpDown = value; break;
                case 42: leftForearmStretch = value; break;
                case 43: leftForearmTwistInOut = value; break;
                case 44: leftHandDownUp = value; break;
                case 45: leftHandInOut = value; break;
                case 59: leftIndex1Stretched = value; break;
                case 61: leftIndex2Stretched = value; break;
                case 62: leftIndex3Stretched = value; break;
                case 60: leftIndexSpread = value; break;
                case 71: leftLittle1Stretched = value; break;
                case 73: leftLittle2Stretched = value; break;
                case 74: leftLittle3Stretched = value; break;
                case 72: leftLittleSpread = value; break;
                case 24: leftLowerLegStretch = value; break;
                case 25: leftLowerLegTwistInOut = value; break;
                case 63: leftMiddle1Stretched = value; break;
                case 65: leftMiddle2Stretched = value; break;
                case 66: leftMiddle3Stretched = value; break;
                case 64: leftMiddleSpread = value; break;
                case 67: leftRing1Stretched = value; break;
                case 69: leftRing2Stretched = value; break;
                case 70: leftRing3Stretched = value; break;
                case 68: leftRingSpread = value; break;
                case 37: leftShoulderDownUp = value; break;
                case 38: leftShoulderFrontBack = value; break;
                case 55: leftThumb1Stretched = value; break;
                case 57: leftThumb2Stretched = value; break;
                case 58: leftThumb3Stretched = value; break;
                case 56: leftThumbSpread = value; break;
                case 28: leftToesUpDown = value; break;
                case 21: leftUpperLegFrontBack = value; break;
                case 22: leftUpperLegInOut = value; break;
                case 23: leftUpperLegTwistInOut = value; break;
                case 9: neckNodDownUp = value; break;
                case 10: neckTiltLeftRight = value; break;
                case 11: neckTurnLeftRight = value; break;
                case 48: rightArmDownUp = value; break;
                case 49: rightArmFrontBack = value; break;
                case 50: rightArmTwistInOut = value; break;
                case 17: rightEyeDownUp = value; break;
                case 18: rightEyeInOut = value; break;
                case 35: rightFootTwistInOut = value; break;
                case 34: rightFootUpDown = value; break;
                case 51: rightForearmStretch = value; break;
                case 52: rightForearmTwistInOut = value; break;
                case 53: rightHandDownUp = value; break;
                case 54: rightHandInOut = value; break;
                case 79: rightIndex1Stretched = value; break;
                case 81: rightIndex2Stretched = value; break;
                case 82: rightIndex3Stretched = value; break;
                case 80: rightIndexSpread = value; break;
                case 91: rightLittle1Stretched = value; break;
                case 93: rightLittle2Stretched = value; break;
                case 94: rightLittle3Stretched = value; break;
                case 92: rightLittleSpread = value; break;
                case 32: rightLowerLegStretch = value; break;
                case 33: rightLowerLegTwistInOut = value; break;
                case 83: rightMiddle1Stretched = value; break;
                case 85: rightMiddle2Stretched = value; break;
                case 86: rightMiddle3Stretched = value; break;
                case 84: rightMiddleSpread = value; break;
                case 87: rightRing1Stretched = value; break;
                case 89: rightRing2Stretched = value; break;
                case 90: rightRing3Stretched = value; break;
                case 88: rightRingSpread = value; break;
                case 46: rightShoulderDownUp = value; break;
                case 47: rightShoulderFrontBack = value; break;
                case 75: rightThumb1Stretched = value; break;
                case 77: rightThumb2Stretched = value; break;
                case 78: rightThumb3Stretched = value; break;
                case 76: rightThumbSpread = value; break;
                case 36: rightToesUpDown = value; break;
                case 29: rightUpperLegFrontBack = value; break;
                case 30: rightUpperLegInOut = value; break;
                case 31: rightUpperLegTwistInOut = value; break;
                case 0: spineFrontBack = value; break;
                case 1: spineLeftRight = value; break;
                case 2: spineTwistLeftRight = value; break;
                case 6: upperChestFrontBack = value; break;
                case 7: upperChestLeftRight = value; break;
                case 8: upperChestTwistLeftRight = value; break;
                default: throw new Exception(String.Format("illigal muscle number:{0}", index));
            }
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.chestFrontBack = chestFrontBack is float ? chestFrontBack : null;
            o.chestLeftRight = chestLeftRight is float ? chestLeftRight : null;
            o.chestTwistLeftRight = chestTwistLeftRight is float ? chestTwistLeftRight : null; ;
            o.headNodDownUp = headNodDownUp is float ? headNodDownUp : null;
            o.headTiltLeftRight = headTiltLeftRight is float ? headTiltLeftRight : null;
            o.headTurnLeftRight = headTurnLeftRight is float ? headTurnLeftRight : null;
            o.jawClose = jawClose is float ? jawClose : null;
            o.jawLeftRight = jawLeftRight is float ? jawLeftRight : null;
            o.leftArmDownUp = leftArmDownUp is float ? leftArmDownUp : null;
            o.leftArmFrontBack = leftArmFrontBack is float ? leftArmFrontBack : null;
            o.leftArmTwistInOut = leftArmTwistInOut is float ? leftArmTwistInOut : null;
            o.leftEyeDownUp = leftEyeDownUp is float ? leftEyeDownUp : null;
            o.leftEyeInOut = leftEyeInOut is float ? leftEyeInOut : null;
            o.leftFootTwistInOut = leftFootTwistInOut is float ? leftFootTwistInOut : null;
            o.leftFootUpDown = leftFootUpDown is float ? leftFootUpDown : null;
            o.leftForearmStretch = leftForearmStretch is float ? leftForearmStretch : null;
            o.leftForearmTwistInOut = leftForearmTwistInOut is float ? leftForearmTwistInOut : null;
            o.leftHandDownUp = leftHandDownUp is float ? leftHandDownUp : null;
            o.leftHandInOut = leftHandInOut is float ? leftHandInOut : null;
            o.leftIndex1Stretched = leftIndex1Stretched is float ? leftIndex1Stretched : null;
            o.leftIndex2Stretched = leftIndex2Stretched is float ? leftIndex2Stretched : null;
            o.leftIndex3Stretched = leftIndex3Stretched is float ? leftIndex3Stretched : null;
            o.leftIndexSpread = leftIndexSpread is float ? leftIndexSpread : null;
            o.leftLittle1Stretched = leftLittle1Stretched is float ? leftLittle1Stretched : null;
            o.leftLittle2Stretched = leftLittle2Stretched is float ? leftLittle2Stretched : null;
            o.leftLittle3Stretched = leftLittle3Stretched is float ? leftLittle3Stretched : null;
            o.leftLittleSpread = leftLittleSpread is float ? leftLittleSpread : null;
            o.leftLowerLegStretch = leftLowerLegStretch is float ? leftLowerLegStretch : null;
            o.leftLowerLegTwistInOut = leftLowerLegTwistInOut is float ? leftLowerLegTwistInOut : null;
            o.leftMiddle1Stretched = leftMiddle1Stretched is float ? leftMiddle1Stretched : null;
            o.leftMiddle2Stretched = leftMiddle2Stretched is float ? leftMiddle2Stretched : null;
            o.leftMiddle3Stretched = leftMiddle3Stretched is float ? leftMiddle3Stretched : null;
            o.leftMiddleSpread = leftMiddleSpread is float ? leftMiddleSpread : null;
            o.leftRing1Stretched = leftRing1Stretched is float ? leftRing1Stretched : null;
            o.leftRing2Stretched = leftRing2Stretched is float ? leftRing2Stretched : null;
            o.leftRing3Stretched = leftRing3Stretched is float ? leftRing3Stretched : null;
            o.leftRingSpread = leftRingSpread is float ? leftRingSpread : null;
            o.leftShoulderDownUp = leftShoulderDownUp is float ? leftShoulderDownUp : null;
            o.leftShoulderFrontBack = leftShoulderFrontBack is float ? leftShoulderFrontBack : null;
            o.leftThumb1Stretched = leftThumb1Stretched is float ? leftThumb1Stretched : null;
            o.leftThumb2Stretched = leftThumb2Stretched is float ? leftThumb2Stretched : null;
            o.leftThumb3Stretched = leftThumb3Stretched is float ? leftThumb3Stretched : null;
            o.leftThumbSpread = leftThumbSpread is float ? leftThumbSpread : null;
            o.leftToesUpDown = leftToesUpDown is float ? leftToesUpDown : null;
            o.leftUpperLegFrontBack = leftUpperLegFrontBack is float ? leftUpperLegFrontBack : null;
            o.leftUpperLegInOut = leftUpperLegInOut is float ? leftUpperLegInOut : null;
            o.leftUpperLegTwistInOut = leftUpperLegTwistInOut is float ? leftUpperLegTwistInOut : null;
            o.neckNodDownUp = neckNodDownUp is float ? neckNodDownUp : null;
            o.neckTiltLeftRight = neckTiltLeftRight is float ? neckTiltLeftRight : null;
            o.neckTurnLeftRight = neckTurnLeftRight is float ? neckTurnLeftRight : null;
            o.rightArmDownUp = rightArmDownUp is float ? rightArmDownUp : null;
            o.rightArmFrontBack = rightArmFrontBack is float ? rightArmFrontBack : null;
            o.rightArmTwistInOut = rightArmTwistInOut is float ? rightArmTwistInOut : null;
            o.rightEyeDownUp = rightEyeDownUp is float ? rightEyeDownUp : null;
            o.rightEyeInOut = rightEyeInOut is float ? rightEyeInOut : null;
            o.rightFootTwistInOut = rightFootTwistInOut is float ? rightFootTwistInOut : null;
            o.rightFootUpDown = rightFootUpDown is float ? rightFootUpDown : null;
            o.rightForearmStretch = rightForearmStretch is float ? rightForearmStretch : null;
            o.rightForearmTwistInOut = rightForearmTwistInOut is float ? rightForearmTwistInOut : null;
            o.rightHandDownUp = rightHandDownUp is float ? rightHandDownUp : null;
            o.rightHandInOut = rightHandInOut is float ? rightHandInOut : null;
            o.rightIndex1Stretched = rightIndex1Stretched is float ? rightIndex1Stretched : null;
            o.rightIndex2Stretched = rightIndex2Stretched is float ? rightIndex2Stretched : null;
            o.rightIndex3Stretched = rightIndex3Stretched is float ? rightIndex3Stretched : null;
            o.rightIndexSpread = rightIndexSpread is float ? rightIndexSpread : null;
            o.rightLittle1Stretched = rightLittle1Stretched is float ? rightLittle1Stretched : null;
            o.rightLittle2Stretched = rightLittle2Stretched is float ? rightLittle2Stretched : null;
            o.rightLittle3Stretched = rightLittle3Stretched is float ? rightLittle3Stretched : null;
            o.rightLittleSpread = rightLittleSpread is float ? rightLittleSpread : null;
            o.rightLowerLegStretch = rightLowerLegStretch is float ? rightLowerLegStretch : null;
            o.rightLowerLegTwistInOut = rightLowerLegTwistInOut is float ? rightLowerLegTwistInOut : null;
            o.rightMiddle1Stretched = rightMiddle1Stretched is float ? rightMiddle1Stretched : null;
            o.rightMiddle2Stretched = rightMiddle2Stretched is float ? rightMiddle2Stretched : null;
            o.rightMiddle3Stretched = rightMiddle3Stretched is float ? rightMiddle3Stretched : null;
            o.rightMiddleSpread = rightMiddleSpread is float ? rightMiddleSpread : null;
            o.rightRing1Stretched = rightRing1Stretched is float ? rightRing1Stretched : null;
            o.rightRing2Stretched = rightRing2Stretched is float ? rightRing2Stretched : null;
            o.rightRing3Stretched = rightRing3Stretched is float ? rightRing3Stretched : null;
            o.rightRingSpread = rightRingSpread is float ? rightRingSpread : null;
            o.rightShoulderDownUp = rightShoulderDownUp is float ? rightShoulderDownUp : null;
            o.rightShoulderFrontBack = rightShoulderFrontBack is float ? rightShoulderFrontBack : null;
            o.rightThumb1Stretched = rightThumb1Stretched is float ? rightThumb1Stretched : null;
            o.rightThumb2Stretched = rightThumb2Stretched is float ? rightThumb2Stretched : null;
            o.rightThumb3Stretched = rightThumb3Stretched is float ? rightThumb3Stretched : null;
            o.rightThumbSpread = rightThumbSpread is float ? rightThumbSpread : null;
            o.rightToesUpDown = rightToesUpDown is float ? rightToesUpDown : null;
            o.rightUpperLegFrontBack = rightUpperLegFrontBack is float ? rightUpperLegFrontBack : null;
            o.rightUpperLegInOut = rightUpperLegInOut is float ? rightUpperLegInOut : null;
            o.rightUpperLegTwistInOut = rightUpperLegTwistInOut is float ? rightUpperLegTwistInOut : null;
            o.spineFrontBack = spineFrontBack is float ? spineFrontBack : null;
            o.spineLeftRight = spineLeftRight is float ? spineLeftRight : null;
            o.spineTwistLeftRight = spineTwistLeftRight is float ? spineTwistLeftRight : null;
            o.upperChestFrontBack = upperChestFrontBack is float ? upperChestFrontBack : null;
            o.upperChestLeftRight = upperChestLeftRight is float ? upperChestLeftRight : null;
            o.upperChestTwistLeftRight = upperChestTwistLeftRight is float ? upperChestTwistLeftRight : null;

            return o;
        }
        public override string ToString()
        {
            var o = (System.Dynamic.ExpandoObject)toJSON("");
            var sets = new List<string>();
            foreach (var kv in o)
            {
                if (Jint.Native.JsValue.Undefined.Equals(kv.Value)) continue;
                sets.Add(kv.Key+":"+kv.Value);
            }
            return String.Format("[Muscles][{0}]", String.Join(",", sets));
        }

    }
}
