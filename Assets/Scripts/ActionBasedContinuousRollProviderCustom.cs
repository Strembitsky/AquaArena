using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to smoothly roll their rig continuously over time
    /// using a specified input action.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    /// <seealso cref="ActionBasedSnapTurnProvider"/>
    [AddComponentMenu("XR/Locomotion/Continuous Roll Provider (Action-based)", 11)]
   // [HelpURL(XRHelpURLConstants.k_ActionBasedContinuousTurnProvider)]
    public class ActionBasedContinuousRollProviderCustom : ContinuousRollProviderBaseCustom
    {
        [SerializeField]
        [Tooltip("The Input System Action that will be used to read Turn data from the left hand controller. Must be a Value Vector2 Control.")]
        InputActionProperty m_LeftHandRollAction;
        /// <summary>
        /// The Input System Action that Unity uses to read Turn data from the left hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
        /// </summary>
        public InputActionProperty leftHandRollAction
        {
            get => m_LeftHandRollAction;
            set => SetInputActionProperty(ref m_LeftHandRollAction, value);
        }

       
        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            m_LeftHandRollAction.EnableDirectAction();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_LeftHandRollAction.DisableDirectAction();
        }

        /// <inheritdoc />
        protected override Vector2 ReadInput()
        {
            var leftHandValue = m_LeftHandRollAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

            return leftHandValue;
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
