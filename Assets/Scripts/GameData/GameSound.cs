/*
 * Struct that defines the sound settings.
 */
using UnityEngine;

namespace GameData
{
    [System.Serializable]
    public struct GameSound 
    {
        [SerializeField] private float masterSound;
        [SerializeField] private float soundEffects;
        [SerializeField] private float music;

        public GameSound(float masterSound, float soundEffects, float music)
        {
            this.masterSound = masterSound;
            this.soundEffects = soundEffects;
            this.music = music;
        }

        public float MasterSound { get { return masterSound; } set { masterSound = Mathf.Clamp01(value); } }
        public float SoundEffects { get {  return soundEffects; } set {  soundEffects = Mathf.Clamp01(value); } }
        public float Music { get { return music; } set { music = Mathf.Clamp01(value); } }

        public static GameSound GetDefaultSounds()
        {
            return new GameSound(1f,1f,0.5f);
        }
    }
}
