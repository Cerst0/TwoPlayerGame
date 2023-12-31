//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/Other/InputManagerGamepad.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @InputManagerGamepad : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputManagerGamepad()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputManagerGamepad"",
    ""maps"": [
        {
            ""name"": ""PlayerBlue"",
            ""id"": ""a268a36b-7e0d-4eec-931c-f6f4ae69c6df"",
            ""actions"": [
                {
                    ""name"": ""MHB"",
                    ""type"": ""PassThrough"",
                    ""id"": ""5893eb4c-b03b-4103-99ee-4d63e8b238fd"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MVB"",
                    ""type"": ""PassThrough"",
                    ""id"": ""16732013-ba5e-46f7-a5a4-6a48afb747c3"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""42f291c4-18e7-49f4-a268-4b98f4910890"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MHB"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""e2dc23af-9b2a-4091-8ada-707b935a8ae1"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MHB"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""b9b83434-9db8-4b61-aa3e-ce389d5626d1"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MHB"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""44d5bb8e-59c9-4528-ae90-97873c892e53"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MVB"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""0b0c664c-04fc-4c77-bb40-ee73149e1741"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MVB"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""daab0157-96cd-47bc-9926-91b7387b583c"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MVB"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""PlayerRed"",
            ""id"": ""9d5e2b8d-39ce-417f-9471-c2e1f7eb45fa"",
            ""actions"": [
                {
                    ""name"": ""MHR"",
                    ""type"": ""PassThrough"",
                    ""id"": ""3866a945-eec8-47ba-a3e9-664153cfdbdd"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MVR"",
                    ""type"": ""PassThrough"",
                    ""id"": ""e7106a18-d2a9-4476-bd15-170fb5af8809"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""e514952c-ba7e-42f9-a4cb-288cbfe7b39b"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MHR"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""b829f891-0ca7-4894-9097-173c67a07581"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MHR"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""fd4561d1-babf-405d-9559-bd6fe6a0c637"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MHR"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""890dfabf-bb94-4f02-8eaa-b9280ab7997e"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MVR"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""094ab2a9-70bc-4a39-87b4-650c50198ee4"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MVR"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""d80e7729-babb-4c67-8aa3-36a9c282e32f"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MVR"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // PlayerBlue
        m_PlayerBlue = asset.FindActionMap("PlayerBlue", throwIfNotFound: true);
        m_PlayerBlue_MHB = m_PlayerBlue.FindAction("MHB", throwIfNotFound: true);
        m_PlayerBlue_MVB = m_PlayerBlue.FindAction("MVB", throwIfNotFound: true);
        // PlayerRed
        m_PlayerRed = asset.FindActionMap("PlayerRed", throwIfNotFound: true);
        m_PlayerRed_MHR = m_PlayerRed.FindAction("MHR", throwIfNotFound: true);
        m_PlayerRed_MVR = m_PlayerRed.FindAction("MVR", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // PlayerBlue
    private readonly InputActionMap m_PlayerBlue;
    private IPlayerBlueActions m_PlayerBlueActionsCallbackInterface;
    private readonly InputAction m_PlayerBlue_MHB;
    private readonly InputAction m_PlayerBlue_MVB;
    public struct PlayerBlueActions
    {
        private @InputManagerGamepad m_Wrapper;
        public PlayerBlueActions(@InputManagerGamepad wrapper) { m_Wrapper = wrapper; }
        public InputAction @MHB => m_Wrapper.m_PlayerBlue_MHB;
        public InputAction @MVB => m_Wrapper.m_PlayerBlue_MVB;
        public InputActionMap Get() { return m_Wrapper.m_PlayerBlue; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerBlueActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerBlueActions instance)
        {
            if (m_Wrapper.m_PlayerBlueActionsCallbackInterface != null)
            {
                @MHB.started -= m_Wrapper.m_PlayerBlueActionsCallbackInterface.OnMHB;
                @MHB.performed -= m_Wrapper.m_PlayerBlueActionsCallbackInterface.OnMHB;
                @MHB.canceled -= m_Wrapper.m_PlayerBlueActionsCallbackInterface.OnMHB;
                @MVB.started -= m_Wrapper.m_PlayerBlueActionsCallbackInterface.OnMVB;
                @MVB.performed -= m_Wrapper.m_PlayerBlueActionsCallbackInterface.OnMVB;
                @MVB.canceled -= m_Wrapper.m_PlayerBlueActionsCallbackInterface.OnMVB;
            }
            m_Wrapper.m_PlayerBlueActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MHB.started += instance.OnMHB;
                @MHB.performed += instance.OnMHB;
                @MHB.canceled += instance.OnMHB;
                @MVB.started += instance.OnMVB;
                @MVB.performed += instance.OnMVB;
                @MVB.canceled += instance.OnMVB;
            }
        }
    }
    public PlayerBlueActions @PlayerBlue => new PlayerBlueActions(this);

    // PlayerRed
    private readonly InputActionMap m_PlayerRed;
    private IPlayerRedActions m_PlayerRedActionsCallbackInterface;
    private readonly InputAction m_PlayerRed_MHR;
    private readonly InputAction m_PlayerRed_MVR;
    public struct PlayerRedActions
    {
        private @InputManagerGamepad m_Wrapper;
        public PlayerRedActions(@InputManagerGamepad wrapper) { m_Wrapper = wrapper; }
        public InputAction @MHR => m_Wrapper.m_PlayerRed_MHR;
        public InputAction @MVR => m_Wrapper.m_PlayerRed_MVR;
        public InputActionMap Get() { return m_Wrapper.m_PlayerRed; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerRedActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerRedActions instance)
        {
            if (m_Wrapper.m_PlayerRedActionsCallbackInterface != null)
            {
                @MHR.started -= m_Wrapper.m_PlayerRedActionsCallbackInterface.OnMHR;
                @MHR.performed -= m_Wrapper.m_PlayerRedActionsCallbackInterface.OnMHR;
                @MHR.canceled -= m_Wrapper.m_PlayerRedActionsCallbackInterface.OnMHR;
                @MVR.started -= m_Wrapper.m_PlayerRedActionsCallbackInterface.OnMVR;
                @MVR.performed -= m_Wrapper.m_PlayerRedActionsCallbackInterface.OnMVR;
                @MVR.canceled -= m_Wrapper.m_PlayerRedActionsCallbackInterface.OnMVR;
            }
            m_Wrapper.m_PlayerRedActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MHR.started += instance.OnMHR;
                @MHR.performed += instance.OnMHR;
                @MHR.canceled += instance.OnMHR;
                @MVR.started += instance.OnMVR;
                @MVR.performed += instance.OnMVR;
                @MVR.canceled += instance.OnMVR;
            }
        }
    }
    public PlayerRedActions @PlayerRed => new PlayerRedActions(this);
    public interface IPlayerBlueActions
    {
        void OnMHB(InputAction.CallbackContext context);
        void OnMVB(InputAction.CallbackContext context);
    }
    public interface IPlayerRedActions
    {
        void OnMHR(InputAction.CallbackContext context);
        void OnMVR(InputAction.CallbackContext context);
    }
}
