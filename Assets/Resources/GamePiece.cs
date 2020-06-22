using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;

public class GamePiece
{
    private float unitsPerSquare = 2;
    public GamePiece(GameObject gameObject, int player, int xSquare, int ySquare)
    {
        this.pieceGameObject = gameObject;
        this.player = player;
        this.xSquare = xSquare;
        this.ySquare = ySquare;
    }

    public void RemovePiece(int yIndex, int xIndex)
    {
        Vector3 pos = new Vector3(xIndex * unitsPerSquare, 0, (7 - yIndex) * unitsPerSquare);
        GamePieceHelperClass helper = GamePieceHelperClass.gamePieceHelperClass;
        if (yIndex > 7 && player == 1)
        {
            if (GameManager.gameManager == null)
                GameManagerSharedDevice.gameManagerSharedDevice.IncrementStoneCount(1);
            else
                GameManager.gameManager.IncrementStoneCount(1);
            helper.ReturnPiece(pieceGameObject,pos);
        }
        else if (yIndex < 0 && player == 2)
        {
            if (GameManager.gameManager == null)
                GameManagerSharedDevice.gameManagerSharedDevice.IncrementStoneCount(2);
            else
                GameManager.gameManager.IncrementStoneCount(2);
            helper.ReturnPiece(pieceGameObject,pos);
        }
        else
        {
            helper.DeletePiece(pieceGameObject,pos);
        }
    }

    

    

    public GameObject pieceGameObject;
    public int player;
    public int ySquare;
    public int xSquare;
}

