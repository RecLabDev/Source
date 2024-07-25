using UnityEngine;
using TMPro;

public class PlayerLabel : MonoBehaviour
{
    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent == null)
        {
            Debug.LogError("TMP_Text component not found on the GameObject.");
        }
    }

    public void SetLabel(string playerName, Color bannerColor)
    {
        if (textComponent != null)
        {
            textComponent.text = playerName;
            textComponent.color = bannerColor;
        }
    }
}
