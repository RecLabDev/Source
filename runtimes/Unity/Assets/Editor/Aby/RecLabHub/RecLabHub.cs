using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Aby.Unity
{
    public class RecLabHub : EditorWindow
    {
        // UXML and USS
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private StyleSheet styleSheet;

        // Server ID
        private int serverIDLength = 36;
        private RecLabServerID newServerID;
        private const string SERVER_ID_PREF_KEY = "RecLabHub_ServerID";

        // UI element references
        private TextField textField;
        private Label dockerLabel;
        private Label statusLabel;
        private VisualElement remoteDockerCluster;

        // UI Elements
        private const string WINDOW_TITLE = "RecLab Hub";
        private const string TEXT_FIELD_NAME = "ServerEntry";
        private const string BACKGROUND_CONTAINER = "Background";
        private const string BUILD_BUTTON_NAME = "BuildButton";
        private const string CANCEL_DOCKER_BUTTON_NAME = "CancelDocker";
        private const string CANCEL_SERVER_BUTTON_NAME = "CancelServer";
        private const string STOP_BUTTON_NAME = "StopButton";
        private const string START_BUTTON_NAME = "StartButton";
        private const string DISCONNECT_BUTTON_NAME = "DisconnectButton";
        private const string DOCKER_LABEL_NAME = "DockerStatus";
        private const string STATUS_LABEL_NAME = "StatusText";
        private const string REMOTE_DOCKER_CLUSTER_NAME = "RemoteDockerCluster";

        // Docker build status
        private bool isBuilt = false;
        
        // Docker build state management
        private enum DockerState { Idle, Building, Built, Live, Stopping }
        private DockerState dockerState = DockerState.Idle;
        private float dockerStateStartTime;
        private const float StateTransitionDuration = 1.5f;

        // Track connection status
        private bool isConnected = false;

        // Connection state management
        private enum ConnectionState { Idle, Connecting, Connected, Disconnecting }
        private ConnectionState currentState = ConnectionState.Idle;
        private float connectionStateStartTime;

        [MenuItem("Aby/RecLab Hub")]
        public static void ShowWindow()
        {
            RecLabHub wnd = GetWindow<RecLabHub>();
            wnd.titleContent = new GUIContent(WINDOW_TITLE);
        }

        public void CreateGUI()
        {
            // Create empty server ID reference, or load one from a previous session
            InitializeServerID();

            VisualElement root = rootVisualElement;

            // Add USS and UXML to the editor window
            AddStyleSheet(root);
            AddUXML(root);

            // Make the background image stretch with window
            SyncWindowSize();

            // Setup UI elements
            SetupUIElements(root);

            // Set the initial status label text
            statusLabel.text = "Status: Not Built";

            // Update button visibility based on connection status
            UpdateButtonVisibility();

            // Start updating the editor window
            EditorApplication.update += OnEditorUpdate;
        }

        public void OnGUI()
        {
            SyncWindowSize();
        }

        private void InitializeServerID()
        {
            newServerID = new RecLabServerID();
            // Load saved server ID, if available
            newServerID.serverID = EditorPrefs.GetString(SERVER_ID_PREF_KEY, string.Empty);
        }

        private void AddStyleSheet(VisualElement root)
        {
            try
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
            catch (System.Exception ex)
            {
                Debug.LogError($"Error adding style sheet: {ex.Message}");
            }
        }

        private void AddUXML(VisualElement root)
        {
            try
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
            catch (System.Exception ex)
            {
                Debug.LogError($"Error adding UXML: {ex.Message}");
            }
        }

        private void SetupUIElements(VisualElement root)
        {
            SetupButton<Button>(root, BUILD_BUTTON_NAME, OnClickedBuildButton);
            SetupButton<Button>(root, CANCEL_DOCKER_BUTTON_NAME, OnClickedCancelDockerButton);
            SetupButton<Button>(root, CANCEL_SERVER_BUTTON_NAME, OnClickedCancelServerButton);
            SetupButton<Button>(root, STOP_BUTTON_NAME, OnClickedStopButton, false); // Ensure stopButton is set up here
            SetupButton<Button>(root, START_BUTTON_NAME, () => OnClickedStartButton(newServerID.serverID));
            SetupButton<Button>(root, DISCONNECT_BUTTON_NAME, OnClickedDisconnectButton, false);
            SetupTextField(root);
            SetupDockerLabel(root);
            SetupStatusLabel(root);
            SetupRemoteDockerCluster(root);
        }

        private void SetupButton<T>(VisualElement root, string buttonName, System.Action onClick, bool initialVisibility = true) where T : Button
        {
            T button = root.Q<T>(buttonName);
            if (button != null)
            {
                button.clicked += () => onClick();
                button.style.display = initialVisibility ? DisplayStyle.Flex : DisplayStyle.None;
                Debug.Log($"{buttonName} initialized: {button != null}, initial visibility: {button.style.display}");
            }
            else
            {
                Debug.LogError($"Button with name '{buttonName}' not found in UXML.");
            }
        }

        private void SetupTextField(VisualElement root)
        {
            textField = root.Q<TextField>(TEXT_FIELD_NAME);
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

                    // Check if length of the server ID is correct
                    ValidateServerID(root.Q<Button>(START_BUTTON_NAME), newServerID.serverID);
                });

                // Initial validation check if length of the server ID is correct
                ValidateServerID(root.Q<Button>(START_BUTTON_NAME), newServerID.serverID);

                // Set initial editable state
                textField.SetEnabled(!isConnected);
            }
            else
            {
                Debug.LogError($"TextField with name '{TEXT_FIELD_NAME}' not found in UXML.");
            }
        }

        private void SetupDockerLabel(VisualElement root)
        {
            dockerLabel = root.Q<Label>(DOCKER_LABEL_NAME);
            if (dockerLabel != null)
            {
                // Do something
            }
            else
            {
                Debug.LogError($"Label with name '{DOCKER_LABEL_NAME}' not found in UXML.");
            }
        }

        private void SetupStatusLabel(VisualElement root)
        {
            statusLabel = root.Q<Label>(STATUS_LABEL_NAME);
            if (statusLabel != null)
            {
                // Do something
            }
            else
            {
                Debug.LogError($"Label with name '{STATUS_LABEL_NAME}' not found in UXML.");
            }
        }

        private void SetupRemoteDockerCluster(VisualElement root)
        {
            remoteDockerCluster = root.Q<VisualElement>(REMOTE_DOCKER_CLUSTER_NAME);
            if (remoteDockerCluster != null)
            {
                remoteDockerCluster.SetEnabled(false);
            }
            else
            {
                Debug.LogError($"Element with name '{REMOTE_DOCKER_CLUSTER_NAME}' not found in UXML.");
            }
        }

        // Button Events
        private void OnClickedBuildButton()
        {
            TransitionDockerState(DockerState.Building, "Building...");
            remoteDockerCluster.SetEnabled(false);
        }

        private void OnClickedCancelDockerButton()
        {
            TransitionDockerState(DockerState.Stopping, "Canceling...");
            remoteDockerCluster.SetEnabled(false);
        }

        private void OnClickedStopButton()
        {
            TransitionDockerState(DockerState.Stopping, "Stopping...");
            remoteDockerCluster.SetEnabled(false);
            if (isConnected)
            {
                OnClickedDisconnectButton();
            }
        }

        private void OnClickedStartButton(string serverID)
        {
            TransitionConnectionState(ConnectionState.Connecting, "Connecting...");
        }
        private void OnClickedCancelServerButton()
        {
            TransitionConnectionState(ConnectionState.Disconnecting, "Canceling...");
        }

        private void OnClickedDisconnectButton()
        {
            TransitionConnectionState(ConnectionState.Disconnecting, "Disconnecting...");
        }

        private void TransitionDockerState(DockerState newState, string dockerStatus)
        {
            dockerState = newState;
            dockerStateStartTime = (float)EditorApplication.timeSinceStartup;
            dockerLabel.text = $"Status: {dockerStatus}";
            statusLabel.style.display = DisplayStyle.Flex;
            Debug.Log($"TransitionDockerState: Transitioned to {newState} with status {dockerStatus}");
            UpdateButtonVisibility();
        }

        private void TransitionConnectionState(ConnectionState newState, string statusText)
        {
            currentState = newState;
            connectionStateStartTime = (float)EditorApplication.timeSinceStartup;
            statusLabel.text = $"Status: {statusText}";
            statusLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateButtonVisibility()
        {
            VisualElement root = rootVisualElement;

            Button buildButton = root.Q<Button>(BUILD_BUTTON_NAME);
            Button cancelDockerButton = root.Q<Button>(CANCEL_DOCKER_BUTTON_NAME);
            Button stopButton = root.Q<Button>(STOP_BUTTON_NAME);
            Button startButton = root.Q<Button>(START_BUTTON_NAME);
            Button cancelServerButton = root.Q<Button>(CANCEL_SERVER_BUTTON_NAME);
            Button disconnectButton = root.Q<Button>(DISCONNECT_BUTTON_NAME);
            TextField textField = root.Q<TextField>(TEXT_FIELD_NAME);

            if (buildButton != null)
            {
                buildButton.style.display = isBuilt ? DisplayStyle.None : DisplayStyle.Flex;
                Debug.Log($"buildButton visibility: {buildButton.style.display}");
            }

            if (cancelDockerButton != null)
            {
                // Show the cancel button during both the "Building" and "Built" states for the local Docker cluster
                cancelDockerButton.style.display = (dockerState == DockerState.Building || dockerState == DockerState.Built) ? DisplayStyle.Flex : DisplayStyle.None;
                if(dockerState == DockerState.Built)
                {
                    cancelDockerButton.SetEnabled(false);
                }
                Debug.Log($"cancelDockerButton visibility: {cancelDockerButton.style.display}");
            }

            if (stopButton != null)
            {
                stopButton.style.display = isBuilt ? DisplayStyle.Flex : DisplayStyle.None;
                Debug.Log($"stopButton visibility: {stopButton.style.display}");
            }

            if (startButton != null)
            {
                startButton.style.display = isConnected ? DisplayStyle.None : DisplayStyle.Flex;
                Debug.Log($"startButton visibility: {startButton.style.display}");
            }

            if(cancelServerButton != null)
            {
                // Show the cancel button only during the "Connecting" state for the remote Docker cluster
                cancelServerButton.style.display = currentState == ConnectionState.Connecting ? DisplayStyle.Flex : DisplayStyle.None;
                if(currentState == ConnectionState.Connected)
                {
                    cancelServerButton.SetEnabled(false);
                }
                Debug.Log($"cancelServerButton visibility: {cancelServerButton.style.display}");
            }

            if (disconnectButton != null)
            {
                disconnectButton.style.display = (isConnected == true || currentState == ConnectionState.Disconnecting) ? DisplayStyle.Flex : DisplayStyle.None;
                if(currentState == ConnectionState.Disconnecting)
                {
                    disconnectButton.SetEnabled(false);
                }
                Debug.Log($"disconnectButton visibility: {disconnectButton.style.display}");
            }

            if (textField != null)
            {
                textField.SetEnabled(!isConnected);
            }

            if (statusLabel != null)
            {
                statusLabel.style.display = DisplayStyle.Flex;
                statusLabel.text = isConnected ? "Status: Connected" : "Status: Disconnected";
            }

            if (buildButton != null && stopButton != null)
            {
                Debug.Log($"UpdateButtonVisibility: DockerState = {dockerState}");
                buildButton.style.display = dockerState == DockerState.Idle ? DisplayStyle.Flex : DisplayStyle.None;
                stopButton.style.display = dockerState == DockerState.Live ? DisplayStyle.Flex : DisplayStyle.None;

                buildButton.SetEnabled(dockerState == DockerState.Idle);
                stopButton.SetEnabled(dockerState == DockerState.Live);

                Debug.Log($"buildButton visibility: {buildButton.style.display}");
                Debug.Log($"stopButton visibility: {stopButton.style.display}");

                // Force layout update
                buildButton.MarkDirtyRepaint();
                stopButton.MarkDirtyRepaint();
                root.MarkDirtyRepaint();
            }

            if (remoteDockerCluster != null)
            {
                remoteDockerCluster.SetEnabled(dockerState == DockerState.Live);
            }
        }

        private void OnEditorUpdate()
        {
            float elapsedTime = (float)EditorApplication.timeSinceStartup - dockerStateStartTime;

            switch (dockerState)
            {
                case DockerState.Building:
                    if (elapsedTime > StateTransitionDuration)
                    {
                        TransitionDockerState(DockerState.Built, "Built!");
                        isBuilt = true;
                    }
                    break;
                case DockerState.Built:
                    if (elapsedTime > StateTransitionDuration)
                    {
                        TransitionDockerState(DockerState.Live, "Live");
                        UpdateButtonVisibility();
                    }
                    break;
                case DockerState.Stopping:
                    if (elapsedTime > StateTransitionDuration)
                    {
                        isBuilt = false;
                        TransitionDockerState(DockerState.Idle, "Not Built");
                        UpdateButtonVisibility();
                    }
                    break;
            }

            elapsedTime = (float)EditorApplication.timeSinceStartup - connectionStateStartTime;

            switch (currentState)
            {
                case ConnectionState.Connecting:
                    if (elapsedTime > StateTransitionDuration)
                    {
                        TransitionConnectionState(ConnectionState.Connected, "Connected!");
                        isConnected = true;
                        UpdateButtonVisibility();
                    }
                    break;
                case ConnectionState.Connected:
                    if (elapsedTime > StateTransitionDuration)
                    {
                        currentState = ConnectionState.Idle;
                    }
                    break;
                case ConnectionState.Disconnecting:
                    if (elapsedTime > StateTransitionDuration)
                    {
                        isConnected = false;
                        TransitionConnectionState(ConnectionState.Idle, "Disconnected");
                        UpdateButtonVisibility();
                    }
                    break;
            }
        }

        private void ValidateServerID(Button startButton, string serverID)
        {
            if (serverID.Length == serverIDLength)
            {
                startButton.SetEnabled(true);
            }
            else
            {
                startButton.SetEnabled(false);
            }
        }

        private void SyncWindowSize()
        {
            var rootContainer = rootVisualElement.Q<VisualElement>(BACKGROUND_CONTAINER);
            if (rootContainer != null)
            {
                rootContainer.style.height = rootVisualElement.contentRect.height;
            }
        }
    }

    [System.Serializable]
    public class RecLabServerID
    {
        public string serverID;
    }
}
           