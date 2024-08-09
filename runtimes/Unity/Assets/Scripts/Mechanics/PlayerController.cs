using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

using Cinemachine;
using Cinemachine.Editor;
using Cinemachine.Utility;

using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

using Aby;
using Aby.Unity;
using UnityEngine.SocialPlatforms.Impl;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        /// <summary>
        /// TODO
        /// </summary>
        public Health health;

        /// <summary>
        /// TODO
        /// </summary>
        public bool isDead = false;

        /// <summary>
        /// TODO
        /// </summary>
        public bool controlEnabled = true;

        /// <summary>
        /// TODO
        /// </summary>
        public Vector3 startingPosition = Vector3.zero;

        /// <summary>
        /// TODO
        /// </summary>
        public Bounds Bounds => collider2d.bounds;

        /// <summary>
        /// TODO
        /// </summary>
        public Vector3 Direction => spriteRenderer.flipX ? Vector3.right : Vector3.left;


        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7.0f;

        /// <summary>
        /// TODO
        /// </summary>
        public CinemachineVirtualCamera playerCamera;

        /// <summary>
        /// TODO
        /// </summary>
        public JumpState jumpState = JumpState.Grounded;

        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7.0f;

        /// <summary>
        /// TODO
        /// </summary>
        public float doubleJumpForce = 0.65f;

        /// <summary>
        /// TODO
        /// </summary>
        private bool isJumping;

        /// <summary>
        /// TODO
        /// </summary>
        private bool stopJump;

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsAttacking => attack;

        /// <summary>
        /// TODO
        /// </summary>
        private bool attack;

        /// <summary>
        /// TODO
        /// </summary>
        public AudioClip respawnSound;
        public AudioClip slashSound;
        public AudioClip hurtSound;
        public AudioClip jumpSound;
        public AudioClip victorySound;

        SpriteRenderer spriteRenderer;
        internal Animator animator;
        public AudioSource audioSource;
        public Collider2D collider2d;

        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        Vector2 move;

        /// <summary>
        /// TODO
        /// </summary>
        void Awake()
        {
            playerCamera = FindObjectOfType<CinemachineVirtualCamera>();

            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();

            health = GetComponent<Health>();
        }

        protected new void OnEnable()
        {
            base.OnEnable();

            // TODO: Set the spawn position to our chosen spawn location ..
            transform.position = startingPosition;

            if (playerCamera == null)
            {
                Debug.LogWarningFormat("Couldn't get camera for player ..");
            }
        }

        protected override void Update()
        {
            if (!IsOwner) return;

            if (controlEnabled)
            {
                // Check if the player is attacking using the Input Manager.
                // It could be an "f" key press or a controller button press.
                if (Input.GetButtonDown("Attack"))
                {
                    attack = true;
                    PlaySlashSound();
                }
                else if (Input.GetButtonUp("Attack"))
                {
                    attack = false;
                }

                var joyStickMove = Input.GetAxis("Horizontal");
                if (attack && jumpState == JumpState.Grounded)
                {
                    joyStickMove = joyStickMove / 4;
                }

                move.x = joyStickMove;

                //adding this for player attacking for damage
                if (attack && animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerSlash"))
                {
                    HandleSlashing();
                }

                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }

                // Movement and sprite direction
                if (move.x > 0.01f)
                {
                    AttemptFlipSpriteServerRpc(false);
                }
                else if (move.x < -0.01f)
                {
                    AttemptFlipSpriteServerRpc(true);
                }
            }
            else
            {
                move.x = 0;
            }

            UpdateJumpState();
            UpdateAttackState();
            UpdateCameraPosition();

            base.Update();
        }

        public void PlayVictorySound()
        {
            audioSource.PlayOneShot(victorySound);
        }

        void UpdateAttackState()
        {
            animator.SetBool("Attacking", attack);
        }

        public void OnAttack(InputAction.CallbackContext actionContext)
        {
            attack = controlEnabled && actionContext.performed;
        }

        private void UpdateCameraPosition()
        {
            if (IsLocalPlayer && playerCamera?.Follow != transform)
            {
                Debug.LogFormat("Camera {0} now follows {1} ..", playerCamera.name, transform);
                playerCamera.Follow = transform;
            }
        }

        void PlaySlashSound()
        {
            if (audioSource != null && slashSound != null)
            {
                audioSource.PlayOneShot(slashSound);
            }
        }

        void HandleSlashing()
        {
            // Determine the direction of the attack based on the facing of the sprite.
            float direction = spriteRenderer.flipX ? -1f : 1f; // -1 for left, 1 for right
            // Set the attackPoint in front of the player based on the direction they are facing.
            var attackPoint = transform.position + new Vector3(direction * 0.5f, 0, 0);
            var attackRange = new Vector2(0.1f, 1f); // This creates a wide hitbox.



            //for opening chest and enemies. 
            var hitObjects = Physics2D.OverlapBoxAll(attackPoint, attackRange, 0);
            // Get all the hit enemies.// should be able to get ride of this now that we have hitObjects.
            //var hitEnemies = Physics2D.OverlapBoxAll(attackPoint, attackRange, 0, LayerMask.GetMask("Enemies"));

            foreach (var hit in hitObjects)
            {
                if (hit.CompareTag("Enemy"))
                {
                    var enemy = hit.GetComponent<EnemyController>();
                    // Check if the enemy is in the direction the player is facing.
                    float enemyDirection = Mathf.Sign(enemy.transform.position.x - transform.position.x);
                    if (enemyDirection == direction)
                    {
                        // The enemy is in the attack direction, schedule the slash event.
                        var slashEvent = Schedule<PlayerSlashEnemy>();
                        slashEvent.player = this;
                        slashEvent.enemy = enemy;
                    }
                }
                else if (hit.CompareTag("Chest"))
                {
                    var chest = hit.GetComponent<Chest>();
                    var slashChest = Schedule<PlayerSlashChest>();
                    slashChest.player = this;
                    slashChest.chest = chest;
                }
            }
        }

        void PlayJumpSound()
        {
            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }



        private bool canDoubleJump = true;

        void UpdateJumpState()
        {
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    PlayJumpSound();
                    jumpState = JumpState.Jumping;
                    stopJump = false;

                    // TODO: Can we do away with `isJumping` in favor of the `jumpState`?
                    isJumping = true;

                    // Reset double-jump flag for each initial jump.
                    canDoubleJump = true;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    // User is in-flight. From this position they can either jump again,
                    if (Input.GetButtonDown("Jump") && !stopJump && canDoubleJump)
                    {
                        PlayJumpSound();
                        //jumpState = JumpState.Jumping;

                        // TODO: Can we do away with `isJumping` in favor of the `jumpState`?
                        isJumping = true;

                        // Can't double jump again until 
                        canDoubleJump = false;
                    }
                    // or land on the ground.
                    else if (IsGrounded)
                    {
                        jumpState = JumpState.Landed;
                    }

                    break;
                case JumpState.Landed:
                    // .. or they can land.
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (isJumping)
            {
                // Apply jump velocity for a double jump or a normal jump if grounded
                if (jumpState == JumpState.InFlight || IsGrounded)
                {
                    var jumpVelocity = jumpState == JumpState.InFlight ? jumpTakeOffSpeed * doubleJumpForce : jumpTakeOffSpeed;
                    velocity.y = jumpVelocity * model.jumpModifier;
                }
                isJumping = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            // Movement and sprite direction
            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            // Animation updates
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        [ServerRpc]
        public void AttemptFlipSpriteServerRpc(bool flipX)
        {
            FlipSpriteClientRpc(flipX);
            // Debug.Log("Sprite flip RPC fired!");
        }

        [ClientRpc]
        public void FlipSpriteClientRpc(bool flipX)
        {
            spriteRenderer.flipX = flipX;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            DoubleJump,
            InFlight,
            Landed
        }
    }
}