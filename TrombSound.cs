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

namespace SmoothSound
{
    public class Sound{
        public Sound(AudioSource source, float startPos, SoundState state = SoundState.Idle){
            this.source = source;
            this.startPos = startPos;
            this.state = state;
            if(Plugin.useDefaultPitch.Value){
                adjustPitch = adjustPitchDefault;
            }
            else{
                adjustPitch = adjustPitchCustom;
            }
        }
        public void setState(SoundState s, int v = 1){
            state = s;
            volStage = v;
            isActing = false;
        }
        public void startActing(){
            switch (state){
                case SoundState.Playing: 
                    startPlaying();
                    break;
                case SoundState.Stopping: 
                    startStopping();
                    break;
                case SoundState.FadingIn: 
                    startFadingIn();
                    break;
                case SoundState.FadingOut: 
                    startFadingOut();
                    break;
                default: 
                    break;
            }
            isActing = true;
        }
        public void actOnState(){
            
            if(TrombSoundManager.currentVolStepTime >= Globals.volStepLength){
                switch (state){
                    case SoundState.Stopping: 
                        source.volume = Globals.stopVols[volStage++];
                        if(volStage >= Globals.stopVols.Length - 1){
                            source.Stop();
                            isActing = false;
                        }
                        break;
                    case SoundState.FadingIn: 
                        source.volume = Globals.fadeInVols[volStage++];
                        if(volStage >= Globals.fadeInVols.Length - 1){
                            source.volume = 1f;
                            state = SoundState.Playing;
                        }
                        break;
                    case SoundState.FadingOut: 
                        source.volume = Globals.fadeOutVols[volStage++];
                        if(volStage >= Globals.fadeOutVols.Length - 1){
                            source.Stop();
                            isActing = false;
                        }
                        break;
                    default: 
                        break;
                }
            }
            adjustPitch();
        }
        public void startPlaying(){
            if(source.isPlaying) source.Stop();
            source.clip = TrombSoundManager.getClipAtCurrentPos();
            startPos = Globals.linePositions[TrombSoundManager.posNum];
            source.volume = Globals.trombVolume;
            adjustPitch();
            source.Play();
            source.timeSamples = 0;
        }
        public void startStopping(){
            for(int i = 0; i < Globals.stopVols.Length-1; ++i){
                if(Globals.stopVols[i] <= source.volume && Globals.stopVols[i+1] > source.volume){
                    volStage = i+1;
                    break;
                }
                else if(i+1 >= Globals.stopVols.Length){
                    volStage = Globals.stopVols.Length-1;
                    break;
                }
            }
        }
        public void startFadingIn(){
            if(source.isPlaying) source.Stop();
            source.clip = TrombSoundManager.getClipAtCurrentPos();
            startPos = Globals.linePositions[TrombSoundManager.posNum];
            source.volume = 0f;
            
            adjustPitch();
            source.Play();
            source.timeSamples = TrombSoundManager.fadeInSampleTime;
        }
        public void startFadingOut(){
            for(int i = 0; i < Globals.fadeOutVols.Length-1; ++i){
                if(Globals.fadeOutVols[i] <= source.volume && Globals.fadeOutVols[i+1] > source.volume){
                    volStage = i+1;
                    break;
                }
                else if(i+1 >= Globals.fadeOutVols.Length){
                    volStage = Globals.fadeOutVols.Length-1;
                    break;
                }
            }
        }
        public void adjustPitchCustom(){
            source.pitch = Mathf.Pow(Globals.semitonePitch, (TrombSoundManager.pointerYPos-startPos)/Globals.semitoneDistance);
        }

        public void adjustPitchDefault(){
            float num18 = Mathf.Pow(startPos - TrombSoundManager.pointerYPos, 2f) * 6.8E-06f;
            float num19 = (startPos - TrombSoundManager.pointerYPos) * (1f + num18);
            if (num19 > 0f)
            {
                num19 = (startPos - TrombSoundManager.pointerYPos) * 1.392f;
                num19 *= 0.5f;
            }
            source.pitch = 1f - num19 * 0.00501f;
            if (source.pitch > 2f)
            {
                source.pitch = 2f;
            }
            else if (source.pitch < 0.5f)
            {
                source.pitch = 0.5f;
            }
        }
        public Action adjustPitch;
        public AudioSource source;
        public float startPos;
        public SoundState state;
        public int volStage = 1;
        public bool isActing = false;
    }
}