using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
public class ScoreboardScript : MonoBehaviour
{
    public static ScoreboardScript scoreboardScript;

    private void Awake()
    {
        scoreboardScript = this;
    }

    public Text p1Score;
    public Text p2Score;

    public Text endGameText;
    public Color p1WinColor;
    public Color p2WinColor;
    public Color tieColor;

    public GameObject menuButton;
    public GameObject restartButton;

    public Text p1Name;
    public Text p2Name;

    public GameObject opponentDisconnected;

    public Color nameTextColor;


    public void UpdateScoreboard(int score1, int score2)
    {
        p1Score.text = "" + score1;
        p2Score.text = "" + score2;
    }

    private void Update()
    {
        float yTransform = Camera.main.transform.rotation.eulerAngles.y;
        p1Score.transform.rotation = Quaternion.Euler(90, yTransform, 0);
        p2Score.transform.rotation = Quaternion.Euler(90, yTransform, 0);
    }

    public void EndGame(int result)
    {
        if (result == 1)
        {
            endGameText.color = p1WinColor;
            endGameText.text = "Player 1 Wins :D";
        }
        else if(result == 2)
        {
            endGameText.color = p2WinColor;
            endGameText.text = "Player 2 Wins :D";
        }
        else
        {
            endGameText.color = tieColor;
            endGameText.text = "Tie :>";
        }
        if (PhotonNetwork.CurrentRoom != null)
            return;
        menuButton.SetActive(true);
        restartButton.SetActive(true);

    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }


    public void UpdateNameText(int player, string name)
    {
        p1Name.enabled = true;
        p2Name.enabled = true;
        if (player == 1)
            p1Name.text = name;
        else
            p2Name.text = name;
        float yTransform = Camera.main.transform.rotation.eulerAngles.y;
    }

    public void SetNameTextTurn(int player, bool val)
    {
        if (player == 1)
        {
            if (!val)
                p1Name.color = Color.white;
            else
                p1Name.color = nameTextColor;
        }
        else
        {
            if (!val)
                p2Name.color = Color.white;
            else
                p2Name.color = nameTextColor;
        }
    }

    public void SwapNameText()
    {
        Vector2 pos = p1Name.gameObject.transform.position;
        p1Name.gameObject.transform.position = p2Name.gameObject.transform.position;
        p2Name.gameObject.transform.position = pos;
    }

    public void ShowOpponentDisconnected()
    {
        opponentDisconnected.SetActive(true);
    }
}
