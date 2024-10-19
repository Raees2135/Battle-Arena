using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerUsernameManager : MonoBehaviour
{
    [SerializeField] InputField usernameInput;
    [SerializeField] Text errorMessage;

    private void Start()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            usernameInput.text = PlayerPrefs.GetString("username");
            PhotonNetwork.NickName = PlayerPrefs.GetString("username");

        }
    }

    public void playerUsernameInputValueChanged()
    {
        string userName = usernameInput.text;

        if(!string.IsNullOrEmpty(userName) && userName.Length <= 20)
        {
            PhotonNetwork.NickName = userName;
            PlayerPrefs.SetString("username", userName);

            errorMessage.text = " ";
            MenuManager.instance.OpenMenu("TitleMenu");
        }
        else
        {
            errorMessage.text = "Username must not be empty and should be 20 characters or less";
        }
    }
}
