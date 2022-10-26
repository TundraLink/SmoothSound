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
using BepInEx.Configuration;

namespace SmoothSound
{
    [HarmonyPatch(typeof(GameController))]
    public class TrombSoundManager : MonoBehaviour
    {   
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPatch(GameController __instance){
            Globals.Init();
            unusedSounds = new LinkedList<Sound>();
            fadingSounds = new LinkedList<Sound>();
            for(int i = 0; i < Globals.totalSounds; i++){
                unusedSounds.AddFirst(new Sound(GameObject.Instantiate<AudioSource>(__instance.currentnotesound, __instance.currentnotesound.transform.parent), 0f));
            }
            Globals.linePositions = __instance.notelinepos;
            Globals.semitoneDistance = __instance.vbounds/12f;
            pointer = __instance.pointer;
            __instance.musictrack.volume = Plugin.trackVolume.Value;
            __instance.StartCoroutine(Globals.assignClips(__instance));
        }

        [HarmonyPatch("stopNote")]
        [HarmonyPrefix]
        private static bool StopFix(){
            nextAction = SoundState.Stopping;
            return false;
        }

        [HarmonyPatch("playNote")]
        [HarmonyPrefix]
        private static bool PlayFix(GameController __instance){
            nextAction = SoundState.Playing;
            return false;
        }

        public static AudioClip getClipAtPos(int pos){
            return Globals.tclips[Mathf.Abs(pos - (Globals.tclips.Length-1))];
        }
        public static AudioClip getClipAtCurrentPos(){
            return getClipAtPos(posNum);
        }

        public static int findNearestPos(){
            float num = 9999f;
            int num2 = 0;
            for (int i = 0; i < Globals.linePositions.Length; i++)
            {
                float num3 = Mathf.Abs(Globals.linePositions[i] - pointer.transform.localPosition.y);
                if (num3 < num)
                {
                    num = num3;
                    num2 = i;
                }
            }
            return num2;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void updateSound(){
            pointerYPos = pointer.transform.localPosition.y;
            posNum = findNearestPos(); 
            currentVolStepTime += Time.deltaTime;
            currentAction = nextAction;
            nextAction = SoundState.Idle;

            //Set States
            setStateUpdate();

            //Update values
            updateValuesUpdate();

            //Set next action
            if(Plugin.useFade.Value) setNextActionUpdate();

            if(currentVolStepTime >= Globals.volStepLength) currentVolStepTime = 0f;
        }

        // Loads a new sound into current and sets states
        private static void loadCurrent(SoundState state, SoundState notState){
            if(current != null){
                current.Value.setState(notState);
                fadingSounds.AddFirst(current);
                current = null;
            }
            if(unusedSounds.Count > 0){
                current = unusedSounds.First;
                unusedSounds.RemoveFirst();
            }
            else {
                current = fadingSounds.Last;
                fadingSounds.RemoveLast();
            }
            current.Value.setState(state);
        }

        // Set state of all sounds this frame
        public static void setStateUpdate(){
            if(currentAction == SoundState.Stopping){
                if(current != null){
                    current.Value.setState(SoundState.Stopping);
                    fadingSounds.AddFirst(current);
                    current = null;
                }
            }
            else if(currentAction == SoundState.Playing){
                loadCurrent(SoundState.Playing, SoundState.Stopping);
            }
            else if(currentAction == SoundState.FadingIn){
                if(current != null){
                    fadeInSampleTime = current.Value.source.time > current.Value.source.clip.length - 1.25f 
                        ? current.Value.source.clip.frequency 
                        : current.Value.source.timeSamples;
                }
                loadCurrent(SoundState.FadingIn, SoundState.FadingOut);
            }
        }

        // Update values of all sounds this frame
        public static void updateValuesUpdate(){
            if(current != null){
                if(current.Value.isActing) current.Value.actOnState();
                else current.Value.startActing();
            }
            int fadeOutCount = 0;
            for(LinkedListNode<Sound> node = fadingSounds.First; node != null; node = node.Next){

                if(node.Value.state == SoundState.FadingOut && fadeOutCount >= 1) node.Value.setState(SoundState.Stopping);
                else fadeOutCount++;

                if(node.Value.isActing) node.Value.actOnState();
                else node.Value.startActing();

                if(!node.Value.source.isPlaying){
                    fadingSounds.Remove(node);
                    unusedSounds.AddLast(node);
                }
            }
        }

        // Set action for next frame
        public static void setNextActionUpdate(){
            if(current != null){
                bool changeNote = Mathf.Abs(Globals.linePositions[posNum] - pointerYPos) + (Globals.semitoneDistance * Globals.fadeFactor) < Mathf.Abs(current.Value.startPos - pointerYPos);
                bool loopClip = current.Value.source.time > current.Value.source.clip.length - 1.25f;
                if(changeNote || loopClip){
                    nextAction = SoundState.FadingIn;
                    nextPos = posNum;
                }
            }
        }
        public static SoundState currentAction = SoundState.Idle;
        public static SoundState nextAction = SoundState.Idle;
        public static float pointerYPos;
        public static GameObject pointer;
        public static int posNum = 0;
        public static int nextPos = 0;
        private static LinkedList<Sound> unusedSounds;
        private static LinkedList<Sound> fadingSounds;
        private static LinkedListNode<Sound> current = null;
        public static int fadeInSampleTime = 0; //Time in samples to fade in to when transitioning notes
        public static float currentVolStepTime = 0f;
        
        
    }

}