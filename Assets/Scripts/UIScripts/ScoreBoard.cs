using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    public static ScoreBoard instance;
    public Text blueTeamText;
    public Text redTeamText;

    public int blueTeamScore = 0;
    public int redTeamScore = 0;

    public GameObject blueTeamWinPanel;
    public GameObject redTeamWinPanel;
    public GameObject blueTeamLosePanel;
    public GameObject redTeamLosePanel;
    public GameObject controllerCanvas;

    PhotonView view;

    private void Awake()
    {
        controllerCanvas.SetActive(true);
        instance = this;
        view = GetComponent<PhotonView>();
    }

    public void PlayerDied(int playerTeam)
    {
        if(playerTeam == 2)
        {
            blueTeamScore++;

        }

        if(playerTeam == 1)
        {
            redTeamScore++;
        }

        view.RPC("UpdateScores", RpcTarget.All, blueTeamScore,redTeamScore);

        if (blueTeamScore >= 40)
        {
            view.RPC("HandleGameEnd", RpcTarget.All, 2);
        }
        else if (redTeamScore >= 40)
        {
            view.RPC("HandleGameEnd", RpcTarget.All, 1);
        }
    }

    [PunRPC]
    void UpdateScores(int blueScore, int redScore)
    {
        blueTeamScore = blueScore;
        redTeamScore = redScore;

        blueTeamText.text = blueScore.ToString();
        redTeamText.text = redScore.ToString();

    }

    [PunRPC]
    void HandleGameEnd(int winningTeam)
    {
        if (winningTeam == 2)
        {
            blueTeamWinPanel.SetActive(true);
            redTeamLosePanel.SetActive(true);
            controllerCanvas.SetActive(false);
        }
        else if (winningTeam == 1)
        {
            redTeamWinPanel.SetActive(true);
            blueTeamLosePanel.SetActive(true);
            controllerCanvas.SetActive(false);
        }

        // Optionally, disable further score updates and interactions
        // DisableGameplay();
    }
}
