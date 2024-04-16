using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aby
{
    public class Chest : MonoBehaviour
    {
        public Animator animator; // Assign this in the inspector
        private bool isOpened = false;

        public void OpenChest()
        {
            if (!isOpened)
            {
                Debug.Log("Chest opened!");
                isOpened = true;
                animator.SetTrigger("Open"); // Make sure "Open" is the name of the trigger in your Animator
            }
        }
    }
}
