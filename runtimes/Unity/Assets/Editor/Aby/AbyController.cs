using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;

using Theta.Unity.Runtime;
using UnityEngine.SceneManagement;

namespace Theta.Unity.Editor.Aby
{
    /// <summary>
    /// TODO
    /// </summary>\
    [InitializeOnLoad]
    public class AbyControllerEditorWindow : EditorWindow
    {
        /// <summary>
        /// TODO
        /// </summary>
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

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
            StartJsRuntime(8080);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void OnReloadButtonClicked()
        {
            Debug.Log("Attempting to reload plugin.");

            // EditorApplication.delayCall
            EditorApplication.ExitPlaymode();

            EditorApplication.delayCall += DelayedOnReloadButtonClicked;
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void DelayedOnReloadButtonClicked()
        {
            EditorApplication.delayCall -= DelayedOnReloadButtonClicked;

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            if (ConfirmEditorRestart())
            {
                EditorApplication.Exit(100); // Request reload.
            }
        }

        //--
        /// <summary>
        /// TODO
        /// </summary>
        private void StartJsRuntime(int portNumber)
        {
            var startExitCode = JsRuntime.Start(portNumber);
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
