/*
 * MonoBehaviour used by the LevelManager to save or load the current state of a Gameplay scene.
 * For more information, see IState and IStateSaveable interfaces (located in Assets/Scripts/Interfaces/SaveState).
 */
using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    public class LevelState : MonoBehaviour
    {
        /*
         * To manage the state of a GameObject, we need to interact with the script implementing the interface
         * IStateSaveable. Since the script may vary, we use polymorphism and save the list of IStateSaveable 
         * directly.
         * However, Unity does not support serialization of interfaces from the Inspector.
         * To populate the list of IStateSaveable, we save the GameObject references containing IStateSaveable 
         * in gameObjects.
         * The method Init extracts the list of IStateSaveable from gameObjects and saves it in saveables.
         */

        public List<GameObject> gameObjects = null;
        private List<IStateSaveable> saveables = null;

        private void Start()
        {
            if(!IsInitialized())
                Init();
        }

        public void SaveState()
        {
            if (!IsInitialized())
            {
                if (!Init()) //If not initializable
                    return;
            }

            List<IState> states = new List<IState>();

            foreach (IStateSaveable state in saveables)
            {
                states.Add(state.SaveState());
            }

            GameManager.Instance.SaveState(states);
        }

        public void LoadState()
        {
            if (!IsInitialized())
            {
                if (!Init()) //If not initializable
                    return;
            }

            List<IState> states;

            if ((states = GameManager.Instance.LoadState()) != null)
            {
                for (int i = 0; i < saveables.Count; i++)
                {
                    saveables[i].LoadState(states[i]);
                }
            }
        }

        private bool IsInitialized()
        {
            return saveables != null && saveables.Count > 0;
        }

        private bool Init() //false = Not initializable | true = initialized
        {
            if (IsInitialized())
                return true;

            if (gameObjects != null)
            {
                saveables = new List<IStateSaveable>();
                foreach (GameObject go in gameObjects)
                {
                    IStateSaveable state = go.GetComponentInChildren<IStateSaveable>(true);
                    if (state != null)
                        saveables.Add(state);
                }

                return saveables.Count > 0;
            }
            else
                return false;

        }
    }
}
