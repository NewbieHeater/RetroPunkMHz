using UnityEngine;

public class Channel : MonoBehaviour
{
    public GameObject ChannelPanel;

    private bool isChannelOpen = false;
    private void Start()
    {
        ChannelPanel.SetActive(false);
    }
    public void ToggleMenu()
    {
        isChannelOpen = !isChannelOpen;
        ChannelPanel.SetActive(isChannelOpen);
    }
}