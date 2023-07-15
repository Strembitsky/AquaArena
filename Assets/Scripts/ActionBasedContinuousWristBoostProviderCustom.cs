using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to gain speed in the direction of their wrists using a button.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    [AddComponentMenu("XR/Locomotion/Continuous Wrist Boost Provider (Action-based)", 11)]
   // [HelpURL(XRHelpURLConstants.k_ActionBasedContinuousTurnProvider)]
    public class ActionBasedContinuousWristBoostProviderCustom : ContinuousWristBoostProviderBaseCustom
    {
        [SerializeField]
        [Tooltip("The Input System Action that will be used to read Boost data from the left hand controller. Must be a Value Vector2 Control.")]
        InputActionProperty m_LeftHandWristBoostAction;

        [SerializeField]
        [Tooltip("The Input System Action that will be used to read Boost data from the right hand controller. Must be a Value Vector2 Control.")]
        InputActionProperty m_RightHandWristBoostAction;
        /// <summary>
        /// The Input System Action that Unity uses to read Turn data from the left hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
        /// </summary>
        public InputActionProperty leftHandWristBoostAction
        {
            get => m_LeftHandWristBoostAction;
            set => SetInputActionProperty(ref m_LeftHandWristBoostAction, value);
        }

        public InputActionProperty rightHandWristBoostAction
        {
            get => m_RightHandWristBoostAction;
            set => SetInputActionProperty(ref m_RightHandWristBoostAction, value);
        }


        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            m_LeftHandWristBoostAction.EnableDirectAction();
            m_RightHandWristBoostAction.EnableDirectAction();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_LeftHandWristBoostAction.DisableDirectAction();
            m_RightHandWristBoostAction.DisableDirectAction();
        }

        /// <inheritdoc />
        protected override float ReadLeftInput()
        {
            var leftHandValue = m_LeftHandWristBoostAction.action?.ReadValue<float>() ?? 0f;
            return leftHandValue;
        }
        protected override float ReadRightInput()
        {
            var rightHandValue = m_RightHandWristBoostAction.action?.ReadValue<float>() ?? 0f;
            return rightHandValue;
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }
    }
}
