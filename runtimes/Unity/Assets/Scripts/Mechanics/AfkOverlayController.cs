using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aby.Unity
{
    public class AfkOverlayController : MonoBehaviour
    {
        /// <summary>
        /// TODO
        /// </summary>
        public int SelectedPanelIndex = 0;

        /// <summary>
        /// TODO
        /// </summary>
        public GameObject[] StatePanels;

        /// <summary>
        /// TODO
        /// </summary>
        private void OnEnable()
        {
            // TODO: Init last state?
            SetActivePanel(SelectedPanelIndex);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void Update()
        {
            // ..
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="index"></param>
        public void SetActivePanel(int index)
        {
            SelectedPanelIndex = index;
            for (var i = 0; i < StatePanels.Length; i++)
            {
                var active = i == SelectedPanelIndex;
                if (StatePanels[i].activeSelf != active)
                {
                    StatePanels[i].SetActive(active);
                }
            }
        }
    }
}
