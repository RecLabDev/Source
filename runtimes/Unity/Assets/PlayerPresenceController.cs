using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Theta
{
    public class PlayerPresenceController : MonoBehaviour
    {
        public string playerName;
        public Color bannerColor;
        public VisualTreeAsset mainUI;
        public StyleSheet[] seasonalThemes;

        private UIDocument uiDocument;
        private VisualElement haloContainer;
        private Label nameLabel;

        void Start()
        {
            // Ensure UI Document component is assigned and correctly set up
            var quad = transform.Find("Quad");
            if (quad != null)
            {
                uiDocument = quad.GetComponent<UIDocument>();
                if (uiDocument != null)
                {
                    var root = uiDocument.rootVisualElement;
                    haloContainer = root.Q<VisualElement>("HaloContainer");
                    nameLabel = root.Q<Label>("NameDisplay");

                    if (haloContainer != null && nameLabel != null)
                    {
                        // Set the player's name and banner color
                        nameLabel.text = playerName;
                        haloContainer.style.backgroundColor = new StyleColor(bannerColor);
                    }
                    else
                    {
                        Debug.LogError("UI Elements not found in the UXML.");
                    }
                }
                else
                {
                    Debug.LogError("UIDocument component not found on the Quad object.");
                }
            }
            else
            {
                Debug.LogError("Quad object not found as a child of the player.");
            }
        }
    }
}
