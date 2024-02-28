using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.UI
{
    /// <summary>
    /// A simple controller for switching between UI panels.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        /// <summary>
        /// TODO
        /// </summary>
        public GameObject[] panels;

        /// <summary>
        /// TODO
        /// </summary>
        void OnEnable()
        {
            SetActivePanel(0);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="index"></param>
        public void SetActivePanel(int index)
        {
            for (var i = 0; i < panels.Length; i++)
            {
                var active = i == index;
                var g = panels[i];
                if (g.activeSelf != active) g.SetActive(active);
            }
        }
    }
}