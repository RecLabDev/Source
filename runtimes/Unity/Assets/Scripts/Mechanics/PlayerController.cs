using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/
        public Collider2D collider2d;
        /*internal new*/
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool attack;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        /// <summary>
        /// TODO
        /// </summary>
        public Bounds Bounds => collider2d.bounds;

        /// <summary>
        /// TODO
        /// </summary>
        public Vector3 Direction => spriteRenderer.flipX ? Vector3.right : Vector3.left;

        public bool IsAttacking => attack;

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
                move.x = Input.GetAxis("Horizontal");

                // Check if the player is attacking using the Input Manager.
                // It could be an "f" key press or a controller button press.
                if (Input.GetButtonDown("Attack"))
                {
                    attack = true;

                }
                else if (Input.GetButtonUp("Attack"))
                {
                    attack = false;
                }

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

        void UpdateAttackState()
        {
            animator.SetBool("Attacking", attack);
        }

        public void OnAttack(InputAction.CallbackContext actionContext)
        {
            attack = controlEnabled && actionContext.performed;
        }

        void HandleSlashing()
        {
            // Determine the direction of the attack based on the facing of the sprite.
            float direction = spriteRenderer.flipX ? -1f : 1f; // -1 for left, 1 for right
            // Set the attackPoint in front of the player based on the direction they are facing.
            var attackPoint = transform.position + new Vector3(direction * 0.5f, 0, 0);
            var attackRange = new Vector2(0.1f, 1f); // This creates a wide hitbox.

            // Get all the hit enemies.
            var hitEnemies = Physics2D.OverlapBoxAll(attackPoint, attackRange, 0, LayerMask.GetMask("Enemies"));

            foreach (var hit in hitEnemies)
            {
                var enemy = hit.GetComponent<EnemyController>();
                if (enemy != null)
                {
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
            }
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
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
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}