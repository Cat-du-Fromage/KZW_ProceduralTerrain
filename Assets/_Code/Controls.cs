// GENERATED AUTOMATICALLY FROM 'Assets/_Code/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""CameraControl"",
            ""id"": ""2d399b90-1008-4ec5-8fdd-6db0dac71498"",
            ""actions"": [
                {
                    ""name"": ""Mouvement"",
                    ""type"": ""Value"",
                    ""id"": ""2e5af36a-c53f-4b86-ac05-cc8e7ddc3f24"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Rotation"",
                    ""type"": ""Button"",
                    ""id"": ""a3b8fd41-2529-4c53-b34e-4779a5d95d24"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""Value"",
                    ""id"": ""4092d86b-2351-447c-892d-8fbfe31a9362"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Faster"",
                    ""type"": ""Button"",
                    ""id"": ""46c185b5-c186-4cbc-bdcb-40093f85076a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""04f80390-5f11-4a63-8e87-2c4da945caad"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouvement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""bdcd7983-a982-4aaa-a4bc-c77bf5aa3c7b"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Mouvement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""31dc9b97-5b5a-442a-837d-e59636b09516"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Mouvement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""0fa77926-4e69-4727-af80-46ae90474796"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Mouvement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ba3874d6-2fb2-47be-b042-e40dd9a60777"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Mouvement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""f769d32e-92a7-4506-a99f-283cb3adeb95"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""005f9dd5-27c7-49d4-8b1d-a764c8c6dedf"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": ""Clamp(min=-1,max=1),Invert"",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""eb785e16-0c88-423e-a9b1-5a30eac8dfb4"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Faster"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": true,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // CameraControl
        m_CameraControl = asset.FindActionMap("CameraControl", throwIfNotFound: true);
        m_CameraControl_Mouvement = m_CameraControl.FindAction("Mouvement", throwIfNotFound: true);
        m_CameraControl_Rotation = m_CameraControl.FindAction("Rotation", throwIfNotFound: true);
        m_CameraControl_Zoom = m_CameraControl.FindAction("Zoom", throwIfNotFound: true);
        m_CameraControl_Faster = m_CameraControl.FindAction("Faster", throwIfNotFound: true);
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

    // CameraControl
    private readonly InputActionMap m_CameraControl;
    private ICameraControlActions m_CameraControlActionsCallbackInterface;
    private readonly InputAction m_CameraControl_Mouvement;
    private readonly InputAction m_CameraControl_Rotation;
    private readonly InputAction m_CameraControl_Zoom;
    private readonly InputAction m_CameraControl_Faster;
    public struct CameraControlActions
    {
        private @Controls m_Wrapper;
        public CameraControlActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Mouvement => m_Wrapper.m_CameraControl_Mouvement;
        public InputAction @Rotation => m_Wrapper.m_CameraControl_Rotation;
        public InputAction @Zoom => m_Wrapper.m_CameraControl_Zoom;
        public InputAction @Faster => m_Wrapper.m_CameraControl_Faster;
        public InputActionMap Get() { return m_Wrapper.m_CameraControl; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CameraControlActions set) { return set.Get(); }
        public void SetCallbacks(ICameraControlActions instance)
        {
            if (m_Wrapper.m_CameraControlActionsCallbackInterface != null)
            {
                @Mouvement.started -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnMouvement;
                @Mouvement.performed -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnMouvement;
                @Mouvement.canceled -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnMouvement;
                @Rotation.started -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnRotation;
                @Rotation.performed -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnRotation;
                @Rotation.canceled -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnRotation;
                @Zoom.started -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnZoom;
                @Faster.started -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnFaster;
                @Faster.performed -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnFaster;
                @Faster.canceled -= m_Wrapper.m_CameraControlActionsCallbackInterface.OnFaster;
            }
            m_Wrapper.m_CameraControlActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Mouvement.started += instance.OnMouvement;
                @Mouvement.performed += instance.OnMouvement;
                @Mouvement.canceled += instance.OnMouvement;
                @Rotation.started += instance.OnRotation;
                @Rotation.performed += instance.OnRotation;
                @Rotation.canceled += instance.OnRotation;
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
                @Faster.started += instance.OnFaster;
                @Faster.performed += instance.OnFaster;
                @Faster.canceled += instance.OnFaster;
            }
        }
    }
    public CameraControlActions @CameraControl => new CameraControlActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface ICameraControlActions
    {
        void OnMouvement(InputAction.CallbackContext context);
        void OnRotation(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
        void OnFaster(InputAction.CallbackContext context);
    }
}
