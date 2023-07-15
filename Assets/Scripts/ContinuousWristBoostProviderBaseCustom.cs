using System;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to boost the rig in the direction of their wrists using a button.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    public abstract class ContinuousWristBoostProviderBaseCustom : LocomotionProvider
    {
        AudioSource wristThrustLeft, wristThrustRight;
        GameObject leftController, rightController;
        XRInteractorLineVisual leftLine, rightLine;
        [SerializeField]
        [Tooltip("The number of meters/second towards wrist to boost when holding button.")]
        float m_BoostSpeed = 5f;
        /// <summary>
        /// The number of meters/second towards wrist to boost when holding button.
        /// </summary>
        public float boostSpeed
        {
            get => m_BoostSpeed;
            set => m_BoostSpeed = value;
        }
        
        bool m_IsMovingXROrigin;

        protected void Start()
        {
            wristThrustLeft = GameObject.Find("WristThrustLeft").GetComponent<AudioSource>();
            wristThrustRight = GameObject.Find("WristThrustRight").GetComponent<AudioSource>();
            leftController = GameObject.Find("LeftHand");
            rightController = GameObject.Find("RightHand");
            leftLine = leftController.GetComponentInChildren<XRInteractorLineVisual>();
            rightLine = rightController.GetComponentInChildren<XRInteractorLineVisual>();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            m_IsMovingXROrigin = false;

            // Use the input amount to scale the turn speed.
            var leftInput = ReadLeftInput();
            var rightInput = ReadRightInput();
            var boostAmount = GetBoostAmount(leftInput, rightInput);
            var boostDirection = GetBoostDirection(leftInput, rightInput);
            WristBoostRig(boostAmount, boostDirection);

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
        /// Reads the current value of theinput.
        /// </summary>
        /// <returns>Returns the input vector, such as from a button.</returns>
        protected abstract float ReadLeftInput();
        protected abstract float ReadRightInput();

        /// <summary>
        /// Determines the press amount for given button <paramref name="input"/> boolean.
        /// </summary>
        /// <param name="input">Input vector, such as from a button.</param>
        /// <returns>Returns the button press boolean. <paramref name="input"/> vector.</returns>
        protected virtual float GetBoostAmount(float leftInput, float rightInput)
        {
            if ((leftInput == 0) && (rightInput == 0))
                return 0f;

            if ((leftInput == 1) && (rightInput == 1))
                return boostSpeed * 2;

            if ((leftInput == 1) || (rightInput == 1))
                return boostSpeed;

            return 0f;
        }

        protected virtual Vector3 GetBoostDirection(float leftInput, float rightInput)
        {
            if ((leftInput == 0) && (rightInput == 0))
            {
                if (wristThrustLeft.isPlaying)
                {
                    wristThrustLeft.Stop();
                }
                if (wristThrustRight.isPlaying)
                {
                    wristThrustRight.Stop();
                }
                return Vector3.zero;
            }

            if ((leftInput == 1) && (rightInput == 1))
            {
                if (!wristThrustLeft.isPlaying)
                {
                    wristThrustLeft.Play();
                }
                if (!wristThrustRight.isPlaying)
                {
                    wristThrustRight.Play();
                }
                return (leftLine.transform.forward + rightLine.transform.forward);
            }

            else if ((leftInput == 1) || (rightInput == 1))
            {
                if (leftInput == 1)
                {
                    if (!wristThrustLeft.isPlaying)
                    {
                        wristThrustLeft.Play();
                    }
                    return leftLine.transform.forward;
                }

                if (rightInput == 1)
                {
                    if (!wristThrustRight.isPlaying)
                    {
                        wristThrustRight.Play();
                    }
                    return rightLine.transform.forward;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Boosts the rig by <paramref name="boostAmount"/>.
        /// </summary>
        /// <param name="boostAmount">The amount of boost.</param>
        protected void WristBoostRig(float boostAmount, Vector3 boostDirection)
        {
            if (Mathf.Approximately(boostAmount, 0f))
                return;

            if (CanBeginLocomotion() && BeginLocomotion())
            {
                var xrOrigin = system.xrOrigin;
                if (xrOrigin != null)
                {
                    m_IsMovingXROrigin = true;
                    Rigidbody xrRigidbody = GetComponent<Rigidbody>();

                    // go vroom
                    Vector3 currentVelocity = xrRigidbody.velocity;
                    if (currentVelocity.magnitude < 4f)
                        xrRigidbody.AddForce((boostDirection.normalized * boostAmount/2), ForceMode.Force);
                    else if (currentVelocity.magnitude >= 4f)
                    {
                        xrRigidbody.AddForce((boostDirection.normalized + -currentVelocity.normalized) * boostAmount/2, ForceMode.Force);
                    }
                }

                EndLocomotion();
            }
        }
    }
}
