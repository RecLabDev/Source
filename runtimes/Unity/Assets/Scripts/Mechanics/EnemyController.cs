using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

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
            //added this for death animation chage:
            animator = GetComponent<Animator>(); // Make sure the enemy GameObject has an Animator component
        }

        //added this for death animation chage://///////////////////////////////////
        public void Hurt()
        {
            Debug.Log("Hurt animation triggered");
            // Trigger the hurt animation
            animator.SetTrigger("hurt");
        }

        public void Die()
        {
            if (isDead) return;
            else
            {
                isDead = true;

                Hurt();
                StartCoroutine(DelayedDeath());
            }
        }

        private IEnumerator DelayedDeath()
        {
            yield return new WaitForSeconds(0.5f); // Assuming your hurt animation is about 1 second long.

            Debug.Log("Death animation triggered");

            animator.SetTrigger("death");
            if (_audio && ouch)
                _audio.PlayOneShot(ouch);

            StartCoroutine(FadeOutAndDisable());
        }

        IEnumerator FadeOutAndDisable()
        {
            // Wait for death animation to finish.
            yield return new WaitForSeconds(1f);

            // Start fading out.
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            Color originalColor = sprite.color;
            float fadeDuration = 1f; // Set duration for how long the fade out should take.
            for (float t = 0; t < 1; t += Time.deltaTime / fadeDuration)
            {
                sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1, 0, t));
                yield return null;
            }

            // Disable or destroy the enemy.
            gameObject.SetActive(false);
            // Or if you want to destroy it: Destroy(gameObject);
        }

        ////////////////////////////////////////////////////////////////////////////

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