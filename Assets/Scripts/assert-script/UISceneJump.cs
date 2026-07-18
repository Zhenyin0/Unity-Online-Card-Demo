using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UISceneJump : MonoBehaviour
{
    // 跳到大地图
    public void ToMapScene()
    {
        SceneManager.LoadScene("map");
    }
    
    public void ToJieSuanScene()
    {
        SceneManager.LoadScene("JieSuan");
    }

    // 跳到联机名单
    public void ToLianjiScene()
    {
        SceneManager.LoadScene("LianjiMingDan");
    }

    // 跳回登录界面
    public void ToLoadScene()
    {
        SceneManager.LoadScene("Load");
    }

    //跳回主界面
    public void ToHomeScene() 
    {
        SceneManager.LoadScene("home");
    }

    public void ToBattleScene()
    {
        SceneManager.LoadScene("battle");
    }

    public void ToFightScene()
    {
        SceneManager.LoadScene("fight");
    }

    public void ToSelectMode()
    {
        SceneManager.LoadScene("selectmodule");
    }

    public void ToOtherMap()
    {
        SceneManager.LoadScene("otherMap");
    }

    // 退出游戏（关闭整个程序）
    public void ToExitGame()
    {
        Application.Quit();
    }
}

