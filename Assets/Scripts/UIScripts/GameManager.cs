using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public bool isMenuOpened = false;
    public GameObject menuUI;
    public GameObject scoreUI;

    public Button quitButton;
    public Button leaveButton;

    public GameObject controllerCanvas;

    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();

        quitButton.onClick.AddListener(QuitGame);
        leaveButton.onClick.AddListener(ToLeaveGame);
    }

    private void OnEnable()
    {
        playerControls.UI.Pause.performed += OnPause;
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.UI.Pause.performed -= OnPause;
        playerControls.Disable();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (!isMenuOpened)
        {
            scoreUI.SetActive(false);
            menuUI.SetActive(true);
            controllerCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isMenuOpened = true;
            AudioListener.pause = true;
        }
        else
        {
            scoreUI.SetActive(true);
            menuUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isMenuOpened = false;
            AudioListener.pause = false;
        }
    }

    public void LeaveGame()
    {
        Debug.Log("Leaving game...");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room, loading main menu...");
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon, loading main menu...");
        SceneManager.LoadScene(0); // Load the main menu scene (assuming it's the first scene in the build settings)
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void ToLeaveGame()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
        Debug.Log("Leaving game...");
    }

}
