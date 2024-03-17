using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipManager", menuName = "Audio/AudioClip Manager", order = 1)]
public class AudioClipManager : ScriptableObject
{
    [System.Serializable]
    public class ClipData
    {
        public string name;
        public AudioClip clip;
        public float startTime;
        public float endTime;
    }

    public ClipData[] clips;

    public AudioClip GetClipByName(string clipName)
    {
        foreach (var clipData in clips)
        {
            if (clipData.name == clipName) return clipData.clip;
        }
        Debug.LogWarning($"Clip named {clipName} not found.");
        return null;
    }

    // You might want to add more utility methods here, for example, to get clips by some criteria, manipulate clips, etc.
}
