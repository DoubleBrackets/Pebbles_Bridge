using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece
{
    public GamePiece(GameObject gameObject, int player, int xSquare, int ySquare)
    {
        this.pieceGameObject = gameObject;
        this.player = player;
        this.xSquare = xSquare;
        this.ySquare = ySquare;
    }

    public void RemovePiece(int yIndex, int xIndex)
    {
        if (yIndex > 6 && player == 1)
        {
            GameManager.gameManager.IncrementStoneCount(1);
        }
        else if (yIndex < 0 && player == 2)
        {
            GameManager.gameManager.IncrementStoneCount(2);
        }
        GameObject.Destroy(pieceGameObject);
    }

    public GameObject pieceGameObject;
    public int player;
    public int ySquare;
    public int xSquare;
}