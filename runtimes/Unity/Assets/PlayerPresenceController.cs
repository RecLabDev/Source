using System.Collections;
using System.Collections.Generic;
using System.Xml;
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


        //make a flag/bubble over the players head that shows their
        //name and the bubble should be the color set in the above field

        // Start is called before the first frame update
        void Start()
        {
            // Ensure UI Document component is assigned and correctly set up
            uiDocument = GetComponentInChildren<UIDocument>();
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
                Debug.LogError("UIDocument component not found on the player or its children.");
            }
        }
    }
}