/*
 *  Static class that acts as an interface between the InputSystem package and other scripts.
 *  Provides methods to interact with and manage ActionMaps, Actions, Bindings, and Devices.
 *  Note: The operations are performed on the InputActionAsset provided by the GameManager.
 */

using System;
using UnityEngine.InputSystem;

namespace GameData
{
    public static class GameInput
    {
        //Action maps
        public const string AM_PLAYER = "Player";   //Action map for gameplay input
        public const string AM_UI = "UI";           //Action map for UI input
        private static bool amPlayerEnabled = false;//To know which action map is enable
        private static bool amUIEnabled = false;    //To know which action map is enable

        //Actions
        public const string A_MOVE = "Move";        //Gameplay & UI
        public const string A_JUMP = "Jump";        //Gameplay
        public const string A_USE1 = "Use1";        //Gameplay 
        public const string A_USE2 = "Use2";        //Gameplay
        public const string A_USE3 = "Use3";        //Gameplay
        public const string A_PAUSE = "Pause";      //Gameplay & UI
        public const string A_SUBMIT = "Submit";    //UI    

        //Control schemes
        public const string CS_KB = "Keyboard";     //Control scheme for Keyboard
        public const string CS_GAMEPAD = "Gamepad"; //Control scheme for Keyboard

        //Bindings to exclude
        public const string EC_GP_LS_X = "<Gamepad>/leftStick/x";
        public const string EC_GP_LS_Y = "<Gamepad>/leftStick/y";
        public const string EC_GP_RS_X = "<Gamepad>/rightStick/x";
        public const string EC_GP_RS_Y = "<Gamepad>/rightStick/y";

        public static readonly string[] EXCLUDE_CONTROLS = 
        { 
            EC_GP_LS_X,
            EC_GP_LS_Y,
            EC_GP_RS_X,
            EC_GP_RS_Y
        };

        /*
         * These controls need to be excluded because they cause problems when attempting to rebind left/right 
         * stick bindings.
         * The rebinding system does not detect the up, down, left, and right binds, but instead detects X and Y.
         * Note: The rebinding system likely needs to be notified that a composite is being rebound. 
         * However, this forces us to rebind every bind of the composite, while we are only modifying a single 
         * bind within the composite. As a result, the system detects X and Y and excludes the 
         * left/right/up/down controls.
         */

        //Keys for cancel rebinding
        private const string CANCEL_REBIND_KB = "<Keyboard>/backSpace";
        private const string CANCEL_REBIND_GAMEPAD = "<Gamepad>/select";

        public enum DeviceType //Device supported
        {
            Keyboard,
            Gamepad
        }

        public enum BindingType
        {
            FORWARD = 0,
            BACKWARD = 1,
            LEFT = 2,
            RIGHT = 3,
            PAUSE = 4,
            JUMP = 5,
            USE1 = 6,
            USE2 = 7,
            USE3 = 8,
            SUBMIT = 9
        }

        public static string GetControlScheme(DeviceType device)
        {
            if (device == DeviceType.Keyboard)
                return CS_KB;
            else
                return CS_GAMEPAD;
        }

        public static string GetCancelRebindKey(DeviceType device)
        {
            if (device == DeviceType.Keyboard)
                return CANCEL_REBIND_KB;
            else
                return CANCEL_REBIND_GAMEPAD;
        }

        //InputActionAsset
        private static InputActionAsset actionAsset = null;

        private static InputActionAsset GetActionAsset()
        {
            if (actionAsset == null) //Avoid querying the GameManager repeatedly.
                actionAsset = GameManager.Instance.LoadInputSettings(false);

            return actionAsset;
        }

        //Functionality
        public static void EnableGameplayActionMap()
        {
            GetActionAsset().FindActionMap(AM_PLAYER).Enable();
            amPlayerEnabled = true;
        }

        public static void DisableGameplayActionMap()
        {
            GetActionAsset().FindActionMap(AM_PLAYER).Disable();
            amPlayerEnabled = false;
        }

        public static void EnableUIActionMap()
        {
            GetActionAsset().FindActionMap(AM_UI).Enable();
            amUIEnabled = true;
        }

        public static void DisableUIActionMap()
        {
            GetActionAsset().FindActionMap(AM_UI).Disable();
            amUIEnabled = false;
        }

        public static bool IsKeyboardConnected()
        {
            if (Keyboard.current == null)
                return false;

            return true;
        }

        public static bool IsGamepadConnected()
        {
            if(Gamepad.all.Count > 0)
            {
                return true;
            }

            return false;
        }

