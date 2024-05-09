using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using Aby;

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

        public bool isDead = false;
        /// <summary>
        /// TODO
        /// </summary>
        public bool controlEnabled = true;

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

        /*internal new*/
        public AudioSource audioSource;

        /// <summary>
        /// TODO
        /// </summary>
        public AudioClip respawnSound;
        public AudioClip slashSound;
        public AudioClip hurtSound;
        public AudioClip jumpSound;
        public AudioClip victorySound;


        /*internal new*/
        public Collider2D collider2d;

        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        /// <summary>
        /// TODO
        /// </summary>
        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void Update()
        {
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
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            UpdateAttackState();
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
                }else if (hit.CompareTag("Chest"))
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
                    jumpState = JumpState.Jumping;
                    isJumping = true;
                    stopJump = false;
                    PlayJumpSound(); // Trigger the jump sound.
                    canDoubleJump = true; // Reset double-jump flag for each initial jump.
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (Input.GetButtonDown("Jump") && !stopJump && canDoubleJump)
                    {
                        jumpState = JumpState.DoubleJump;
                        isJumping = true; // This is critical to trigger the double jump in ComputeVelocity.
                        canDoubleJump = false; // Ensure double-jump can't be used again until the player lands.
                        PlayJumpSound();
                    }
                    else if (IsGrounded)
                    {
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.DoubleJump:
                    // The logic for double-jumping is handled in the InFlight case. 
                    // This state primarily serves as a marker that a double-jump has occurred.
                    if (IsGrounded)
                    {
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (isJumping)
            {
                // Apply jump velocity for a double jump or a normal jump if grounded
                if (jumpState == JumpState.DoubleJump || IsGrounded)
                {
                    var jumpVelocity = jumpState == JumpState.DoubleJump ? jumpTakeOffSpeed * doubleJumpForce : jumpTakeOffSpeed;
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