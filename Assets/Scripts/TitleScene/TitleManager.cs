using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public static TitleManager Instance { get; private set; }

    public ButtonHovering selectedButton = null;
    private Animator CamAnim;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CamAnim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(selectedButton && Input.GetKeyDown(KeyCode.Escape))
        {
            selectedButton.Deselect();
            selectedButton = null;
        }
    }

    public void CamNewGame(bool flag)
    {
        CamAnim.SetBool("NewGame", flag);
    }
    public void StartNewGame()
    {
        SceneManager.LoadScene("sangyeon_workspace");
    }
}
