using UnityEngine;

[CreateAssetMenu(fileName = "ReverbData", menuName = "Reverb/Create New Reverb Data")]
public class ReverbData : ScriptableObject
{
    public string id;
    [TextArea(3, 10)]
    public string dialogue;
}
