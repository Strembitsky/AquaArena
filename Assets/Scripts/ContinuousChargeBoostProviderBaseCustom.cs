using System;
using Unity.VisualScripting;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to charge a boost and release to gain speed.
    /// </summary>
    public abstract class ContinuousChargeBoostProviderBaseCustom : LocomotionProvider
    {
        AudioSource ChargeBoostCharge, ChargeBoostRelease;
        [SerializeField]
        [Tooltip("The number of meters/second towards wrist to boost when releasing trigger.")]
        float m_BoostSpeed = 12.5f;

        /// <summary>
        /// The number of meters/second towards wrist to boost when holding button.
        /// </summary>
        public float boostSpeed
        {
            get => m_BoostSpeed;
            set => m_BoostSpeed = value;
        }

        GameObject chargeCylinder;
        bool m_IsMovingXROrigin;
        float chargeValue;

        protected void Start()
        {
            chargeValue = 0f;
            chargeCylinder = GameObject.Find("ChargeCylinder");
            ChargeBoostCharge = GameObject.Find("ChargeBoostCharge").GetComponent<AudioSource>();
            ChargeBoostRelease = GameObject.Find("ChargeBoostRelease").GetComponent<AudioSource>();
        }
        protected void Update()
        {
            m_IsMovingXROrigin = false;

            // Use the input amount to scale the turn speed.
            var input = ReadInput();
            var boostDirection = GetBoostDirection(input);
            ChargeBoostRig(input, boostDirection);

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

        /// <summary>
        /// Reads the current value of the trigger input.
        /// </summary>
        /// <returns>Returns the input vector, such as from a trigger.</returns>
        protected abstract float ReadInput();

        protected virtual Vector3 GetBoostDirection(float rightInput)
        {
                GameObject rightController = GameObject.Find("RightHand");
                XRInteractorLineVisual rightLine = rightController.GetComponentInChildren<XRInteractorLineVisual>();
                Vector3 rightDirection = rightLine.transform.forward;
                return (rightDirection);
        }

        /// <summary>
        /// Charges a boost, and upon reaching full charge, applies a force of <paramref name="boostSpeed"/>.
        /// </summary>
        /// <param name="boostSpeed">The amount to boost the player.</param>
        protected void ChargeBoostRig(float input, Vector3 boostDirection)
        {
            if (Mathf.Approximately(input, 0f))
            {
                ChargeBoostCharge.Stop();
                // if fully charged
                if (chargeValue >= 100f)
                {
                    if (CanBeginLocomotion() && BeginLocomotion())
                    {
                        var xrOrigin = system.xrOrigin;
                        if (xrOrigin != null)
                        {
                            m_IsMovingXROrigin = true;
                            Rigidbody xrRigidbody = GetComponent<Rigidbody>();

                            // go vroom
                            Vector3 currentVelocity = xrRigidbody.velocity;
                            ChargeBoostRelease.Play();
                            if (currentVelocity.magnitude < 12.5f)
                                xrRigidbody.AddForce((boostDirection.normalized * boostSpeed), ForceMode.Impulse);
                            else if (currentVelocity.magnitude >= 12.5f)
                            {
                                xrRigidbody.AddForce((boostDirection.normalized + -currentVelocity.normalized) * boostSpeed, ForceMode.Impulse);
                            }
                            // reset charge and visual charge
                            chargeValue = 0f;
                            chargeCylinder.transform.localScale = new Vector3(0.5f, 3.6f, 0.5f);
                            chargeCylinder.GetComponent<Renderer>().material.color = new Color(255, 105, 180);
                        }

                        EndLocomotion();
                    }
                }
                else
                {
                    // if not holding trigger and not fully charged, reset charge and charge visual
                    chargeValue = 0f;
                    chargeCylinder.transform.localScale = new Vector3(0.5f, 3.6f, 0.5f);
                    return;
                }
            }
            else
            {
                if (!ChargeBoostCharge.isPlaying)
                {
                    ChargeBoostCharge.Play();
                }
            }
            // if trigger held, and charge value is not full, increase charge value and scale up charge visual
            if (chargeValue < 100f)
            {
                chargeValue += 1f;
                chargeCylinder.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
            }
            // if trigger held, and charge value is full, change color of the charge visual
            else
            {
                chargeCylinder.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
            }
            return;
        }
    }
}
