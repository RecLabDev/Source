using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.UI
{
    /// <summary>
    /// A simple controller for switching between UI panels.
    /// </summary>
    public class LobbyDisplayController : MonoBehaviour
    {
        /// <summary>
        /// TODO
        /// </summary>
        public GameObject[] Panels;

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
            for (var i = 0; i < Panels.Length; i++)
            {
                var active = i == index;
                var g = Panels[i];
                if (g.activeSelf != active) g.SetActive(active);
            }
        }
    }
}