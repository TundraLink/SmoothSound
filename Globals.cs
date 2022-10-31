using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine.Events;
using System.IO;
using System;
using BepInEx.Configuration;

namespace SmoothSound {
    public enum SoundState{
        FadingIn,
        FadingOut,
        Stopping,
        Playing,
        Idle
    }
    class Globals{
        public static void Init(GameController __instance){
            fadeFactor = Plugin.fadeFactor.Value;
            volStepLength = Plugin.fadeTime.Value / 1000f;
            switch(__instance.soundset){
                case 0:
                    trombVolume = Plugin.trombVolume.Value * Plugin.defaultVolume.Value;
                    break;
                case 1:
                    trombVolume = Plugin.trombVolume.Value * Plugin.bassVolume.Value;
                    break;
                case 2:
                    trombVolume = Plugin.trombVolume.Value * Plugin.mutedVolume.Value;
                    break;
                case 3:
                    trombVolume = Plugin.trombVolume.Value * Plugin.eightbitVolume.Value;
                    break;
                case 4:
                    trombVolume = Plugin.trombVolume.Value * Plugin.clubVolume.Value;
                    break;
                case 5:
                    trombVolume = Plugin.trombVolume.Value * Plugin.fartVolume.Value;
                    break;
                default:
                    trombVolume = Plugin.trombVolume.Value;
                    break;
            }

            List<float> temp = new List<float>();
            for(float i = 1f; i > 0.7f; i -= 0.03f){
                temp.Add(i);
            }
            temp.AddRange(new float[]{0.6f, 0.4f, 0.2f, 0.1f, 0f});
            fadeOutVols = temp.ToArray();
            stopVols = new float[]{1f, 0.35f, 0.122f, 0.043f, 0.015f, 0f};
            fadeInVols = new float[fadeOutVols.Length];
            for(int i = 0; i < fadeOutVols.Length; ++i){
                fadeInVols[i] = 10f*Mathf.Log10(2f-fadeOutVols[i]);
                if(fadeInVols[i] > 1f) fadeInVols[i] = 1f;
            }
            for(int i = 0; i < stopVols.Length; ++i){
                stopVols[i] *= trombVolume;
            }
            for(int i = 0; i < fadeOutVols.Length; ++i){
                fadeInVols[i] *= trombVolume;
                fadeOutVols[i] *= trombVolume;
            }
            foreach(float t in stopVols){
                Plugin.LogDebug("Stop :" + t.ToString());
            }
            foreach(float t in fadeOutVols){
                Plugin.LogDebug("Fade Out :" + t.ToString());
            }
            foreach(float t in fadeInVols){
                Plugin.LogDebug("Fade In :" + t.ToString());
            }
        }
        public static IEnumerator assignClips(GameController __instance){
            while(__instance.trombclips == null){
                yield return null;
            }
            tclips = __instance.trombclips.tclips;
            __instance.currentnotesound.clip = tclips[0];
            yield break;
        }
        public static float[] linePositions;
        public static AudioClip[] tclips;
        public static float fadeFactor;
        public static float volStepLength; //in seconds
        public static float[] fadeInVols;
        public static float[] fadeOutVols;
        public static float[] stopVols;
        public static float trombVolume;
        public static float semitoneDistance;
        public static float semitonePitch = Mathf.Pow(2f, 1f/12f);
        public static int totalSounds = 8;
    }
}