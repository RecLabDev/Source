using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
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
        internal AudioSource _audio;
        //added this for death animation chage:
        internal Animator animator; // Make sure to assign this in Awake or in the Editor.
        SpriteRenderer spriteRenderer;

        //added this for death animation chage:
        private bool isDead = false;
        public bool IsDead => isDead;

        public Bounds Bounds => _collider.bounds;

        /// <summary>
        /// TODO
        /// </summary>
        public Vector3 Direction => spriteRenderer.flipX ? Vector3.right : Vector3.left;

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsAttacking => !isDead;

        void Awake()
        {
            control = GetComponent<AnimationController>();
            _collider = GetComponent<Collider2D>();
            _audio = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void Hurt()
        {
            // TODO
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

        /*public Collider2D playerInteractionCollider;

        public void Die()
        {
            if (isDead) return;
            isDead = true;

            // Trigger hurt animation.
            animator.SetTrigger("hurt");

            // Play the ouch sound.
            if (_audio && ouch)
            {
                _audio.PlayOneShot(ouch);
            }

            // Change the layer of the enemy to DeadEnemies so it doesn't collide with the player anymore.
            gameObject.layer = LayerMask.NameToLayer("Ghosts");

            // Continue with the death sequence.
            StartCoroutine(DelayedDeath());
        }*/



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

        /// <summary>
        /// TODO
        /// </summary>
        void Update()
        {
            if (isDead)
            {
                control.move.x = 0;
            }
            else if (path != null)
            {
                if (mover == null) mover = path.CreateMover(control.maxSpeed * 0.5f);
                control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1, 1);
            }
        }
    }
}