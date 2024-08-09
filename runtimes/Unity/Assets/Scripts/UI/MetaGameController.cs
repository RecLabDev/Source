using Aby.Unity;
using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Platformer.UI
{
    /// <summary>
    /// The MetaGameController is responsible for switching control between the high level
    /// contexts of the application, eg the Main Menu and Gameplay systems.
    /// </summary>
    public class MetaGameController : MonoBehaviour
    {
        /// <summary>
        /// The game controller.
        /// </summary>
        public GameController gameController;

        /// <summary>
        /// TODO
        /// </summary>
        public AfkOverlayController afkOverlay;

        /// <summary>
        /// A list of canvas objects which are used during gameplay (when the main ui is turned off)
        /// </summary>
        public Canvas[] gamePlayCanvases;

        /// <summary>
        /// TODO
        /// </summary>
        bool shouldEnable = false;

        /// <summary>
        /// Mount to the Main Menu and enable target tracking.
        /// </summary>
        void OnEnable()
        {
            ToggleAfkOverlay(shouldEnable);
        }

        /// <summary>
        /// TODO
        /// </summary>
        void Update()
        {
            // ..
            if (Input.GetButtonDown("Menu"))
            {
                ToggleAfkOverlay(!shouldEnable);
            }
        }

        /// <summary>
        /// Turn the main menu on or off.
        /// </summary>
        /// <param name="show"></param>
        public void ToggleAfkOverlay(bool targetState)
        {
            if (shouldEnable != targetState)
            {
                afkOverlay.gameObject.SetActive(targetState);
                shouldEnable = targetState;
            }
        }
    }
}
