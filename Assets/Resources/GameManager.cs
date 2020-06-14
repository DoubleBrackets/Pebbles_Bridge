using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviour, IPunObservable
{

    //Manages the game logic for multiplayer scene

    /*
     * PERSPECTIVE REFERENCE
     * 
     * Player 2
     * 
     * Up ^
     * index [0,0]
     * <Left    Board   Right >
     * 
     *                         index [6,6]
     * Down  v
     * 
     * Player 1
     */



    public static GameManager gameManager;

    public GameObject stonePrefab1;
    public GameObject stonePrefab2;
    public GameObject gridPrefab;

    private GamePiece[,] board;

    private float unitsPerSquare = 2;

    public int turn = 1;//WHose tUrn

    //Controls

    private bool isUpdatingPositions = false;

    private int p1StoneCount;
    private int p2StoneCount;
    
    public int startStoneCount = 30;

    private bool gameEnded = false;

    public bool methodReturnContainer;

    private void Awake()
    {
        gameManager = this;
        board = new GamePiece[7,7];

        p1StoneCount = startStoneCount;
        p2StoneCount = startStoneCount;

        UpdateGamePiecePositions();
        //CreateGrid();
    }

    private void Update()
    {
        if (gameEnded)
            return;
        ScoreboardScript.scoreboardScript.UpdateScoreboard(p1StoneCount, p2StoneCount);
    }

    public void PullColumn(int columnIndex)
    {
        PhotonView pView = PhotonView.Get(this);
        pView.RPC("PullColumnMain", RpcTarget.AllBuffered, columnIndex);
    }

    [PunRPC]
    public bool PullColumnMain(int columnIndex)//Pulls a column one down
    {
        int upTo = CheckIfColumnPullable(columnIndex);//Pulls all squares up to this(if -1, then return false since column is not pullable)
        if (upTo == -1)
        {
            methodReturnContainer = false;
            return false;
        }


        if (board[6, columnIndex] != null && upTo == 6)//Last piece is pushed off board
        {
            board[6, columnIndex].RemovePiece(7, columnIndex);
            board[6, columnIndex] = null;
        }
        GamePiece[] newColumn = new GamePiece[7];

        for (int x = 1; x <= upTo; x++)
        {
            newColumn[x] = board[x - 1, columnIndex];
        }

        for (int x = 0; x <= upTo; x++)
        {
            board[x, columnIndex] = newColumn[x];
        }
        methodReturnContainer = true;
        return true;
    }

    public void PushColumn(int columnIndex)
    {
        PhotonView pView = PhotonView.Get(this);
        pView.RPC("PushColumnMain", RpcTarget.AllBuffered, columnIndex);
    }

    [PunRPC]
    public bool PushColumnMain(int columnIndex)//Pushes a column one up
    {
        int upTo = CheckIfColumnPushable(columnIndex);//Pushes all squares up to this(if -1, then return false since column is not pushable)
        if (upTo == -1)
        {
            methodReturnContainer = false;
            return false;
        }

        if (board[0,columnIndex] != null && upTo == 0)//LAst piece is pushed off board
        {
            board[0, columnIndex].RemovePiece(-1, columnIndex);
            board[0, columnIndex] = null;
        }
        GamePiece[] newColumn = new GamePiece[7];

        for(int x = 5;x >= upTo;x--)
        {
            newColumn[x] = board[x+1, columnIndex];
        }

        for(int x = 6; x >= upTo; x--)
        {
            board[x, columnIndex] = newColumn[x];
        }
        methodReturnContainer = true;
        return true;
    }

    private int CheckIfColumnPullable(int columnIndex)//Checks if player2 can pull a column, returns -1 if cannot, returns index of row where column is pushed to
    {
        int p1Count = 0;
        int p2Count = 0;
        for(int x = 0;x < 7;x++)
        {
            if(board[x,columnIndex] == null)
            {
                if (p2Count >= p1Count)
                    return x;
                return -1;
            }
            else if(board[x,columnIndex].player == 1)
            {
                p1Count++;
            }
            else if (board[x, columnIndex].player == 2)
            {
                p2Count++;
            }
        }
        if (p2Count >= p1Count)
            return 6;
        return -1;
    }

    private int CheckIfColumnPushable(int columnIndex)//Checks if player1 can push a column, returns -1 if cannot, returns index of row where column is pushed to
    {
        int p1Count = 0;
        int p2Count = 0;
        for (int x = 6; x >= 0; x--)
        {
            if (board[x, columnIndex] == null)
            {
                if (p1Count >= p2Count)
                    return x;
                return -1;
            }
            else if (board[x, columnIndex].player == 1)
            {
                p1Count++;
            }
            else if (board[x, columnIndex].player == 2)
            {
                p2Count++;
            }
        }
        if (p1Count >= p2Count)
            return 0;
        return -1;
    }
    public void PullDiagonal(int columnIndex,int dir)
    {
        PhotonView pView = PhotonView.Get(this);
        pView.RPC("PullDiagonalMain", RpcTarget.AllBuffered, columnIndex,dir);
    }


    [PunRPC]
    public bool PullDiagonalMain(int columnIndex,int dir)//Pulls a diagonal one down in dir direction
    {
        int upTo = CheckIfDiagonalPullable(columnIndex,dir);//Pulls all squares up to this row upTo(if -1, then return false since column is not pullable)
        if (upTo == -1)
        {
            methodReturnContainer = false;
            return false;
        }
        int lastRow;//Last row in the diagonal
        if (dir == 1)
            lastRow = ((6 - columnIndex));
        else
            lastRow = ((columnIndex));

        if (upTo == lastRow && board[lastRow, columnIndex+dir * lastRow] != null)//Last piece in diagonal is pushed off board
        {
            board[lastRow, columnIndex + dir * lastRow].RemovePiece(lastRow+1, columnIndex + dir * (lastRow+1));
            board[lastRow, columnIndex + dir * lastRow] = null;
        }

        GamePiece[] newDiagonal = new GamePiece[7];//Diagonal

        for (int x = 1; x <= upTo; x++)
        {
            newDiagonal[x] = board[x - 1, columnIndex+dir*(x-1)];
        }

        for (int x = 0; x <= upTo; x++)
        {
            board[x, columnIndex+x*dir] = newDiagonal[x];
        }
        methodReturnContainer = true;
        return true;
    }

    public void PushDiagonal(int columnIndex, int dir)
    {
        PhotonView pView = PhotonView.Get(this);
        pView.RPC("PushDiagonalMain", RpcTarget.AllBuffered, columnIndex, dir);
    }
    [PunRPC]
    public bool PushDiagonalMain(int columnIndex, int dir)//Pushes a diagonal one down in dir direction
    {
        int upTo = CheckIfDiagonalPushable(columnIndex, dir);//Pushesall squares up to this row upTo(if -1, then return false since column is not pullable)
        if (upTo == -1)
        {
            methodReturnContainer = false;
            return false;
        }

        int lastRow;//Last row in the diagonal
        if (dir == 1)
            lastRow = (6 - (6 - columnIndex));
        else
            lastRow = (6 - (columnIndex));


        if (upTo == lastRow && board[lastRow, columnIndex + dir * (6-lastRow)] != null)//Last piece in diagonal is pushed off board
        {
            board[lastRow, columnIndex + dir * (6 - lastRow)].RemovePiece(lastRow - 1, columnIndex + dir * (6 - lastRow) + dir);
            board[lastRow, columnIndex + dir * (6 - lastRow)] = null;
        }

        GamePiece[] newDiagonal = new GamePiece[7];//Diagonal

        for (int x = 5; x >= upTo; x--)
        {
            newDiagonal[x] = board[x + 1, columnIndex + dir * (6 - (x + 1))];
        }

        for (int x = 6; x >= upTo; x--)
        {
            board[x, columnIndex + dir * (6 - x)] = newDiagonal[x];
        }
        methodReturnContainer = true;
        return true;
    }

    private int CheckIfDiagonalPushable(int columnIndex,int direction)//Checks if p1 can push a diagonal, returns index of row where column is pushed to
    {
        int p1Count = 0;
        int p2Count = 0;
        int lastRow;

        if(direction == 1)
            lastRow = (6 - (6 - columnIndex));
        else
            lastRow = (6 - (columnIndex));

        for (int x = 6; x >= lastRow; x--)
        {
            if (board[x, columnIndex+ ((6-x)*direction)] == null)
            {
                if (p1Count >= p2Count)
                    return x;
                return -1;
            }
            else if (board[x, columnIndex + ((6 - x) * direction)].player == 1)
            {
                p1Count++;
            }
            else if (board[x, columnIndex + ((6 - x) * direction)].player == 2)
            {
                p2Count++;
            }
        }
        if (p1Count >= p2Count)
            return lastRow;
        return -1;
    }

    private int CheckIfDiagonalPullable(int columnIndex, int direction)//Checks if p2 can pull a diagonal, returns index of row where column is pushed to
    {
        int p1Count = 0;
        int p2Count = 0;
        int lastRow;

        if (direction == 1)
            lastRow = ((6 - columnIndex));
        else
            lastRow = ((columnIndex));
        for (int x = 0; x <= lastRow; x++)
        {
            if (board[x, columnIndex + x * direction] == null)
            {
                if (p2Count >= p1Count)
                    return x;
                return -1;
            }
            else if (board[x, columnIndex + x * direction].player == 1)
            {
                p1Count++;
            }
            else if (board[x, columnIndex + x * direction].player == 2)
            {
                p2Count++;
            }
        }
        if (p2Count >= p1Count)
            return lastRow;
        return -1;
    }



    private void CheckForGameEnd()//Checks if game has ended
    {
        for(int x = 0;x < 7;x++)//Checks if any player 1 stones
        {
            if (board[0,x] != null && board[0, x].player == 1)
            {
                EndGame(1);
                return;
            }
        }
        for (int x = 0; x < 7; x++)//Checks if any player 1 stones
        {
            if (board[6, x] != null && board[6, x].player == 2)
            {
                EndGame(2);
                return;
            }
        }
        if(p1StoneCount == 0 && p2StoneCount == 0)
        {
            EndGame(0);
            return;
        }
    }

    void EndGame(int result)
    {
        gameEnded = true;
        ScoreboardScript.scoreboardScript.EndGame(result);
    }

    public void PlaceStone(int yIndex, int xIndex, int player, int xOffset, int yOffset)
    {
        PhotonView pView = PhotonView.Get(this);
        pView.RPC("PlaceStoneMain", RpcTarget.AllBuffered, yIndex,xIndex,player,xOffset,yOffset);
    }

    [PunRPC]

    public void PlaceStoneMain(int yIndex, int xIndex,int player,int xOffset, int yOffset)//Places stone(note, replaces any stone on the square, push/ pull first)
    {
        GameObject stoneGameObject;
        if(player == 1)
            stoneGameObject = Instantiate(stonePrefab1,new Vector3((xIndex+xOffset)*unitsPerSquare,0,(6-yIndex-yOffset)*unitsPerSquare),Quaternion.identity);
        else
            stoneGameObject = Instantiate(stonePrefab2, new Vector3((xIndex + xOffset) * unitsPerSquare, 0, (6 - yIndex - yOffset) * unitsPerSquare), Quaternion.identity);
        GamePiece newPiece = new GamePiece(stoneGameObject, player, xIndex, yIndex);
        board[yIndex, xIndex] = newPiece;
        DecrementStoneCount(player);
    }

    public void UpdateGamePiecePositions()
    {
        PhotonView pView = PhotonView.Get(this);
        pView.RPC("RPCUpdateGamePiecePositions", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPCUpdateGamePiecePositions()
    {
        StartCoroutine(UpdateGamePiecePositionsMain());
    }

    public IEnumerator UpdateGamePiecePositionsMain()//Updates each GamePiece xSquare and ySquare to match positions, as well as Physics positions
    {
        isUpdatingPositions = true;
        for (int i = 0;i <= 20;i++)
        {
            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    if (board[y, x] != null && board[y,x].pieceGameObject != null)
                    {
                        board[y, x].ySquare = y;
                        board[y, x].xSquare = x;
                        Vector3 originalPos = board[y, x].pieceGameObject.transform.position;
                        board[y, x].pieceGameObject.transform.position = Vector3.Lerp(originalPos,new Vector3(x * unitsPerSquare, 0, (6 - y) * unitsPerSquare),0.2f);
                    }
                }
            }        
            yield return new WaitForFixedUpdate();
        }
        //Updates scoreboard
        CheckForGameEnd();
        isUpdatingPositions = false;
    }

    void CreateGrid()
    {
        for(int y = 0;y < 7;y++)
        {
            for(int x = 0;x < 7;x++)
            {
                Instantiate(gridPrefab, new Vector3(x * unitsPerSquare, 0, y * unitsPerSquare),Quaternion.identity);
            }
        }
    }

    public void IncrementStoneCount(int player)
    {
        if (player == 1)
        {
            p1StoneCount++;
        }
        else
            p2StoneCount++;
    }

    public void DecrementStoneCount(int player)
    {
        if (player == 1)
        {
            p1StoneCount--;
        }
        else
            p2StoneCount--;
    }

    public int GetCount(int player)
    {
        if (player == 1)
        {
            return p1StoneCount;
        }
        else
            return p2StoneCount;
    }

    public bool GetIsUpdatingPositions()
    {
        return isUpdatingPositions;
    }

    public int GetP1StoneCount()
    {
        return p1StoneCount;
    }

    public int GetP2StoneCount()
    {
        return p2StoneCount;
    }

    public void SetTurn(int val)
    {
        PhotonView pView = PhotonView.Get(this);
        pView.RPC("SetTurnMain", RpcTarget.AllBuffered,val);
    }

    [PunRPC]

    private void SetTurnMain(int val)
    {
        turn = val;
        ScoreboardScript.scoreboardScript.SetNameTextTurn(val, true);
        ScoreboardScript.scoreboardScript.SetNameTextTurn(val % 2 + 1, false);
    }

    //Networking

/*
    void SendBoardData()
    {
        PhotonView pView = PhotonView.Get(this);
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                GamePiece p = board[y, x];
                if (p == null)
                {
                    pView.RPC("UpdateBoardSquareData", RpcTarget.OthersBuffered, 0, 0, 0,0);
                }
                else
                {
                    PhotonView gamePieceView = p.pieceGameObject.GetComponent<PhotonView>();
                    int viewId = gamePieceView.ViewID;
                    pView.RPC("UpdateBoardSquareData", RpcTarget.OthersBuffered, y, x, p.player,viewId);
                }

            }
        }
        pView.RPC("UpdateStoneCountAndTurn", RpcTarget.All,  p1StoneCount, p2StoneCount,turn);
    }
    [PunRPC]
    void UpdateBoardSquareData(int yIndex, int xIndex, int player,int gamePieceViewId)
    {
        if(yIndex == 0)
        {
            board[yIndex, xIndex] = null;
            return;
        }
        GamePiece piece = new GamePiece(PhotonView.Find(gamePieceViewId).gameObject, player, xIndex, yIndex);
        board[yIndex, xIndex] = piece;   
    }
    [PunRPC]

    void UpdateStoneCountAndTurn(int p1Count, int p2Count, int turn)
    {
        Debug.Log("BoardData Updated");
        p1StoneCount = p1Count;
        p2StoneCount = p2Count;
        this.turn = turn;
    }

    [PunRPC]

    void SendIsUpdating(bool isUpdating)
    {
        this.isUpdatingPositions = isUpdating;
    }

*/
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /*
        if (stream.IsWriting)
        {
            Debug.Log("Writing GameManager Data To Server");
            stream.SendNext(turn);
            stream.SendNext(isUpdatingPositions);
            SendBoardData(stream);
        }
        else
        {
            Debug.Log("Receiving GameManager Data From Server");
            turn = (int)stream.ReceiveNext();
            isUpdatingPositions = (bool)stream.ReceiveNext();
            ReceiveBoardData(stream);
        }
        */
    }
    //Game piece data is sent and received in order of:
    //player number, ySquare, xSquare, gameObject

    /*
    private void SendBoardData(PhotonStream stream)
    {
        for(int y = 0;y < 7;y++)
        {
            for(int x = 0;x < 7;x++)
            {
                GamePiece p = board[y, x];
                if(p == null)
                {
                    stream.SendNext(0);//Sends 0 if this square is null
                    stream.SendNext(0);
                    stream.SendNext(0);
                    stream.SendNext(0);
                }
                else
                {
                    stream.SendNext(p.player);
                    stream.SendNext(p.ySquare);
                    stream.SendNext(p.xSquare);
                    PhotonView gamePieceView = p.pieceGameObject.GetComponent<PhotonView>();
                    int viewId = gamePieceView.ViewID;
                    stream.SendNext(viewId);
                }
                
            }
        }
    }

    private void ReceiveBoardData(PhotonStream stream)
    {
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                int receivePlayer = (int)stream.ReceiveNext();
                int receiveYSquare = (int)stream.ReceiveNext();
                int receiveXSquare = (int)stream.ReceiveNext();
                int receivePieceViewId = (int)stream.ReceiveNext();
                if (receivePlayer == 0)
                {
                    board[y, x] = null;
                    return;
                }
                GamePiece p = new GamePiece(PhotonView.Find(receivePieceViewId).gameObject,receivePlayer, receiveXSquare, receiveYSquare);
                board[y, x] = p;
            }
        }
    }
    */
}

