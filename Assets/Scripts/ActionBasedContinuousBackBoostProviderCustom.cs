using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to smoothly rotate their rig continuously over time
    /// using a specified input action.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    /// <seealso cref="ActionBasedSnapTurnProvider"/>
    [AddComponentMenu("XR/Locomotion/Continuous Back Boost Provider (Action-based)", 11)]
   // [HelpURL(XRHelpURLConstants.k_ActionBasedContinuousTurnProvider)]
    public class ActionBasedContinuousBackBoostProviderCustom : ContinuousBackBoostProviderBaseCustom
    {
        public XRGrabVelocityTracked ballScript;
        [SerializeField]
        [Tooltip("The Input System Action that will be used to read Boost data from the left hand controller. Must be a Value Vector2 Control.")]
        InputActionProperty m_BackBoostAction;
        /// <summary>
        /// The Input System Action that Unity uses to read Turn data from the left hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
        /// </summary>
        public InputActionProperty BackBoostAction
        {
            get => m_BackBoostAction;
            set => SetInputActionProperty(ref m_BackBoostAction, value);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            m_BackBoostAction.EnableDirectAction();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_BackBoostAction.DisableDirectAction();
        }

        /// <inheritdoc />
        protected override float ReadInput()
        {
            if (!ballScript.canBackBoost)
                return 0f;
            var inputValue = m_BackBoostAction.action?.ReadValue<float>() ?? 0f;
            return inputValue;
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
