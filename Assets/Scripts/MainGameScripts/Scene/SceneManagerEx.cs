using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : Singleton<SceneManagerEx>
{
    private BaseScene PrevScene;
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }
	public void LoadScene(Define.Scene type)
    {
        //Managers.Clear();
        PrevScene = CurrentScene;
        CurrentScene.Clear();
        SceneManager.LoadScene(GetSceneName(type));
    }

    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public Define.Scene GetCurrentScene()
    {
        return CurrentScene.SceneType;
    }

    public Define.Scene GetPrevScene()
    {
        return PrevScene.SceneType;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
