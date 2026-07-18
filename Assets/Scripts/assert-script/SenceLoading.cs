using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// SL：UI交互层 → 只负责界面、数据收集、场景跳转
/// 不写任何联网逻辑，全部交给NL
/// </summary>
public class SenceLoading : MonoBehaviour
{
    [Header("输入框")]
    public InputField input_Account;
    public InputField input_Pwd;

    [Header("按钮")]
    public Button btn_Register;
    public Button btn_Login;

    [Header("错误提示")]
    public Text txt_ErrorTip;

    void Start()
    {
        btn_Register.onClick.AddListener(OnClickRegister);
        btn_Login.onClick.AddListener(OnClickLogin);
    }

    void OnDestroy()
    {
        btn_Register.onClick.RemoveListener(OnClickRegister);
        btn_Login.onClick.RemoveListener(OnClickLogin);
    }

    #region 注册逻辑
    void OnClickRegister()
    {
        btn_Register.interactable = false;

        string account = input_Account.text.Trim();
        string pwd = input_Pwd.text.Trim();

        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(pwd))
        {
            SetErrorTip("账号/密码不能为空");
            btn_Register.interactable = true;
            return;
        }

        NetLoad.Instance.DoRegister(account, pwd, (isSuccess, msg) =>
        {
            btn_Register.interactable = true;

            if (isSuccess)
            {
                Debug.Log($"[注册成功] {msg}");
                SetErrorTip("注册成功，请登录");
            }
            else
            {
                Debug.LogError($"[注册失败] {msg}");
                SetErrorTip(msg);
            }
        });
    }
    #endregion

    #region 登录逻辑
    void OnClickLogin()
    {
        btn_Login.interactable = false;

        string account = input_Account.text.Trim();
        string pwd = input_Pwd.text.Trim();

        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(pwd))
        {
            SetErrorTip("账号/密码不能为空");
            btn_Login.interactable = true;
            return;
        }

        if (NetLoad.Instance == null)
        {
            SetErrorTip("网络层未初始化");
            btn_Login.interactable = true;
            return;
        }

        NetLoad.Instance.DoLogin(account, pwd, (isSuccess, msg) =>
        {
            btn_Login.interactable = true;

            if (isSuccess)
            {
                Debug.Log($"[登录成功] {msg}");
                SceneManager.LoadScene("home");
            }
            else
            {
                Debug.LogError($"[登录失败] {msg}");
                SetErrorTip(msg);
            }
        });
    }
    #endregion

    void SetErrorTip(string tip)
    {
        if (txt_ErrorTip != null)
        {
            txt_ErrorTip.text = tip;
            CancelInvoke(nameof(ClearTip));
            Invoke(nameof(ClearTip), 3f);
        }
    }

    void ClearTip()
    {
        if (txt_ErrorTip != null)
            txt_ErrorTip.text = "";
    }
}
