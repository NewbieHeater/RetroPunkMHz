using System.Collections.Generic;
using UnityEngine;

public enum EpisodePhase
{
    Start,
    Middle,
    End
}

[CreateAssetMenu(menuName = "Story/Main Stream Episode")]
public class MainStreamEpisode : ScriptableObject
{
    public string episodeId;

    [Header("에피소드 안의 상태들")]
    public List<MainStreamStateObj> states;

    [Header("역할 지정")]
    public MainStreamStateObj startState;
    public List<MainStreamStateObj> middleStates;
    public MainStreamStateObj endState;
}
