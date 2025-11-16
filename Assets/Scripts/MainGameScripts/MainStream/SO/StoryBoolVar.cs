using UnityEngine;

[CreateAssetMenu(menuName = "Story/Bool Variable")]
public class StoryBoolVar : StoryVariable
{
    [SerializeField] private bool value;
    public bool Value
    {
        get => value;
        set => this.value = value;
    }
}
