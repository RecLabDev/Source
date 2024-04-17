using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aby
{
    public class Chest : MonoBehaviour
    {
        public Animator animator; // Assign this in the inspector
        public AudioSource audioSource;
        public AudioClip chestOpeningSound;
        private bool isOpened = false;

        // Reference to the ChestEffect component
        public ChestEffect chestEffect; // Assign this in the inspector


        void Awake()
        {
            // Attempt to find the ChestEffect on the same GameObject if it's not assigned.
            chestEffect = GetComponentInChildren<ChestEffect>();
            if (chestEffect == null)
            {
                Debug.Log("chest effect componet not set!");

            }


            // Also ensure the AudioSource is assigned
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
        public void OpenChest()
        {
            if (!isOpened)
            {
                isOpened = true; // to ensure that the chest doesnt get opened again after it is already opened.
                animator.SetTrigger("Open"); // Make sure "Open" is the name of the trigger in your Animator
                PlayChestOpeningSound();
                chestEffect.OpenChestEffect();
            }
        }
        /*public void OpenChest()
        {
            if (!isOpened && chestEffect != null && animator != null)
            {
                isOpened = true;
                animator.SetTrigger("Open");
                chestEffect.OpenChestEffect();

                // Play the chest opening sound if the audio source and clip are available
                if (audioSource != null && chestOpeningSound != null)
                {
                    audioSource.PlayOneShot(chestOpeningSound);
                }
            }
        }*/


        //getting some error here when it comes to audio.
        public void PlayChestOpeningSound()
        {
            audioSource.PlayOneShot(chestOpeningSound);
            if (audioSource != null && chestOpeningSound != null)
            {
                audioSource.PlayOneShot(chestOpeningSound);
            }
        }
    }
}