using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class sceneLoader : MonoBehaviour
{
    public SceneAsset targetScene;

    public void LoadScene()
    {
        SceneManager.LoadScene(targetScene.name);
    }
}
