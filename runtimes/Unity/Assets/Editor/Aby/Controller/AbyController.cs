using System.ComponentModel;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;

using Aby.Unity.Plugin;

namespace Aby.Unity.Editor.Aby
{
    /// <summary>
    /// TODO
    /// </summary>
    [InitializeOnLoad]
    public class AbyControllerEditorWindow : EditorWindow
    {
        /// <summary>
        /// TODO
        /// </summary>
        [System.Serializable]
        public class State
        {
            /// <summary>
            /// TODO
            /// </summary>
            [SerializeField]
            public string Status;
        }

        /// <summary>
        /// TODO
        /// </summary>
        const int DEFAULT_SERVICE_PORT = 9000;

        /// <summary>
        /// The expected Exit Code used to ask the dev script to reload
        /// the window, instead of just shutting down.
        /// </summary>
        const int RELOAD_CLIENT_EXIT_CODE = 100;

        /// <summary>
        /// TODO
        /// </summary>
        [SerializeField]
        private State state = new State();

        /// <summary>
        /// TODO
        /// </summary>
        [SerializeField]
        private AbyExecutor m_ServiceConfig;

        /// <summary>
        /// TODO
        /// </summary>
        [SerializeField]
        private VisualTreeAsset visualTreeAsset = default;

        /// <summary>
        /// TODO
        /// </summary>
        // private readonly State m_BindingContext = new State();

        /// <summary>
        /// TODO
        /// </summary>
        static AbyControllerEditorWindow()
        {
            // Mount a heirarchy gui event handler.
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        }

        /// <summary>
        /// TODO
        /// </summary>
        [MenuItem("Aby/Aby Runtime Controller")]
        public static void ShowWindow()
        {
            var abyControllerWindow = GetWindow<AbyControllerEditorWindow>();
            abyControllerWindow.titleContent = new GUIContent("Aby Controller");
        }

        //--
        /// <summary>
        /// TODO
        /// </summary>
        private void Awake()
        {
            // Mount environment assets and pick one to use with the editor window.
            // TODO: Move this to a static value on AbyEnvConfig itself.
            var envConfigs = AssetDatabase.FindAssets("t:AbyEnv");
            if (envConfigs.Length == 0)
            {
                Debug.LogWarning("Couldn't find Aby Environment configs.");
            }
            else
            {
                var envConfigPath = AssetDatabase.GUIDToAssetPath(envConfigs[0]);
                m_ServiceConfig = AssetDatabase.LoadAssetAtPath<AbyExecutor>(envConfigPath);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void OnDestroy()
        {
            // TODO
        }

        //--
        /// <summary>
        /// Mounts the root `VisualElement` and inits the start/reload buttons.
        /// </summary>
        public void CreateGUI()
        {
            if (visualTreeAsset != null)
            {
                rootVisualElement.Add(visualTreeAsset.Instantiate());
                DrawRuntimeControlToolbar();
            }
            else
            {
                Debug.LogError("VisualTreeAsset is not assigned.");
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void DrawRuntimeControlToolbar()
        {
            var stateLabel = rootVisualElement.Q<Label>("RuntimeState");
            if (stateLabel == null)
            {
                Debug.LogWarning("RuntimeState element not found ..");
            }
            else
            {
                // TODO: Get the current state from the selected runtime instance.
                stateLabel.text = $"Runtime State: TODO";
            }

            var toggleButton = rootVisualElement.Q<Button>("ToggleButton");
            if (toggleButton == null)
            {
                Debug.LogError("ToggleButton element not found!");
            }
            else
            {
                toggleButton.clicked += OnToggleButtonClicked;
            }


            var reloadButton = rootVisualElement.Q<Button>("ReloadButton");
            if (reloadButton == null)
            {
                Debug.LogError("ReloadButton element not found!");
            }
            else
            {
                reloadButton.clicked += OnReloadButtonClicked;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void OnGUI()
        {
            var toggleButton = rootVisualElement.Q<Button>("ToggleButton");
            if (toggleButton != null)
            {
                // TODO: Something like `selectedRuntime.IsAlive == false ? "Start" : "Stop"`
                toggleButton.text = "Toggle";
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            // Debug.LogFormat("Found GUI item: {0}", instanceID);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void OnToggleButtonClicked()
        {
            // TODO: Start or stop the runtime, depending on its state ..
            // if (!abyRuntime.IsAlive)
            // {
            //     abyRuntime.StartServiceThread();
            // }
            // else
            // {
            //     abyRuntime.StopServiceThread();
            // }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void OnReloadButtonClicked()
        {
            Debug.Log("Attempting to reload plugin.");

            // We need to exit play mode first so we can save and safely
            // run shutdown operations on Aby's managed threads.
            EditorApplication.ExitPlaymode();

            // The call to `ExitPlaymode` above doesn't complete until "later",
            // so we defer the rest of the operation until then.
            EditorApplication.delayCall += DelayedOnReloadButtonClicked;
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void DelayedOnReloadButtonClicked()
        {
            EditorApplication.delayCall -= DelayedOnReloadButtonClicked;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                if (ConfirmEditorRestart())
                {
                    EditorApplication.Exit(RELOAD_CLIENT_EXIT_CODE); // Request reload.
                }
            }
        }

        //--
        /// <summary>
        /// TODO
        /// </summary>
        private bool ConfirmEditorRestart()
        {
            return EditorUtility.DisplayDialog(
                "Restart Editor?",
                $"You'll need to restart the editor before changes take effect.",
                "Yes pls! (Recommended)",
                "No thx."
            );
        }
    }
}
