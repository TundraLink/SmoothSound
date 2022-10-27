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
using Mono.Cecil;
using BepInEx.Configuration;
using TrombSettings;

namespace SmoothSound
{
    [HarmonyPatch]
    [BepInPlugin("SmoothSound", "SmoothSound", "1.0.1")]
    [BepInDependency("com.hypersonicsharkz.trombsettings")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake(){
            Instance = this;

            pluginEnabled = Config.Bind<bool>("General Settings", "Enabled", true, "Enable the mod");
            trackVolume = Config.Bind<float>("Sound settings", "TrackVolume", 1f, "Set the volume of the backing music track");
            trombVolume = Config.Bind<float>("Sound settings", "TromboneVolume", 1f, "Set the volume of the trombone");
            useDefaultPitch = Config.Bind<bool>("Note Settings", "UseDefaultPitch", false, "Use the default way of finding pitch");
            useFade = Config.Bind<bool>("Note Settings", "UseFade", true, "Use fading when the current note is far from where it started");
            fadeFactor = Config.Bind<float>("Note settings", "TransitionDistance", 3f, "Additional distance before played note transitions to nearest clip");
            fadeTime = Config.Bind<float>("Note settings", "FadeTime", 5f, "Time in ms between fade steps");
            
            TrombEntryList trombEntryList = TrombConfig.TrombSettings["Smooth Sound"]; 
            trombEntryList.Add(pluginEnabled);
            trombEntryList.Add(new StepSliderConfig(0f, 1f, 0.01f, false, trackVolume));
            trombEntryList.Add(new StepSliderConfig(0f, 1f, 0.01f, false, trombVolume));
            trombEntryList.Add(useDefaultPitch);
            trombEntryList.Add(useFade);
            trombEntryList.Add(new StepSliderConfig(0f, 12f, 0.2f, false, fadeFactor));
            trombEntryList.Add(new StepSliderConfig(1f, 20f, 0.5f, false, fadeTime));
            if(pluginEnabled.Value){
                new Harmony("SmoothSound").PatchAll();
                Logger.LogDebug("SmoothSound Loaded");
            }
        }

        internal static void LogDebug(string message)
		{
			Plugin.Instance.Log(message, LogLevel.Debug);
		}

		internal static void LogInfo(string message)
		{
			Plugin.Instance.Log(message, LogLevel.Info);
		}

		internal static void LogWarning(string message)
		{
			Plugin.Instance.Log(message, LogLevel.Warning);
		}

		internal static void LogError(string message)
		{
			Plugin.Instance.Log(message, LogLevel.Error);
		}

		private void Log(string message, LogLevel logLevel)
		{
			base.Logger.Log(logLevel, message);
		}

		public static Plugin Instance;
        
        public static ConfigEntry<float> trackVolume;
        public static ConfigEntry<float> trombVolume;
        public static ConfigEntry<float> fadeFactor;
        public static ConfigEntry<float> fadeTime;
        public static ConfigEntry<bool> pluginEnabled;
        public static ConfigEntry<bool> useDefaultPitch;
        public static ConfigEntry<bool> useFade;
    }

}