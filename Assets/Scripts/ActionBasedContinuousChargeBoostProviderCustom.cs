using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to charge a boost and release it for great speed.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    [AddComponentMenu("XR/Locomotion/Continuous Charge Boost Provider (Action-based)", 11)]
   // [HelpURL(XRHelpURLConstants.k_ActionBasedContinuousChargeBoostProvider)]
    public class ActionBasedContinuousChargeBoostProviderCustom : ContinuousChargeBoostProviderBaseCustom
    {
        public XRGrabVelocityTracked ballScript;
        [SerializeField]
        [Tooltip("The Input System Action that will be used to read Boost data from the left hand controller. Must be a Value Vector2 Control.")]
        InputActionProperty m_ChargeBoostAction;
        /// <summary>
        /// The Input System Action that Unity uses to read button press data from the left hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="float"/> Control.
        /// </summary>
        public InputActionProperty ChargeBoostAction
        {
            get => m_ChargeBoostAction;
            set => SetInputActionProperty(ref m_ChargeBoostAction, value);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            m_ChargeBoostAction.EnableDirectAction();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_ChargeBoostAction.DisableDirectAction();
        }

        /// <inheritdoc />
        protected override float ReadInput()
        {
            if (!ballScript.canChargeBoost)
                return 0f;
            var inputValue = m_ChargeBoostAction.action?.ReadValue<float>() ?? 0f;
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
