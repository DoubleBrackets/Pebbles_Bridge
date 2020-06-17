using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class NetworkPlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public int playerId = 1;//Determines if player 1 or player 2
    public static GameObject LocalPlayerInstance;

    public GameObject hoverIndicatorObject;

    //Game controls variables
    private int selectedXIndex;
    private int selectedYIndex;

    private int firstXIndex;
    private int firstyIndex;

    private float unitsPerSquare = 2;

    public Color actionColor;
    private Color defaultColor;

    private void Awake()
    {
        if(photonView.IsMine && PhotonNetwork.IsConnected)
        {
            LocalPlayerInstance = gameObject;

            defaultColor = hoverIndicatorObject.GetComponent<MeshRenderer>().material.color;
            playerId = PhotonNetwork.CurrentRoom.PlayerCount;//First player gets p1(white)
            CenterCamera(playerId);
            Debug.Log("Player has been assigned id" + playerId);
            if (playerId == 2)
                ScoreboardScript.scoreboardScript.SwapNameText();
        }
        if(!PhotonNetwork.IsConnected)
        {
            CenterCamera(playerId);
            defaultColor = hoverIndicatorObject.GetComponent<MeshRenderer>().material.color;
        }   
    }

    private void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        //Square highlighting
        Plane p = new Plane(Vector3.up, 0);
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        if (p.Raycast(mouseRay, out dist))
        {
            Vector3 mousePos = mouseRay.GetPoint(dist);

            float xSquare = Mathf.Clamp(Mathf.Round(mousePos.x / unitsPerSquare), 0, 7);
            float zSquare = Mathf.Clamp(Mathf.Round(mousePos.z / unitsPerSquare), 0, 7);
            hoverIndicatorObject.transform.position = new Vector3(xSquare * unitsPerSquare, 0, zSquare * unitsPerSquare);
            selectedXIndex = (int)xSquare;
            selectedYIndex = 7 - (int)zSquare;
        }
        //Controls
        if (!GameManager.gameManager.GetIsUpdatingPositions())
        {
            if (UserInput())
                GameManager.gameManager.UpdateGamePiecePositions();
        }


    }

    bool UserInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            firstXIndex = selectedXIndex;
            firstyIndex = selectedYIndex;
            hoverIndicatorObject.GetComponent<MeshRenderer>().material.color = actionColor;
        }
        if (Input.GetMouseButtonUp(0))
        {
            hoverIndicatorObject.GetComponent<MeshRenderer>().material.color = defaultColor;
            float angle = Mathf.Atan2(selectedYIndex - firstyIndex, selectedXIndex - firstXIndex);
            int xChange = Mathf.RoundToInt(Mathf.Cos(angle));
            int yChange = Mathf.RoundToInt(Mathf.Sin(angle));

            if (yChange == 1 && GameManager.gameManager.turn == 2 && playerId == 2 && firstyIndex == 0)//Pulling(p2)
            {
                if (xChange == 0)//p2 Pulls column
                {
                    GameManager.gameManager.PullColumn(firstXIndex);
                    if (GameManager.gameManager.methodReturnContainer)
                    {
                        GameManager.gameManager.PlaceStone(0, firstXIndex, 2, 0, -1);
                        if (GameManager.gameManager.GetP1StoneCount() > 0)
                            GameManager.gameManager.SetTurn(1);
                        return true;
                    }
                }
                else 
                {
                    GameManager.gameManager.PullDiagonal(firstXIndex, xChange);
                    if(GameManager.gameManager.methodReturnContainer)
                    {
                        GameManager.gameManager.PlaceStone(0, firstXIndex, 2, -xChange, -1);
                        if (GameManager.gameManager.GetP1StoneCount() > 0)
                            GameManager.gameManager.SetTurn(1);
                        return true;
                    }
                }
            }
            else if (yChange == -1 && GameManager.gameManager.turn == 1 && playerId == 1 && firstyIndex == 7)//Pushing(p1)
            {
                if (xChange == 0)
                {
                    GameManager.gameManager.PushColumn(firstXIndex);
                    if (GameManager.gameManager.methodReturnContainer)
                    {
                        GameManager.gameManager.PlaceStone(7, firstXIndex, 1, 0, 1);
                        if (GameManager.gameManager.GetP2StoneCount() > 0)
                            GameManager.gameManager.SetTurn(2);
                        return true;
                    }
                }
                else 
                {
                    GameManager.gameManager.PushDiagonal(firstXIndex, xChange);
                    if(GameManager.gameManager.methodReturnContainer)
                    {
                        GameManager.gameManager.PlaceStone(7, firstXIndex, 1, -xChange, 1);
                        if (GameManager.gameManager.GetP2StoneCount() > 0)
                            GameManager.gameManager.SetTurn(2);
                        return true;
                    }
                }
            }

        }
        return false;
    }

    void CenterCamera(int player)
    {
        Camera.main.transform.position = new Vector3(3.5f * unitsPerSquare, 18, 3.5f * unitsPerSquare);
        Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90, (player-1) * 180, 0));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(unitsPerSquare);
        }
        else
        {
            unitsPerSquare = (float)stream.ReceiveNext();
        }
    }
}
