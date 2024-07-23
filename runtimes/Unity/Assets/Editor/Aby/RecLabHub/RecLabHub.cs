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
            //Create empty server ID reference, or load one from a previous session
            InitializeServerID();
            VisualElement root = rootVisualElement;

            //Add USS and UXML to the editor window
            AddStyleSheet(root);
            AddUXML(root);

            //Setup UI elements
            SetupConnectButton(root);
            SetupDisconnectButton(root);
            SetupTextField(root);

            // Update button visibility based on connection status
            UpdateButtonVisibility();
        }
        
        private void InitializeServerID()
        {
            newServerID = new RecLabServerID();

            //Load saved server ID, if available
            newServerID.serverID = EditorPrefs.GetString(SERVER_ID_PREF_KEY, string.Empty);
        }

        private void AddStyleSheet(VisualElement root)
        {
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogError("USS not assigned in the inspector.");
            }
        }

        private void AddUXML(VisualElement root)
        {
            if (visualTreeAsset != null)
            {
                VisualElement uxml = visualTreeAsset.CloneTree();
                root.Add(uxml);
            }
            else
            {
                Debug.LogError("UXML not assigned in the inspector.");
            }
        }

        private void SetupConnectButton(VisualElement root)
        {
            Button connectButton = root.Q<Button>(CONNECT_BUTTON_NAME);
            if (connectButton != null)
            {
                connectButton.clicked += () =>
                {
                    OnClickedConnectButton(newServerID.serverID);
                };

                //Initial validation check if length of the server ID is 9 char
                ValidateServerID(connectButton, newServerID.serverID);
            }
            else
            {
                Debug.LogError($"Button with name '{CONNECT_BUTTON_NAME}' not found in UXML.");
            }
        }

        private void SetupDisconnectButton(VisualElement root)
        {
            Button disconnectButton = root.Q<Button>(DISCONNECT_BUTTON_NAME);
            if (disconnectButton != null)
            {
                disconnectButton.clicked += () =>
                {
                    OnClickedDisconnectButton();
                };

                //Hide disconnect button by default
                disconnectButton.style.display = DisplayStyle.None;
            }
            else
            {
                Debug.LogError($"Button with name '{DISCONNECT_BUTTON_NAME}' not found in UXML.");
            }
        }

        private void SetupTextField(VisualElement root)
        {
            TextField textField = root.Q<TextField>(TEXT_FIELD_NAME);
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

                    //Check if length of the server ID is 9 char
                    ValidateServerID(root.Q<Button>(CONNECT_BUTTON_NAME), newServerID.serverID);
                });

                //Initial validation check if length of the server ID is 9 char
                ValidateServerID(root.Q<Button>(CONNECT_BUTTON_NAME), newServerID.serverID);

                //Set initial editable state
                textField.SetEnabled(!isConnected);
            }
            else
            {
                Debug.LogError($"TextField with name '{TEXT_FIELD_NAME}' not found in UXML.");
            }
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
            TextField textField = root.Q<TextField>(TEXT_FIELD_NAME);

            if (connectButton != null)
            {
                connectButton.style.display = isConnected ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (disconnectButton != null)
            {
                disconnectButton.style.display = isConnected ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (textField != null)
            {
                textField.SetEnabled(!isConnected);
            }
        }

        private void ValidateServerID(Button connectButton, string serverID)
        {
            if (serverID.Length == 9)
            {
                connectButton.SetEnabled(true);
            }
            else
            {
                connectButton.SetEnabled(false);
            }
        }
    }

    [System.Serializable]
    public class RecLabServerID
    {
        public string serverID;
    }
}
