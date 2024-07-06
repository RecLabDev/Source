using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Unity.Netcode;

namespace Aby.Unity
{
    public class HudManager : MonoBehaviour
    {
        /// <summary>
        /// TODO
        /// </summary>
        private UIDocument uiDocument;

        /// <summary>
        /// TODO
        /// </summary>
        void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            Debug.LogFormat("UI Document: {0}", uiDocument);

            DrawPartyDisplay();
        }

        /// <summary>
        /// TODO
        /// </summary>
        void Update()
        {

        }

        /// <summary>
        /// TODO
        /// </summary>
        private void DrawPartyDisplay()
        {
            var hostButton = uiDocument.rootVisualElement.Q<Button>("HostButton");
            if (hostButton == null)
            {
                Debug.LogError("HostButton not found!");
            }
            else
            {
                hostButton.clicked += OnClickedHostButton;
            }

            var clientButton = uiDocument.rootVisualElement.Q<Button>("ClientButton");
            if (clientButton == null)
            {
                Debug.LogError("ClientButton not found!");
            }
            else
            {
                clientButton.clicked += OnClickedClientButton;
            }

            var serverButton = uiDocument.rootVisualElement.Q<Button>("ServerButton");
            if (serverButton == null)
            {
                Debug.LogError("ServerButton not found!");
            }
            else
            {
                serverButton.clicked += OnClickedServerButton;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void OnClickedHostButton()
        {
            NetworkManager.Singleton.StartHost();
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void OnClickedClientButton()
        {
            NetworkManager.Singleton.StartClient();
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void OnClickedServerButton()
        {
            NetworkManager.Singleton.StartServer();
        }
    }
}
