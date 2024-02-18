using System.ComponentModel;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;

using Theta.Unity.Runtime;

namespace Theta.Unity.Editor.Aby
{
    /// <summary>
    /// TODO
    /// </summary>\
    [InitializeOnLoad]
    public class AbyControllerEditorWindow : EditorWindow
    {
        /// <summary>
        /// The expected Exit Code used to ask the dev script to reload
        /// the window, instead of just shutting down.
        /// </summary>
        const int RELOAD_REQUEST_EXIT_CODE = 100;

        /// <summary>
        /// TODO
        /// </summary>
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

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
        private void OnDestroy()
        {
            // TODO
        }

        /// <summary>
        /// TODO
        /// </summary>
        [MenuItem("Theta/Aby Controller")]
        public static void ShowWindow()
        {
            var abyControllerWindow = GetWindow<AbyControllerEditorWindow>();
            abyControllerWindow.titleContent = new GUIContent("Aby Controller");
        }

        //--
        /// <summary>
        /// Mounts the root `VisualElement` and inits the start/reload buttons.
        /// </summary>
        public void CreateGUI()
        {
            var editorVisualTree = m_VisualTreeAsset.Instantiate();
            rootVisualElement.Add(editorVisualTree);

            SetupStartButton();
            SetupReloadButton();
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void OnGUI()
        {
            MountStatusDisplay();
        }

        /// <summary>
        /// TODO
        /// </summary>
        private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            // Debug.LogFormat("Found GUI item: {0}", instanceID);
        }

        //--
        /// <summary>
        /// TODO
        /// </summary>
        private void SetupStartButton()
        {
            var startBtn = rootVisualElement.Q<Button>("StartButton");
            if (startBtn == null)
            {
                Debug.LogError("StartButton element not found!");
            }
            else
            {
                startBtn.clicked += OnStartButtonClicked;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void SetupReloadButton()
        {
            var reloadBtn = rootVisualElement.Q<Button>("ReloadButton");
            if (reloadBtn == null)
            {
                Debug.LogError("StartButton element not found!");
            }
            else
            {
                reloadBtn.clicked += OnReloadButtonClicked;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void MountStatusDisplay()
        {
            var runtimeStateLabel = rootVisualElement.Q<Label>("RuntimeState");
            if (runtimeStateLabel == null)
            {
                Debug.LogWarning("RuntimeState element not found ..");
            }
            else
            {
                var runtimeState = Theta.Unity.Runtime.JsRuntime.GetState();
                runtimeStateLabel.text = $"Runtime State: {runtimeState}";
            }
        }

        //--
        /// <summary>
        /// TODO
        /// </summary>
        private void OnStartButtonClicked()
        {
            // TODO: Get the port from config and/or ui field.
            StartJsRuntime(9000);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void OnReloadButtonClicked()
        {
            Debug.Log("Attempting to reload plugin.");

            // We need to exit play mode first so we can (optionally) save
            // and safely run shutdown operations.
            EditorApplication.ExitPlaymode();

            // `ExitPlaymore` doesn't complete until "later", so we defer
            // the rest of the operation until then.
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
                    EditorApplication.Exit(RELOAD_REQUEST_EXIT_CODE); // Request reload.
                }
            }
        }

        //--
        /// <summary>
        /// TODO
        /// </summary>
        private void StartJsRuntime(int servicePortNumber)
        {
            var startExitCode = JsRuntime.Start(servicePortNumber);
            Debug.LogFormat("JsRuntime exited with code: {0}", startExitCode);
            Debug.LogFormat("JsRuntime State: {0}", JsRuntime.GetState());
        }

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
