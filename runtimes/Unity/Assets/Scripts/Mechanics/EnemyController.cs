using System;
using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using Unity.VisualScripting;
using UnityEngine;
using static Platformer.Core.Simulation;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Platformer.Mechanics
{
    /// <summary>
    /// A simple controller for enemies. Provides movement control over a patrol path.
    /// </summary>
    [RequireComponent(typeof(AnimationController), typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        public PatrolPath path;
        public AudioClip ouch;

        internal PatrolPath.Mover mover;
        internal AnimationController control;
        internal Collider2D _collider;
        internal CapsuleCollider2D _collider2;
        internal AudioSource _audio;
        //added this for death animation chage:
        internal Animator animator; // Make sure to assign this in Awake or in the Editor.
        SpriteRenderer spriteRenderer;

        //added this for death animation chage:
        protected bool isDead = false; //changed from private to protected for flying enemies
        public bool IsDead => isDead;

        public bool isFlying = false;
        public bool IsFlying => isFlying;

        public Bounds Bounds => _collider.bounds;

        /// <summary>
        /// TODO
        /// </summary>
        public Vector3 Direction => spriteRenderer.flipX ? Vector3.right : Vector3.left;

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsAttacking = false;

        //for chase stuff
        private GameObject playerLockOn;

        public bool chase = false;
        public Transform startingPoint;

        // Add a detection radius field.
        public float detectionRadius = 0.1f;

        // Flag to check if the enemy is currently chasing.
        private bool isChasing = false;

        void Awake()
        {
            control = GetComponent<AnimationController>();
            //changed from Collider2D to CapsuleCollider2D
            _collider = GetComponent<Collider2D>();
            _collider2 = GetComponent<CapsuleCollider2D>();
            _audio = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            playerLockOn = GameObject.FindGameObjectWithTag("Player");
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Hurt()
        {
            // TODO
        }

        public void Chase()
        {
            //


            // Make sure the move.x is set so that the walking animation can be triggered.
            control.move.x = (playerLockOn.transform.position.x > transform.position.x) ? 1 : -1;

            // Calculate the target position with the player's x position but maintain the enemy's current y position.
            Vector3 targetPosition = new Vector3(playerLockOn.transform.position.x, transform.position.y, transform.position.z);

            // Move towards the target position at the defined speed, only on the x-axis.
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, control.maxSpeed * Time.deltaTime);

            // Update the velocityX parameter of the animator to trigger the walking animation.
            animator.SetFloat("velocityX", Mathf.Abs(control.move.x));

            var relativeDirection = (transform.position - playerLockOn.transform.position).normalized;
            var isEnemyFacingPlayer = Vector3.Dot(Direction, relativeDirection) > 0;
            float distanceToPlayerX = Mathf.Abs(transform.position.x - playerLockOn.transform.position.x);
            if (isEnemyFacingPlayer && (distanceToPlayerX < 1))
            {
                IsAttacking = true;
                animator.SetBool("attacking", true);
                //schedule player death


            }
            else
            {
                IsAttacking = false;
                animator.SetBool("attacking", false);
            }
        }

        public void Flip()
        {
            // Determine the direction by comparing the enemy's position with the player's position.
            bool shouldFaceRight = playerLockOn.transform.position.x > transform.position.x;

            // If the direction to face is right and the sprite is not already facing right, flip it.
            if (shouldFaceRight && spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
            }
            // If the direction to face is left and the sprite is not already facing left, flip it.
            else if (!shouldFaceRight && !spriteRenderer.flipX)
            {
                spriteRenderer.flipX = true;
            }
        }

        public void ReturnStartPosition()
        {
            transform.position = Vector2.MoveTowards(transform.position, startingPoint.position, control.maxSpeed * Time.deltaTime);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Die()
        {
            if (isDead) return;
            else
            {
                // Note: We'll want to trigger hurt seperately from death
                //   when we implement heath for all the enemies.
                // TODO: Move this to a seperate interaction.
                animator.SetTrigger("hurt");

                isDead = true;
                TotalDeaths++;
                if (_audio && ouch)
                {
                    _audio.PlayOneShot(ouch);
                }
                gameObject.layer = LayerMask.NameToLayer("Ghosts");
                // TODO: Can we do this without a co-routine?
                StartCoroutine(DelayedDeath());
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayedDeath()
        {
            // Disable enemy movement and other interactions
            control.enabled = false; // assuming this disables the enemy's ability to move or be moved

            // Play the death animation
            animator.SetTrigger("death");

            // Wait for the animation to finish
            yield return new WaitForSeconds(1.5f); // Adjust this time to the length of your death animation

            // Start the fade out and despawn process
            StartCoroutine(Despawn());
        }

        private int TotalDeaths = 0;
        private int TotalRevives = 0;
        public float despawnDelay = 60f;

        IEnumerator Despawn()
        {
            if (TotalDeaths > TotalRevives)
            {
                // Wait to remove the corpse from the scene.
                yield return new WaitForSeconds(despawnDelay);

                var sprite = GetComponent<SpriteRenderer>();
                var originalColor = sprite.color;
                var fadeDuration = 0.25f;
                for (float t = 0; t < 1; t += Time.deltaTime / fadeDuration)
                {
                    sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1, 0, t));
                    yield return null;
                }

                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="collision"></param>
        void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                var ev = Schedule<PlayerEnemyCollision>();
                ev.player = player;
                ev.enemy = this;
            }
        }


        void Patrol()
        {
            // Your existing patrol logic here.
            if (mover == null) mover = path.CreateMover(control.maxSpeed * 0.5f);
            control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1, 1);
        }

        //added for flying enemies
        protected virtual void Move()
        {
            if (isDead)
            {
                control.move.x = 0;
            }
            else if (path != null)
            {
                // If not chasing and a patrol path is set, continue patrolling.
                Patrol();
            }

            if (isFlying)
            {
                // Negate gravity by subtracting it, multiplied by gravityModifier and deltaTime
                control.velocity += Vector2.up * control.gravityModifier * Physics2D.gravity * Time.deltaTime;
            }
        }




        /// <summary>
        /// TODO
        /// </summary>
        protected virtual void Update()
        {
            Debug.LogFormat("player lock on: {0}", playerLockOn);
            if (!isDead && playerLockOn == null)
            {
                var closestPlayers = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
                foreach (var closestPlayer in closestPlayers)
                {
                    Debug.LogFormat("Found Player: {0}", closestPlayer);
                    var closestPlayerController = closestPlayer.GetComponent<PlayerController>();
                    if (!closestPlayerController.isDead)
                    {
                        playerLockOn = closestPlayer.gameObject;
                    }
                }
            }

            if (playerLockOn != null)
            {
                Chase();
            }
            else if (path != null)
            {
                // If not chasing and a patrol path is set, continue patrolling.
                Patrol();
            }
        }
    }
}