using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aby
{
    public class ChestEffect : MonoBehaviour
    {
        public Animator animator; // Assign this in the inspector
        public AudioSource audioSource;
        public AudioClip chestOpeningSound;
        private bool isOpened = false;

        public void OpenChestEffect()
        {
            if (!isOpened)
            {
                Debug.Log("Made it into ChestEffect");
                isOpened = true; // to ensure that the chest doesnt get opened again after it is already opened.
                animator.SetTrigger("Open"); // Make sure "Open" is the name of the trigger in your Animator
                //PlayChestOpeningSound();
            }
        }
        public void PlayChestOpeningSound()
        {
            audioSource.PlayOneShot(chestOpeningSound);
        }
    }
}
