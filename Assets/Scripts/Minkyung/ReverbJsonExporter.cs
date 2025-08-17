#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class ReverbJsonExporter : EditorWindow
{
    [MenuItem("Tools/Reverb/Export All To JSON")]
    static void OpenWindow() => GetWindow<ReverbJsonExporter>("Reverb Exporter");

    void OnGUI()
    {
        if (GUILayout.Button("Export All ReverbData to StreamingAssets/Reverb"))
        {
            ExportAll();
        }
    }

    void ExportAll()
    {
        string outDir = Path.Combine(Application.dataPath, "StreamingAssets/Reverb");
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        string[] guids = AssetDatabase.FindAssets("t:ReverbData");
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            ReverbData data = AssetDatabase.LoadAssetAtPath<ReverbData>(path);
            if (data == null) continue;

            var dto = new ReverbDataDTO();
            dto.id = data.id;
            dto.title = data.title;
            dto.loop = data.loop;
            dto.reverbPreset = data.reverbPreset.ToString();
            dto.lines = new ReverbLineDTO[data.lines.Length];
            for (int i = 0; i < data.lines.Length; i++)
            {
                var l = data.lines[i];
                var ld = new ReverbLineDTO()
                {
                    speaker = l.speaker,
                    text = l.text,
                    audioClipPath = l.audioClip ? AssetDatabase.GetAssetPath(l.audioClip) : ""
                };
                dto.lines[i] = ld;
            }

            string json = JsonUtility.ToJson(dto, true);
            File.WriteAllText(Path.Combine(outDir, data.name + ".json"), json);
        }

        AssetDatabase.Refresh();
        Debug.Log("Export complete to: " + outDir);
    }


    [System.Serializable]
    private class ReverbDataDTO
    {
        public string id;
        public string title;
        public bool loop;
        public string reverbPreset;
        public ReverbLineDTO[] lines;
    }
    [System.Serializable]
    private class ReverbLineDTO
    {
        public string speaker;
        public string text;
        public string audioClipPath;
    }
}
#endif