        public static string GetHumanReadableString(InputBinding binding) //Overload method
        {
            //effectivePath: The path currently in use (most probably the override path)
            return GetHumanReadableString(binding.effectivePath);
        }

        public static string GetHumanReadableString(string bindingPath)
        {
            return InputControlPath.ToHumanReadableString(
                bindingPath,
                InputControlPath.HumanReadableStringOptions.UseShortNames |
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        }

        public static bool CheckDuplicate(InputAction action, int bindingIndex, DeviceType device)
        {
            InputBinding newBinding = action.bindings[bindingIndex];

            //Action map Player
            foreach (InputBinding binding in GetActionAsset().FindActionMap(AM_PLAYER, false).bindings)
            {
                if (binding.id.Equals(newBinding.id)) //Ignore himself
                    continue;

                if (binding.effectivePath.Equals(newBinding.effectivePath))
                    return true;
            }

            //Action map UI
            //Submit UI
            InputBinding uiBinding = GetSubmitUIBinding(device);
            if (!uiBinding.id.Equals(newBinding.id) && uiBinding.effectivePath.Equals(newBinding.effectivePath))
                return true;

            //The other bindings for this action map have the same bindings as their equivalents in the 
            //Player action map, making the check redundant.

            return false;
        }

        //Actions
        public static InputAction GetMoveAction()
        {
            return GetActionAsset()[AM_PLAYER + "/" + A_MOVE];
        }

        public static InputAction GetMoveUIAction()
        {
            return GetActionAsset()[AM_UI + "/" + A_MOVE];
        }

        public static InputAction GetJumpAction()
        {
            return GetActionAsset()[AM_PLAYER + "/" + A_JUMP];
        }

        public static InputAction GetUse1Action()
        {
            return GetActionAsset()[AM_PLAYER + "/" + A_USE1];
        }

        public static InputAction GetUse2Action()
        {
            return GetActionAsset()[AM_PLAYER + "/" + A_USE2];
        }

        public static InputAction GetUse3Action()
        {
            return GetActionAsset()[AM_PLAYER + "/" + A_USE3];
        }

        public static InputAction GetPauseAction()
        {
            return GetActionAsset()[AM_PLAYER + "/" + A_PAUSE];
        }

        public static InputAction GetPauseUIAction()
        {
            return GetActionAsset()[AM_UI + "/" + A_PAUSE];
        }

        public static InputAction GetSubmitUIAction()
        {
            return GetActionAsset()[AM_UI + "/" + A_SUBMIT];
        }

        //Bindings
        //Move & MoveUI
        /*
         * For the "Move" action, we have 4 bindings, and these bindings are part of a composite.
         * Depending on how we search for the index, we will need to add a different number to the binding index obtained.
         * Generally, this composite follows the structure:
         * Name         Index N
         * Binding 1    Index N+1 [Pointed to by the filter]
         * Binding 2    Index N+2
         * Binding 3    Index N+3
         * Binding 4    Index N+4
         * When searching by filtering with the group (called control scheme), the result points to the index of the 
         * first binding. This means GetBindingIndex returns Index N+1, requiring us to add 0, 1, 2, or 3.
         * If we use other methods to get the bindings (e.g., searching by the name of the composite), we obtain another 
         * index —for example, Index N— requiring us to add 1, 2, 3, or 4.
         */

        public static InputBinding GetForwardBinding(DeviceType device)                 //Gameplay
        {
            InputAction action = GetMoveAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            return action.bindings[bindingIndex];
        }

        public static InputBinding GetForwardUIBinding(DeviceType device)               //UI
        {
            InputAction action = GetMoveUIAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            return action.bindings[bindingIndex];
        }

        public static int GetForwardBindingRelativeIndex(bool relativeToScheme)         //Gameplay & UI
        {
            if (relativeToScheme)
                return 0;
            else
                return 1;
        }

        public static InputBinding GetBackwardBinding(DeviceType device)                //Gameplay
        {
            InputAction action = GetMoveAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            return action.bindings[bindingIndex + 1];
        }

        public static InputBinding GetBackwardUIBinding(DeviceType device)              //UI
        {
            InputAction action = GetMoveUIAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            return action.bindings[bindingIndex + 1];
        }

        public static int GetBackwardBindingRelativeIndex(bool relativeToScheme)        //Gameplay & UI
        {
            if (relativeToScheme)
                return 1;
            else
                return 2;
        }

        public static InputBinding GetLeftBinding(DeviceType device)                    //Gameplay
        {
            InputAction action = GetMoveAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            return action.bindings[bindingIndex + 2];
        }

        public static InputBinding GetLeftUIBinding(DeviceType device)                  //UI
        {
            InputAction action = GetMoveAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            return action.bindings[bindingIndex + 2];
        }

        public static int GetLeftBindingRelativeIndex(bool relativeToScheme)            //Gameplay & UI
        {
            if (relativeToScheme)
                return 2;
            else
                return 3;
        }

        public static InputBinding GetRightBinding(DeviceType device)                   //Gameplay
        {
            InputAction action = GetMoveAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            return action.bindings[bindingIndex + 3];
        }

        public static InputBinding GetRightUIBinding(DeviceType device)                 //UI
        {
            InputAction action = GetMoveUIAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            return action.bindings[bindingIndex + 3];
        }

        public static int GetRightBindingRelativeIndex(bool relativeToScheme)           //Gameplay & UI
        {
            if (relativeToScheme)
                return 3;
            else
                return 4;
        }

        //Pause & PauseUI
        public static InputBinding GetPauseBinding(DeviceType device)                   //Gameplay
        {
            InputAction action = GetPauseAction();
            return action.bindings[action.GetBindingIndex(group: GetControlScheme(device))];
        }

        public static InputBinding GetPauseUIBinding(DeviceType device)                 //UI
        {
            InputAction action = GetPauseUIAction();
            return action.bindings[action.GetBindingIndex(group: GetControlScheme(device))];
        }

        //Jump
        public static InputBinding GetJumpBinding(DeviceType device)                    //Gameplay
        {
            InputAction action = GetJumpAction();
            return action.bindings[action.GetBindingIndex(group: GetControlScheme(device))];
        }

        //Use1-2-3
        public static InputBinding GetUse1Binding(DeviceType device)                    //Gameplay
        {
            InputAction action = GetUse1Action();
            return action.bindings[action.GetBindingIndex(group: GetControlScheme(device))];
        }

        public static InputBinding GetUse2Binding(DeviceType device)                    //Gameplay
        {
            InputAction action = GetUse2Action();
            return action.bindings[action.GetBindingIndex(group: GetControlScheme(device))];
        }

        public static InputBinding GetUse3Binding(DeviceType device)                    //Gameplay
        {
            InputAction action = GetUse3Action();
            return action.bindings[action.GetBindingIndex(group: GetControlScheme(device))];
        }

        //SubmitUI
        public static InputBinding GetSubmitUIBinding(DeviceType device)                //UI
        {
            InputAction action = GetSubmitUIAction();
            return action.bindings[action.GetBindingIndex(group: GetControlScheme(device))];
        }

        /*
         *  The InputActionAsset contains the default bindings configured in the editor.
         *  All changes are saved as overrides in another location (PlayerPrefs) and applied to the asset.
         *  To reset the bindings, the overrides must be removed.
         */
        public static void ResetBindings(DeviceType device)
        {
            GetActionAsset().Disable();

            //Move (Gameplay)
            InputAction action = GetMoveAction();
            int bindingIndex = action.GetBindingIndex(group: GetControlScheme(device));
            action.RemoveBindingOverride(bindingIndex);         //Forward
            action.RemoveBindingOverride(bindingIndex + 1);     //Backward
            action.RemoveBindingOverride(bindingIndex + 2);     //Left
            action.RemoveBindingOverride(bindingIndex + 3);     //Right

            //Move (UI)
            action = GetMoveUIAction();
            action.GetBindingIndex(group: GetControlScheme(device));
            action.RemoveBindingOverride(bindingIndex);         //Forward
            action.RemoveBindingOverride(bindingIndex + 1);     //Backward
            action.RemoveBindingOverride(bindingIndex + 2);     //Left
            action.RemoveBindingOverride(bindingIndex + 3);     //Right

            //Pause (Gameplay)
            action = GetPauseAction();
            action.RemoveBindingOverride(action.GetBindingIndex(group: GetControlScheme(device)));

            //Pause (UI)
            action = GetPauseUIAction();
            action.RemoveBindingOverride(action.GetBindingIndex(group: GetControlScheme(device)));

            //Jump (Gameplay)
            action = GetJumpAction();
            action.RemoveBindingOverride(action.GetBindingIndex(group: GetControlScheme(device)));

            //Use1 (Gameplay)
            action = GetUse1Action();
            action.RemoveBindingOverride(action.GetBindingIndex(group: GetControlScheme(device)));

            //Use2 (Gameplay)
            action = GetUse2Action();
            action.RemoveBindingOverride(action.GetBindingIndex(group: GetControlScheme(device)));

            //Use3 (Gameplay)
            action = GetUse3Action();
            action.RemoveBindingOverride(action.GetBindingIndex(group: GetControlScheme(device)));

            //Submit (UI)
            action = GetSubmitUIAction();
            action.RemoveBindingOverride(action.GetBindingIndex(group: GetControlScheme(device)));

            //Enable only the actually active action map
            if (amPlayerEnabled)
                EnableGameplayActionMap();

            if (amUIEnabled)
                EnableUIActionMap();
        }

        //Rebindings
        //RebindingOperation
        private static InputActionRebindingExtensions.RebindingOperation rebindOperation;
        private static InputAction action;
        private static int bindingIndex;
        private static string oldBindingPath;
        private static DeviceType deviceType;
        private static bool hasBoth;
        private static string name;

        private static void Dispose() //To ensure there is no pending RebindingOperation
        {
            if (rebindOperation != null)
            {
                rebindOperation.Dispose();
                rebindOperation = null;
            }
        }

        //onComplete(bool foundDuplicate, string duplicateBinding, string effectiveBinding)
        private static void Rebind(
            InputAction actionToRebind, int addToIndex, DeviceType device, bool hasBothMap, string actionName = null,
            Action<bool, string, string> onComplete = null, Action onCancel = null
            )
        {
            Dispose();

            action = actionToRebind;
            hasBoth = hasBothMap;
            deviceType = device;
            name = actionName;

            //Disable action to rebind
            action.Disable();

            if (hasBoth)
            {
                switch (name)
                {
                    case A_MOVE:
                        GetMoveUIAction().Disable();
                        break;

                    case A_PAUSE:
                        GetPauseUIAction().Disable();
                        break;
                }
            }

            bindingIndex = action.GetBindingIndex(group: GetControlScheme(device)) + addToIndex;

            //Get the bindings path that is currently being used.
            oldBindingPath = action.bindings[bindingIndex].effectivePath;

            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithBindingGroup(GetControlScheme(device))
                .WithCancelingThrough(GetCancelRebindKey(device))
                .OnComplete(operation => OnComplete(onComplete, onCancel))
                .OnCancel(operation =>
                {
                    OnCancel();
                    if (onCancel != null)
                        onCancel();
                }
                );

            for (int i = 0; i < EXCLUDE_CONTROLS.Length; i++)
            {
                rebindOperation.WithControlsExcluding(EXCLUDE_CONTROLS[i]);
            }

            rebindOperation.Start();
        }

        private static void EnableSingleAction()
        {
            // Note: The action, along with its corresponding UI action, is disabled by the Rebind function.
            if (action.actionMap.name.Equals(AM_PLAYER))
            {
                if (amPlayerEnabled)
                    action.Enable();

                if(hasBoth && amUIEnabled) //hasBoth: Indicates that the action has a corresponding UI action.
                { 
                    switch (name)
                    {
                        case A_MOVE:
                            GetMoveUIAction().Enable();
                            break;

                        case A_PAUSE:
                            GetPauseUIAction().Enable();
                            break;
                    }
                }
            }
            else if(action.actionMap.name.Equals(AM_UI) && amUIEnabled)
            {
                action.Enable();
            }
        }

        private static void OnCancel()
        {
            Dispose();
            EnableSingleAction();
        }

        private static void OnComplete(Action<bool, string, string> onComplete = null, Action onCancel = null)
        {
            Dispose();

            //If it is inserting the same key
            if (action.bindings[bindingIndex].effectivePath.Equals(oldBindingPath)) 
            {
                EnableSingleAction();
                //Cancel the operation
                onCancel?.Invoke();
                return;
            }

            //If the new binding is a duplicate
            if (GameInput.CheckDuplicate(action, bindingIndex, deviceType)) 
            {
                //Retrieve the readable name of the new binding path (duplicate)
                string duplicatePath = GameInput.GetHumanReadableString(action.bindings[bindingIndex]);

                //Rewrite the old binding path
                action.ApplyBindingOverride(bindingIndex, oldBindingPath);
                EnableSingleAction();

                if (onComplete != null) //onComplete(bool foundDuplicate, string duplicateBinding, string effectiveBinding)
                    onComplete(true, duplicatePath, GameInput.GetHumanReadableString(oldBindingPath));

                return;
            }

            if (hasBoth)
            {
                switch (name)
                {
                    case A_MOVE:
                        GetMoveUIAction().ApplyBindingOverride(bindingIndex, action.bindings[bindingIndex]);
                        break;

                    case A_PAUSE:
                        GetPauseUIAction().ApplyBindingOverride(bindingIndex, action.bindings[bindingIndex]);
                        break;
                }
            }

            EnableSingleAction();

            if (onComplete != null) //onComplete(bool foundDuplicate, string duplicateBinding, string effectiveBinding)
                onComplete(false, null, GetHumanReadableString(action.bindings[bindingIndex]));
        }

        public static void CancelRebind()
        {
            rebindOperation.Cancel();
        }

        //Rebindings: Move
        public static void RebindMoveForward(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetMoveAction(), 0, deviceType, true, A_MOVE, onComplete, onCancel);
        }

