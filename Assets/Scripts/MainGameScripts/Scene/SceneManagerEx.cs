using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : Singleton<SceneManagerEx>
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }
    public bool isGameSceneActive = false;
	public void LoadScene(Define.Scene type)
    {
        //Managers.Clear();
        //CurrentScene.Clear(); 하성 이거 때문에 씬 바꾸는거 오류나서 일단 주석처리를 했음 
        //그리고 플레이어 이동은 돈디스트로이 때문인거 같은데 이거는 플레이어 이동 스크립트에서 해결해야 할듯?
        SceneManager.LoadScene(GetSceneName(type));
    }

    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    Define.Scene GetCurrentScene()
    {
        return CurrentScene.SceneType;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
