using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;

using Theta.Unity.Runtime;

namespace Theta.Unity.Editor.Aby
{
    /// <summary>
    /// TODO
    /// </summary>
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
        /// TODO
        /// </summary>
        public void CreateGUI()
        {
            var uxmlContent = m_VisualTreeAsset.Instantiate();
            rootVisualElement.Add(uxmlContent);

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
            var runtimeStateLabel = rootVisualElement.Q<Label>("RuntimeStatus");
            if (runtimeStateLabel != null)
            {
                var runtimeState = Theta.Unity.Runtime.JsRuntime.GetState();
                runtimeStateLabel.text = $"Runtime Status: {runtimeState}";
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
            var startReport = JsRuntime.Start(portNumber);
            Debug.LogFormat("JsRuntime exited with report: {0}", startReport);
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
