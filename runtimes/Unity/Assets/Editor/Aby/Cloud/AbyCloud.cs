using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AbyCloud : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Aby/Aby Cloud Dashboard")]
    public static void ShowExample()
    {
        AbyCloud wnd = GetWindow<AbyCloud>();
        wnd.titleContent = new GUIContent("Aby Cloud");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        var label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        var labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}
