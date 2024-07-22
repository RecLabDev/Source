using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aby.Unity
{
    public class RecLabHub : EditorWindow
    {      
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private StyleSheet styleSheet;
        
        private RecLabServerID newServerID;
        private const string SERVER_ID_PREF_KEY = "RecLabHub_ServerID";

        private const string WINDOW_TITLE = "RecLab Hub";
        private const string TEXT_FIELD_NAME = "TextField";
        private const string CONNECT_BUTTON_NAME = "ConnectButton";
        private const string DISCONNECT_BUTTON_NAME = "DisconnectButton";

        //Track connection status
        private bool isConnected = false;

        [MenuItem("Aby/RecLab Hub")]
        public static void OpenWindow()
        {
            RecLabHub wnd = GetWindow<RecLabHub>();
            wnd.titleContent = new GUIContent(WINDOW_TITLE);

            //Lock the size of the window, uncomment if needed
            //wnd.minSize = new Vector2(500, 300);
            //wnd.maxSize = new Vector2(500, 300);
        }

        public void CreateGUI()
        {   
            // Initialize server ID
            newServerID = new RecLabServerID();
            newServerID.serverID = EditorPrefs.GetString(SERVER_ID_PREF_KEY, string.Empty); // Load saved server ID, if available

            // Create root VisualElement
            VisualElement root = rootVisualElement;

            // Add the style sheet
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogError("USS not assigned in the inspector.");
            }

            // Add the UXML
            if (visualTreeAsset != null)
            {
                VisualElement uxml = visualTreeAsset.CloneTree();
                root.Add(uxml);
            }
            else
            {
                Debug.LogError("UXML not assigned in the inspector.");
            }

            // Get the TextField and assign current value of serverID.text
            TextField textField = root.Q<TextField>("TextField");
            if (textField != null)
            {
                textField.value = newServerID.serverID;

                // Register a callback to handle server changes
                textField.RegisterValueChangedCallback(evt =>
                {
                    newServerID.serverID = evt.newValue;
                    EditorPrefs.SetString(SERVER_ID_PREF_KEY, newServerID.serverID);
                    
                    EditorUtility.SetDirty(this);
                    Debug.Log($"Server ID updated to '{newServerID.serverID}'!");
                });
            }
            else
            {
                Debug.LogError($"TextField with name '{TEXT_FIELD_NAME}' not found in UXML.");
            }

            // Get the Connect Button and assign click event
            Button connectButton = root.Q<Button>(CONNECT_BUTTON_NAME);
            if (connectButton != null)
            {
                connectButton.clicked += () =>
                {
                    OnClickedConnectButton(newServerID.serverID);
                };
            }
            else
            {
                Debug.LogError($"Button with name '{CONNECT_BUTTON_NAME}' not found in UXML.");
            }

            // Get the Disconnect Button and assign click event
            Button disconnectButton = root.Q<Button>(DISCONNECT_BUTTON_NAME);
            if (disconnectButton != null)
            {
                disconnectButton.clicked += () =>
                {
                    OnClickedDisconnectButton();
                };
                disconnectButton.style.display = DisplayStyle.None; // Hide by default
            }
            else
            {
                Debug.LogError($"Button with name '{DISCONNECT_BUTTON_NAME}' not found in UXML.");
            }

            // Update Disconnect button visibility based on connection status
            // Update button visibility based on connection status
            UpdateButtonVisibility();
        }
        
        private void OnClickedConnectButton(string serverID)
        {
            //Placeholder for actual connection logic
            Debug.Log($"Attempting to connect to server with ID: {serverID}");
            //Implement connection logic here
            isConnected = true;
            UpdateButtonVisibility();
        }

        private void OnClickedDisconnectButton()
        {
            // Placeholder for actual disconnection logic
            Debug.Log($"Disconnecting from server with ID: {newServerID.serverID}");
            // Implement disconnection logic here
            isConnected = false; // Assume disconnection is successful for now
            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
        {
            VisualElement root = rootVisualElement;
            Button connectButton = root.Q<Button>(CONNECT_BUTTON_NAME);
            Button disconnectButton = root.Q<Button>(DISCONNECT_BUTTON_NAME);

            if (connectButton != null)
            {
                connectButton.style.display = isConnected ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (disconnectButton != null)
            {
                disconnectButton.style.display = isConnected ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }

    [System.Serializable]
    public class RecLabServerID
    {
        public string serverID;
    }
}