        public static void RebindMoveBackward(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetMoveAction(), 1, deviceType, true, A_MOVE, onComplete, onCancel);
        }

        public static void RebindMoveLeft(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetMoveAction(), 2, deviceType, true, A_MOVE, onComplete, onCancel);
        }

        public static void RebindMoveRight(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetMoveAction(), 3, deviceType, true, A_MOVE, onComplete, onCancel);
        }

        //Rebindings: Pause
        public static void RebindPause(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetPauseAction(), 0, deviceType, true, A_PAUSE, onComplete, onCancel);
        }

        //Rebindings: Jump
        public static void RebindJump(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetJumpAction(), 0, deviceType, false, null, onComplete, onCancel);
        }

        //Rebindings: Use1-2-3
        public static void RebindUse1(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetUse1Action(), 0, deviceType, false, null, onComplete, onCancel);
        }

        public static void RebindUse2(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetUse2Action(), 0, deviceType, false, null, onComplete, onCancel);
        }

        public static void RebindUse3(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetUse3Action(), 0, deviceType, false, null, onComplete, onCancel);
        }

        //Rebindings: SubmitUI
        public static void RebindSubmitUI(DeviceType deviceType, Action<bool, string, string> onComplete, Action onCancel)
        {
            Rebind(GetSubmitUIAction(), 0, deviceType, false, null, onComplete, onCancel);
        }

        /*
         *  Output
         */

        //Move
        public static string PrintForwardBinding()
        {
            if(IsGamepadConnected())
                return GetHumanReadableString(GetForwardBinding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetForwardBinding(DeviceType.Keyboard));
        }

        public static string PrintBackwardBinding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetBackwardBinding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetBackwardBinding(DeviceType.Keyboard));
        }

        public static string PrintLeftBinding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetLeftBinding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetLeftBinding(DeviceType.Keyboard));
        }

        public static string PrintRightBinding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetRightBinding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetRightBinding(DeviceType.Keyboard));
        }

        //Jump
        public static string PrintJumpBinding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetJumpBinding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetJumpBinding(DeviceType.Keyboard));
        }

        //Pause
        public static string PrintPauseBinding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetPauseBinding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetPauseBinding(DeviceType.Keyboard));
        }

        //Use
        public static string PrintUse1Binding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetUse1Binding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetUse2Binding(DeviceType.Keyboard));
        }

        public static string PrintUse2Binding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetUse1Binding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetUse2Binding(DeviceType.Keyboard));
        }

        public static string PrintUse3Binding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetUse1Binding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetUse2Binding(DeviceType.Keyboard));
        }

        //Submit
        public static string PrintSubmitUIBinding()
        {
            if (IsGamepadConnected())
                return GetHumanReadableString(GetSubmitUIBinding(DeviceType.Gamepad));
            else
                return GetHumanReadableString(GetSubmitUIBinding(DeviceType.Keyboard));
        }

        public static string PrintBinding(BindingType bindingType)
        {
            DeviceType deviceType;

            if(IsGamepadConnected())
                deviceType = DeviceType.Gamepad;
            else
                deviceType = DeviceType.Keyboard;

            InputBinding binding;

            switch(bindingType)
            {
                case BindingType.FORWARD:
                    binding = GetForwardBinding(deviceType);
                break;

                case BindingType.BACKWARD:
                    binding = GetBackwardBinding(deviceType);
                    break;

                case BindingType.LEFT:
                    binding = GetLeftBinding(deviceType);
                    break;

                case BindingType.RIGHT:
                    binding = GetRightBinding(deviceType);
                    break;

                case BindingType.JUMP:
                    binding = GetJumpBinding(deviceType);
                    break;

                case BindingType.PAUSE:
                    binding = GetPauseBinding(deviceType);
                    break;
                case BindingType.USE1:
                    binding = GetUse1Binding(deviceType);
                    break;

                case BindingType.USE2:
                    binding = GetUse2Binding(deviceType);
                    break;

                case BindingType.USE3:
                    binding = GetUse3Binding(deviceType);
                    break;

                case BindingType.SUBMIT:
                    binding = GetSubmitUIBinding(deviceType);
                    break;

                default:
                    return "";
            }

            return GetHumanReadableString(binding);
        }
    }
}
