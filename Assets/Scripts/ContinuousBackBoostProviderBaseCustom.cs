using System;
using Unity.VisualScripting;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to boost in the direction of the camera.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    public abstract class ContinuousBackBoostProviderBaseCustom : LocomotionProvider
    {
        [SerializeField]
        [Tooltip("The number of meters/second towards wrist to boost when holding button.")]
        float m_BoostSpeed = 20f;
        AudioSource BackBoost;
        bool backBoosted;
        public Rigidbody xrRigidbody;
        public Vector3 currentVelocity;
        /// <summary>
        /// The number of meters/second towards camera look direction to boost when pressing button.
        /// </summary>
        public float boostSpeed
        {
            get => m_BoostSpeed;
            set => m_BoostSpeed = value;
        }
        
        bool m_IsMovingXROrigin;

        protected void Start()
        {
            BackBoost = GameObject.Find("BackBoostSound").GetComponent<AudioSource>();
            xrRigidbody = GameObject.Find("XR Origin").GetComponent<Rigidbody>();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            m_IsMovingXROrigin = false;

            // Check if the button is pressed
            var input = ReadInput();
            BackBoostRig(input);

            switch (locomotionPhase)
            {
                case LocomotionPhase.Idle:
                case LocomotionPhase.Started:
                    if (m_IsMovingXROrigin)
                        locomotionPhase = LocomotionPhase.Moving;
                    break;
                case LocomotionPhase.Moving:
                    if (!m_IsMovingXROrigin)
                        locomotionPhase = LocomotionPhase.Done;
                    break;
                case LocomotionPhase.Done:
                    locomotionPhase = m_IsMovingXROrigin ? LocomotionPhase.Moving : LocomotionPhase.Idle;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(LocomotionPhase)}={locomotionPhase}");
                    break;
            }
        }

        protected void FixedUpdate()
        {
            currentVelocity = xrRigidbody.velocity;
        }

        /// <summary>
        /// Reads the current value of the button input for boost..
        /// </summary>
        /// <returns>Returns the input float, such as from a button.</returns>
        protected abstract float ReadInput();

        /// <summary>
        /// Boosts the rig by <paramref name="boostSpeed"/>.
        /// </summary>
        /// <param name="boostSpeed">The amount of boost.</param>
        protected void BackBoostRig(float input)
        {
            if (Mathf.Approximately(input, 0f))
            {
                backBoosted = false;
                return;
            }

            if (CanBeginLocomotion() && BeginLocomotion())
            {
                var xrOrigin = system.xrOrigin;
                if (xrOrigin != null)
                {
                    if (!backBoosted)
                    {
                        m_IsMovingXROrigin = true;
                        BackBoost.Play();
                        if (currentVelocity.magnitude < 5f)
                            xrRigidbody.AddForce(((xrOrigin.Camera.transform.forward.normalized) * 75f), ForceMode.Force);

                        else if (currentVelocity.magnitude >= 5f)
                        {
                            xrRigidbody.AddForce((xrOrigin.Camera.transform.forward.normalized + -currentVelocity.normalized) * 75f, ForceMode.Force);
                        }
                        backBoosted = true;
                    }
                }

                EndLocomotion();
            }
        }
    }
}
