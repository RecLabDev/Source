using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Aby.Unity
{
    public class RecLabHub : EditorWindow
    {
        //UXML and USS
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private StyleSheet styleSheet;


        //UI Elements
        private const string WINDOW_TITLE = "RecLab Hub";
        private const string TEXT_FIELD_NAME = "TextField";
        private const string BACKGROUND_CONTAINER = "Background";
        //private const string CONNECT_BUTTON_NAME = "ConnectButton";
        //private const string DISCONNECT_BUTTON_NAME = "DisconnectButton";
        //private const string STATUS_LABEL_NAME = "ConnectionStatus";

        [MenuItem("Aby/RecLab Hub")]
        public static void ShowWindow()
        {
            RecLabHub wnd = GetWindow<RecLabHub>();
            wnd.titleContent = new GUIContent(WINDOW_TITLE);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            //Add USS and UXML to the editor window
            AddStyleSheet(root);
            AddUXML(root);

            //Make the background image stretch with window
            SyncWindowSize();
        }

        public void OnGUI()
        {
            SyncWindowSize();
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

        private void SyncWindowSize()
        {
            var rootContainer = rootVisualElement.Q<VisualElement>(BACKGROUND_CONTAINER);
            if (rootContainer != null)
            {
                rootContainer.style.height = rootVisualElement.contentRect.height;
            }
        }
    }
}
