using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using Theta.Unity.Runtime;

namespace Theta.Unity.Editor.Aby
{
    /// <summary>
    /// TODO
    /// </summary>
    public class AbyRuntimeMonitorEditorWindow : EditorWindow
    {
        /// <summary>
        /// TODO
        /// </summary>
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        /// <summary>
        /// TODO
        /// </summary>
        [MenuItem("Theta/Aby Runtime Monitor")]
        public static void ShowExample()
        {
            var abyMonitorWindow = GetWindow<AbyRuntimeMonitorEditorWindow>();
            abyMonitorWindow.titleContent = new GUIContent("Aby Monitor");
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void CreateGUI()
        {
            var uxmlContent = m_VisualTreeAsset.Instantiate();
            rootVisualElement.Add(uxmlContent);

            CreateMastheadGUI();
            CreateEnvironmentMenu();
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void CreateMastheadGUI()
        {
            var viewport = rootVisualElement.Q<VisualElement>("Viewport");
            if (viewport == null)
            {
                Debug.LogError("Viewport element not found!");
            }
            else
            {
                var label = new Label("TODO: Setup log stream from Rust-side ..");
                viewport.Add(label);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void CreateEnvironmentMenu()
        {
            var envMenu = rootVisualElement.Q<ToolbarMenu>("EnvironmentMenu");
            if (envMenu == null)
            {
                Debug.LogError("EnvironmentMenu element not found!");
            }
            else
            {
                envMenu.text = $"Active: Dev";
                envMenu.menu.AppendAction("Dev", OnEnvironmentMenuChange, DropdownMenuAction.Status.Checked);
                envMenu.menu.AppendAction("QA", OnEnvironmentMenuChange, DropdownMenuAction.Status.Normal);
                envMenu.menu.AppendAction("Prod", OnEnvironmentMenuChange, DropdownMenuAction.Status.Normal);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="option"></param>
        private void OnEnvironmentMenuChange(DropdownMenuAction option)
        {
            if (option.status == DropdownMenuAction.Status.Checked)
            {
                Debug.LogFormat("Environment `{0}` Already Selected", option.name);

                // TODO: Trigger re-fetch of Envs from the project.
            }
            else
            {
                Debug.LogFormat("Environment Changed to `{0}`", option.name);

                // TODO: Set the current environment.
            }
        }
    }
}
