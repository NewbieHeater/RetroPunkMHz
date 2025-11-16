using UnityEngine;

[CreateAssetMenu(menuName = "Story/Int Variable")]
public class StoryIntVar : StoryVariable
{
    [SerializeField] private int value;
    public int Value
    {
        get => value;
        set => this.value = value;
    }
}
