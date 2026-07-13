/*
 * Static class that contains utility functions.
 */

using System;
using UnityEngine;

public static class Utilities 
{
    /*
     *  Time Converter
     */
    public static TimeSpan FromIntToTime(int days, int hours, int minutes, int seconds, int milliseconds)
    {
        return  new TimeSpan(days, hours, minutes, seconds, milliseconds);
    }

    //overload method
    public static TimeSpan FromIntToTime(int minutes, int seconds, int milliseconds) 
    { 
        return FromIntToTime(0,0,minutes, seconds, milliseconds);
    }

    //Convert time expressed as an integer into a float representing time in the format totalSeconds.milliseconds.
    public static float FromIntTimeToFloat(int days, int hours, int minutes, int seconds, int milliseconds)
    {
        TimeSpan timeSpan = new TimeSpan(days, hours, minutes, seconds, milliseconds);

        //Convert TimeSpan to float (seconds.milliseconds)
        return (float)timeSpan.TotalSeconds;
    }

    //overload method
    public static float FromIntTimeToFloat(int minutes, int seconds, int milliseconds) 
    {
        return FromIntTimeToFloat(0,0, minutes, seconds, milliseconds);
    }

    /*
     *  Gravity Utilities
     */

    //Jump
    public static void SetupJumpValues(float jumpTime, float jumpHeight, float gravity, out float jumpMultiplier, out float jumpVelocity)
    {
        /*
         * We aim to reach a height (jumpHeight) and then descend within a total time of jumpTime seconds.
         * The motion follows a uniformly accelerated rectilinear trajectory.
         * Definitions:
         *   - H: The target height (jumpHeight).
         *   - Vo: The initial velocity required.
         *   - tH: The time required to reach the peak height.
         *   - T: The total time for the jump (jumpTime).
         *
         * Derivations:
         * From the velocity equation V(t) = g*t + Vo:
         *   - At t = tH, V(tH) = 0, g = gravity.
         *   - (1) Vo = -g * tH, where tH = T / 2, as it's the peak time of a parabola.
         *
         * From the displacement equation X(t) = Xo + Vo*t + (1/2)*g*t^2:
         *   - At X(tH) = H, using the previously derived velocity equation, we obtain the following result:
         *   - (2) H = -(1/2) * g * tH^2.
         *
         * By substituting (2) into (1), we can calculate the required initial velocity (Vo).
         */

        float timeToMax = jumpTime / 2; //The peak of a parabola is located at its midpoint!
        float newGravity = (-2 * jumpHeight) / Mathf.Pow(timeToMax, 2); //Derived from formula (2)
        jumpMultiplier = newGravity / gravity; //Since the fall time is intended to be T/2,
                                               //the gravity value must be adjusted accordingly.
        jumpVelocity = (2 * jumpHeight) / timeToMax; //The initial velocity required for the jump.
    }

    /*
     *  AudioSource Handler
     */

    public enum HandleSoundState
    {
        PLAY,
        STOP,
        PAUSE,
        UNPAUSE,
        RESTART
    }

    /*
     * clip:
     * - A value of null indicates that the update will be applied to the currently assigned clip.
     * - Note: Using null when source.clip is null will result in no effect.
     *      
     * state:
     * - PLAY:
     *   - If the clip is the same, start the clip or continue playing if it’s already playing.
     *   - If the clip is different, replace the clip and start the new clip.
     * - STOP: Stop the clip.
     * - PAUSE: Pause the clip if it’s currently playing.
     * - UNPAUSE: Resume playback if the clip was previously paused.
     * - RESTART: Restart the clip from the beginning.
     *  
     * loop:
     * - Can be updated in every case except STOP.
     * - 0: No update.
     * - 1: Loop = true.
     * - 2: Loop = false.
     */

    public static void HandleSound(AudioSource source, AudioClip clip, HandleSoundState state, int loop)
    {
        if (source == null)
        {
            Debug.LogWarning("Utilities.HandleSound: source is null!");
            return;
        }

        if(clip == null && source.clip == null)
        {
            return;
        }

        switch(state)
        {
            case HandleSoundState.RESTART: //Ignore the other settings and restart
                if(source.clip == null)
                    return;

                if(source.isPlaying)
                    source.Stop();

                source.volume = SoundManager.Instance.GetSoundEffectsVolume();
                source.Play();
            break;
            

            case HandleSoundState.PLAY:
                source.volume = SoundManager.Instance.GetSoundEffectsVolume();

                if (clip != null && !clip.Equals(source.clip)) //Clip changed
                {
                    if (source.isPlaying)
                        source.Stop();

                    source.clip = clip;

                    source.Play();
                    break;
                }

                if (!source.isPlaying)
                    source.Play();
            break;

            case HandleSoundState.UNPAUSE:
                if (source.time > 0f && !source.isPlaying) //== Was playing
                {
                    source.volume = SoundManager.Instance.GetSoundEffectsVolume();
                    source.UnPause();
                }
            break;

            case HandleSoundState.PAUSE:
                if (source.isPlaying)
                    source.Pause();
            break;

            case HandleSoundState.STOP:
                if (source.isPlaying)
                    source.Stop();
            return;
        }

        if (loop > 0)
        {
            if (loop == 1 && !source.loop )
                source.loop = true;
            else if(loop == 2 && source.loop)
                source.loop = false;
        }
    }

    /*
     * Particle System
     */

    public static void GenerateTempParticleSystem(ParticleSystem prefab, Vector3 position, Quaternion rotation)
    {
        ParticleSystem ps = GameObject.Instantiate<ParticleSystem>(prefab, position, rotation);

        if(ps != null)
        {
            ParticleSystem.MainModule main = ps.main; //struct-like object 
            main.stopAction = ParticleSystemStopAction.Destroy; //not require to do: ps.main = main
            ps.Play();
        }
    }
}
