using UnityEngine;
using Unity.Netcode;

namespace Invector.vCharacterController
{
    public class vThirdPersonAnimator : vThirdPersonMotor
    {
        #region Variables                

        public const float walkSpeed = 0.5f;
        public const float runningSpeed = 1f;
        public const float sprintSpeed = 1.5f;

        // Networked variables for synchronization
        public NetworkVariable<bool> isStrafingNet = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> isSprintingNet = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> isGroundedNet = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> verticalSpeedNet = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> horizontalSpeedNet = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> inputMagnitudeNet = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        #endregion  

        public virtual void UpdateAnimator()
        {
            if (animator == null || !animator.enabled) return;

            // Sync the animator with network variables
            animator.SetBool(vAnimatorParameters.IsStrafing, isStrafingNet.Value);
            animator.SetBool(vAnimatorParameters.IsSprinting, isSprintingNet.Value);
            animator.SetBool(vAnimatorParameters.IsGrounded, isGroundedNet.Value);
            animator.SetFloat(vAnimatorParameters.GroundDistance, groundDistance);

            if (isStrafing)
            {
                animator.SetFloat(vAnimatorParameters.InputHorizontal, stopMove ? 0 : horizontalSpeedNet.Value, strafeSpeed.animationSmooth, Time.deltaTime);
                animator.SetFloat(vAnimatorParameters.InputVertical, stopMove ? 0 : verticalSpeedNet.Value, strafeSpeed.animationSmooth, Time.deltaTime);
            }
            else
            {
                animator.SetFloat(vAnimatorParameters.InputVertical, stopMove ? 0 : verticalSpeedNet.Value, freeSpeed.animationSmooth, Time.deltaTime);
            }

            animator.SetFloat(vAnimatorParameters.InputMagnitude, stopMove ? 0f : inputMagnitudeNet.Value, isStrafing ? strafeSpeed.animationSmooth : freeSpeed.animationSmooth, Time.deltaTime);
        }

        // Method to update the networked variables on the server
        public void SetIsStrafing(bool value)
        {
            if (IsOwner) // Ensure only the owner can update this
                isStrafingNet.Value = value;
        }

        public void SetIsSprinting(bool value)
        {
            if (IsOwner) // Ensure only the owner can update this
                isSprintingNet.Value = value;
        }

        public void SetIsGrounded(bool value)
        {
            if (IsOwner) // Ensure only the owner can update this
                isGroundedNet.Value = value;
        }

        public void SetMoveSpeed(float verticalSpeed, float horizontalSpeed, float inputMagnitude)
        {
            if (IsOwner) // Ensure only the owner can update this
            {
                verticalSpeedNet.Value = verticalSpeed;
                horizontalSpeedNet.Value = horizontalSpeed;
                inputMagnitudeNet.Value = inputMagnitude;
            }
        }

        public virtual void SetAnimatorMoveSpeed(vMovementSpeed speed)
        {
            Vector3 relativeInput = transform.InverseTransformDirection(moveDirection);
            verticalSpeed = relativeInput.z;
            horizontalSpeed = relativeInput.x;

            var newInput = new Vector2(verticalSpeed, horizontalSpeed);

            if (speed.walkByDefault)
                inputMagnitude = Mathf.Clamp(newInput.magnitude, 0, isSprinting ? runningSpeed : walkSpeed);
            else
                inputMagnitude = Mathf.Clamp(isSprinting ? newInput.magnitude + 0.5f : newInput.magnitude, 0, isSprinting ? sprintSpeed : runningSpeed);

            // Update the networked variables
            SetMoveSpeed(verticalSpeed, horizontalSpeed, inputMagnitude);
        }
    }

    public static partial class vAnimatorParameters
    {
        public static int InputHorizontal = Animator.StringToHash("InputHorizontal");
        public static int InputVertical = Animator.StringToHash("InputVertical");
        public static int InputMagnitude = Animator.StringToHash("InputMagnitude");
        public static int IsGrounded = Animator.StringToHash("IsGrounded");
        public static int IsStrafing = Animator.StringToHash("IsStrafing");
        public static int IsSprinting = Animator.StringToHash("IsSprinting");
        public static int GroundDistance = Animator.StringToHash("GroundDistance");
    }
}
