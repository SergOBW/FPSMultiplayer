//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections;
using MFPS.PlayerController;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Movement. This is our main, and base, component that handles the character's movement.
    /// It contains all of the logic relating to moving, running, crouching, jumping...etc
    /// </summary>
    public class Movement : MovementBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Acceleration")]
        
        [Tooltip("How fast the character's speed increases.")]
        [SerializeField]
        private float acceleration = 9.0f;

        [Tooltip("Acceleration value used when the character is in the air. This means either jumping, or falling.")]
        [SerializeField]
        private float accelerationInAir = 3.0f;

        [Tooltip("How fast the character's speed decreases.")]
        [SerializeField]
        private float deceleration = 11.0f;
        
        [Title(label: "Speeds")]

        [Tooltip("The speed of the player while walking.")]
        [SerializeField]
        private float speedWalking = 4.0f;
        
        [Tooltip("How fast the player moves while aiming.")]
        [SerializeField]
        private float speedAiming = 3.2f;
        
        [Tooltip("How fast the player moves while aiming.")]
        [SerializeField]
        private float speedCrouching = 3.5f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float speedRunning = 6.8f;
        
        [Title(label: "Walking Multipliers")]
        
        [Tooltip("Value to multiply the walking speed by when the character is moving forward."), SerializeField]
        [Range(0.0f, 1.0f)]
        private float walkingMultiplierForward = 1.0f;

        [Tooltip("Value to multiply the walking speed by when the character is moving sideways.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float walkingMultiplierSideways = 1.0f;

        [Tooltip("Value to multiply the walking speed by when the character is moving backwards.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float walkingMultiplierBackwards = 1.0f;
        
        [Title(label: "Air")]

        [Tooltip("How much control the player has over changes in direction while the character is in the air.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float airControl = 0.8f;

        [Tooltip("The value of the character's gravity. Basically, defines how fast the character falls.")]
        [SerializeField]
        private float gravity = 1.1f;

        [Tooltip("The value of the character's gravity while jumping.")]
        [SerializeField]
        private float jumpGravity = 1.0f;

        [Tooltip("The force of the jump.")]
        [SerializeField]
        private float jumpForce = 100.0f;

        [Tooltip("Force applied to keep the character from flying away while descending slopes.")]
        [SerializeField]
        private float stickToGroundForce = 0.03f;

        [Title(label: "Crouching")]

        [Tooltip("Setting this to false will always block the character from crouching.")]
        [SerializeField]
        private bool canCrouch = true;

        [Tooltip("If true, the character will be able to crouch/un-crouch while falling, which can lead to " +
                 "some slightly interesting results.")]
        [SerializeField, ShowIf(nameof(canCrouch), true)]
        private bool canCrouchWhileFalling = false;

        [Tooltip("If true, the character will be able to jump while crouched too!")]
        [SerializeField, ShowIf(nameof(canCrouch), true)]
        private bool canJumpWhileCrouching = true;

        [Tooltip("Height of the character while crouching.")]
        [SerializeField, ShowIf(nameof(canCrouch), true)]
        private float crouchHeight = 1.0f;
        
        [Tooltip("Mask of possible layers that can cause overlaps when trying to un-crouch. Very important!")]
        [SerializeField, ShowIf(nameof(canCrouch), true)]
        private LayerMask crouchOverlapsMask;

        [Title(label: "Rigidbody Push")]

        [Tooltip("Force applied to other rigidbodies when walking into them. This force is multiplied by the character's " +
                 "velocity, so it is never applied by itself, that's important to note.")]
        [SerializeField]
        private float rigidbodyPushForce = 1.0f;
        
        

        #endregion

        #region FIELDS

        /// <summary>
        /// Controller.
        /// </summary>
        [SerializeField]private CharacterController controller;

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// The player character's equipped weapon.
        /// </summary>
        private WeaponBehaviour equippedWeapon;

        /// <summary>
        /// Default height of the character.
        /// </summary>
        private float standingHeight;

        /// <summary>
        /// Velocity.
        /// </summary>
        private Vector3 velocity;

        /// <summary>
        /// Is the character on the ground.
        /// </summary>
        private bool isGrounded;
        /// <summary>
        /// Was the character standing on the ground last frame.
        /// </summary>
        private bool wasGrounded;

        /// <summary>
        /// Is the character jumping?
        /// </summary>
        private bool jumping;
        /// <summary>
        /// If true, the character controller is crouched.
        /// </summary>
        private bool crouching;

        /// <summary>
        /// Stores the Time.time value when the character last jumped.
        /// </summary>
        private float lastJumpTime;

        private bool canMove;

        private float lastSlideTime;

        [Range(0.2f, 1.5f)] public float slideTime = 0.75f;
        [Range(0.1f, 2.5f)] public float slideCoolDown = 1.2f;
        [Range(1, 12)] public float slideFriction = 10;
        public float slideCameraTiltAngle = -22;
        
        private readonly Vector3 feetPositionOffset = new Vector3(0, 0.8f, 0);

        private bool isSliding;
        
        private float slideForce = 0;
        public float slideSpeed = 10;
        public MouseLook mouseLook;
        [SerializeField] private Transform headRoot;
        #endregion

        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Get Player Character
        }
        /// Initializes the FpsController on start.
        protected override void Start()
        {
            //Cache the controller.
            playerCharacter = bl_GameManager.Instance.LocalPlayerReferences.playerCharacter;
            
            //Save the default height.
            standingHeight = controller.height;
            
            mouseLook.Init(transform, headRoot);
            bl_EventHandler.onMatchStart += OnMatchStart;
            canMove = bl_MatchTimeManagerBase.HaveTimeStarted();
        }
        

        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        protected override void Update()
        {
            //Get the equipped weapon!
            //if (!canMove)return;
            equippedWeapon = playerCharacter.GetInventory().GetEquipped();

            //Get this frame's grounded value.
            isGrounded = IsGrounded();
            //Check if it has changed from last frame.
            if (isGrounded && !wasGrounded)
            {
                //Set jumping.
                jumping = false;
                //Set lastJumpTime.
                lastJumpTime = 0.0f;
            }
            else if (wasGrounded && !isGrounded)
                lastJumpTime = Time.time;
            
            //Move.
            MoveCharacter();
            //Save the grounded value to check for difference next frame.
            wasGrounded = isGrounded;
        }
        /// <summary>
        /// OnControllerColliderHit.
        /// </summary>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //Zero out the upward velocity if the character hits the ceiling.
            if (hit.moveDirection.y > 0.0f && velocity.y > 0.0f)
                velocity.y = 0.0f;

            //We need a rigidbody to push the object we just hit.
            Rigidbody hitRigidbody = hit.rigidbody;
            if (hitRigidbody == null)
                return;
            
            //AddForce.
            Vector3 force = (hit.moveDirection + Vector3.up * 0.35f) * velocity.magnitude * rigidbodyPushForce;
            hitRigidbody.AddForceAtPosition(force, hit.point);
        }
        
        #endregion

        #region METHODS

        /// <summary>
        /// Moves the character.
        /// </summary>
        ///
        private void OnMatchStart()
        {
            Debug.Log("MATCH STARTED");
            canMove = true;
        }
        private void MoveCharacter()
        {
            //Get Movement Input!
            if (!canMove)
            {
                return;
            }
            Vector2 frameInput = Vector3.ClampMagnitude(playerCharacter.GetInputMovement(), 1.0f);
            //Calculate local-space direction by using the player's input.
            var desiredDirection = new Vector3(frameInput.x, 0.0f, frameInput.y);
            
            //Running speed calculation.
            if (isSliding)
                desiredDirection *= slideForce;
            else if(playerCharacter.IsRunning())
                desiredDirection *= speedRunning;
            else
            {
                //Crouching Speed.
                if (crouching)
                    desiredDirection *= speedCrouching;
                else
                {
                    //Aiming speed calculation.
                    if (playerCharacter.IsAiming())
                        desiredDirection *= speedAiming;
                    else
                    {
                        //Multiply by the normal walking speed.
                        desiredDirection *= speedWalking;
                        //Multiply by the sideways multiplier, to get better feeling sideways movement.
                        desiredDirection.x *= walkingMultiplierSideways;
                        //Multiply by the forwards and backwards multiplier.
                        desiredDirection.z *=
                            (frameInput.y > 0 ? walkingMultiplierForward : walkingMultiplierBackwards);
                    }
                }
            } 
            
            Debug.Log(desiredDirection);

            //World space velocity calculation.
            desiredDirection = transform.TransformDirection(desiredDirection);
            //Multiply by the weapon movement speed multiplier. This helps us modify speeds based on the weapon!
            if (equippedWeapon != null)
                desiredDirection *= equippedWeapon.GetMultiplierMovementSpeed();
            
            //Apply gravity!
            if (isGrounded == false)
            {
                //Get rid of any upward velocity.
                if (wasGrounded && !jumping)
                    velocity.y = 0.0f;
                
                //Movement.
                velocity += desiredDirection * (accelerationInAir * airControl * Time.deltaTime);
                //Gravity.
                velocity.y -= (velocity.y >= 0 ? jumpGravity : gravity) * Time.deltaTime;
            }
            //Normal Movement On Ground.
            else if(!jumping)
            {
                //Update velocity with movement on the ground values.
                velocity = Vector3.Lerp(velocity, new Vector3(desiredDirection.x, velocity.y, desiredDirection.z), Time.deltaTime * (desiredDirection.sqrMagnitude > 0.0f ? acceleration : deceleration));
            }

            //Velocity Applied.
            Vector3 applied = velocity * Time.deltaTime;
            //Stick To Ground Force. Helps with making the character walk down slopes without floating.
            if (controller.isGrounded && !jumping)
                applied.y = -stickToGroundForce;

            //Move.
            if (controller.enabled)
            {
                controller.Move(applied);
            }

            if (slideForce > 0)
            {
                slideForce -= Time.deltaTime;
            }
            else
            {
                slideForce = 0;
            }
        }

        /// <summary>
        /// WasGrounded.
        /// </summary>
        public override bool WasGrounded() => wasGrounded;
        /// <summary>
        /// IsJumping.
        /// </summary>
        public override bool IsJumping() => jumping;

        /// <summary>
        /// Can Crouch.
        /// </summary>
        public override bool CanCrouch(bool newCrouching)
        {
            //Always block crouching if we need to.
            if (canCrouch == false)
                return false;

            //If we're in the air, and we cannot crouch while in the air, then we can ignore this execution!
            if (isGrounded == false && canCrouchWhileFalling == false)
                return false;
            
            //The controller can always crouch, the issue is un-crouching!
            if (newCrouching)
                return true;

            //Overlap check location.
            Vector3 sphereLocation = transform.position + Vector3.up * standingHeight;
            //Check for any overlaps.
            return (Physics.OverlapSphere(sphereLocation, controller.radius, crouchOverlapsMask).Length == 0);
        }
        
        public override void TrySlide()
        {
            Debug.Log("Try slide");
            if ((Time.time - lastSlideTime) < slideTime * slideCoolDown) return;//wait the equivalent of one extra slide before be able to slide again
            if (IsJumping()) return;

            Vector3 startPosition = (transform.position - feetPositionOffset) + (transform.forward * controller.radius);
            if (Physics.Linecast(startPosition, startPosition + transform.forward)) return;//there is something in front of the feet's

            isSliding = true;
            slideForce = slideSpeed;//slide force will be continually decreasing
            //playerReferences.gunManager.HeadAnimator.Play("slide-start", 0, 0); // if you want to use an animation instead
            mouseLook.SetTiltAngle(slideCameraTiltAngle);
            //Update the capsule's height.
            controller.height = isSliding ? crouchHeight : standingHeight;
            //Update the capsule's center.
            controller.center = controller.height / 2.0f * Vector3.up;
            /*
            if (slideSound != null)
            {
                m_AudioSource.clip = slideSound;
                m_AudioSource.volume = 0.7f;
                m_AudioSource.Play();
            }
            */
            mouseLook.UseOnlyCameraRotation();
            this.InvokeAfter(slideTime, () =>
            {
                /*
                if (Crounching && !bl_UtilityHelper.isMobile)
                    State = PlayerState.Crouching;
                else if (State != PlayerState.Jumping)
                    State = PlayerState.Idle;
                    */
                //Crounching = false;
                //DoCrouchTransition();
                isSliding = false;
                lastSlideTime = Time.time;
                mouseLook.SetTiltAngle(0);
                mouseLook.PortBodyOrientationToCamera();
                controller.height = isSliding ? crouchHeight : standingHeight;
                //Update the capsule's center.
                controller.center = controller.height / 2.0f * Vector3.up;
            });
            
        }

        public override bool IsSliding()
        {
            return isSliding;
        }

        /// <summary>
        /// IsCrouching.
        /// </summary>
        /// <returns></returns>
        public override bool IsCrouching() => crouching;

        public override PlayerState GetPlayerState()
        {
            
            if (isSliding)
            {
                return PlayerState.Sliding;
            }
            if (IsCrouching())
            {
                return PlayerState.Crouching;
            }

            if (IsJumping())
            {
                return PlayerState.Jumping;
            }

            if (playerCharacter != null)
            {
                if (playerCharacter.IsRunning())
                {
                    return PlayerState.Running;
                }
            }
            
            if (playerCharacter != null)
            {
                if (playerCharacter.IsWalking())
                {
                    return PlayerState.Walking;
                }
            }
            
            return PlayerState.Idle;
        }

        /// <summary>
        /// Jump.
        /// </summary>
        public override void Jump()
        {
            //We can ignore this if we're crouching and we're not allowed to do crouch-jumps.
            if (crouching && !canJumpWhileCrouching)
                return;
            
            //Block jumping when we're not grounded. This avoids us double jumping.
            if (!isGrounded)
                return;

            //Jump.
            jumping = true;
            //Apply Jump Velocity.
            velocity = new Vector3(velocity.x, Mathf.Sqrt(2.0f * jumpForce * jumpGravity), velocity.z);

            //Save lastJumpTime.
            lastJumpTime = Time.time;
        }
        /// <summary>
        /// Changes the controller's capsule height.
        /// </summary>
        public override void Crouch(bool newCrouching)
        {
            //Set the new crouching value.
            crouching = newCrouching;
            
            //Update the capsule's height.
            controller.height = crouching ? crouchHeight : standingHeight;
            //Update the capsule's center.
            controller.center = controller.height / 2.0f * Vector3.up;
        }

        public override void TryCrouch(bool value)
        {
            //Crouch.
            if (value && CanCrouch(true))
                Crouch(true);
            //Coroutine Un-Crouch.
            else if(!value)
                StartCoroutine(nameof(TryUncrouch));
        }

        /// <summary>
        /// Try Toggle Crouch.
        /// </summary>
        public override void TryToggleCrouch() => TryCrouch(!crouching);
        /// <summary>
        /// Tries to un-crouch the character.
        /// </summary>
        private IEnumerator TryUncrouch()
        {
            //If the movementBehaviour says that we can't go into whatever crouching state is the opposite, then
            //the character will have to forget about it, no way around it bois!
            yield return new WaitUntil(() => CanCrouch(false));
            
            //Un-Crouch.
            Crouch(false);
        }

        #endregion

        #region GETTERS

        /// <summary>
        /// GetLastJumpTime.
        /// </summary>
        public override float GetLastJumpTime() => lastJumpTime;

        /// <summary>
        /// Get Multiplier Forward.
        /// </summary>
        public override float GetMultiplierForward() => walkingMultiplierForward;
        /// <summary>
        /// Get Multiplier Sideways.
        /// </summary>
        public override float GetMultiplierSideways() => walkingMultiplierSideways;
        /// <summary>
        /// Get Multiplier Backwards.
        /// </summary>
        public override float GetMultiplierBackwards() => walkingMultiplierBackwards;

        /// <summary>
        /// Returns the value of Velocity.
        /// </summary>
        public override Vector3 GetVelocity()
        {
            return controller != null ? controller.velocity : Vector3.zero;
        }
        /// <summary>
        /// Returns the value of Grounded.
        /// </summary>
        public override bool IsGrounded()
        {
            return controller!= null && controller.isGrounded;
        }

        #endregion
    }
}