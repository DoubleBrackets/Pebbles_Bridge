using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerSharedDevice : MonoBehaviour
{
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



    public static GameManagerSharedDevice gameManagerSharedDevice;

    public GameObject stonePrefab1;
    public GameObject stonePrefab2;
    public GameObject hoverPrefab;
    public GameObject gridPrefab;

    private GameObject hoverIndicatorObject;
    public Color actionColor;
    private int selectedXIndex;
    private int selectedYIndex;
    private Color defaultColor;

    private GamePiece[,] board;

    private float unitsPerSquare = 2;

    private int turn = 1;//WHose tUrn

    //Controls
    private int firstXIndex;
    private int firstyIndex;

    private bool isUpdatingPositions = false;

    private int p1StoneCount;
    private int p2StoneCount;
    
    public int startStoneCount = 30;

    private bool gameEnded = false;

    private int piecesToWin = 3;

    private void Awake()
    {
        gameManagerSharedDevice = this;
        board = new GamePiece[7,7];

        hoverIndicatorObject = Instantiate(hoverPrefab, Vector2.zero, Quaternion.identity);
        defaultColor = hoverIndicatorObject.GetComponent<MeshRenderer>().material.color;

        p1StoneCount = startStoneCount;
        p2StoneCount = startStoneCount+3;

        CenterCamera();
        //CreateGrid();
    }

    private void Update()
    {
        if (gameEnded)
            return;
        //Selects a square
        Plane p = new Plane(Vector3.up, 0);
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        if(p.Raycast(mouseRay,out dist))
        {
            Vector3 mousePos = mouseRay.GetPoint(dist);

            float xSquare = Mathf.Clamp(Mathf.Round(mousePos.x / unitsPerSquare), 0, 6);
            float zSquare = Mathf.Clamp(Mathf.Round(mousePos.z / unitsPerSquare), 0, 6);
            hoverIndicatorObject.transform.position = new Vector3(xSquare * unitsPerSquare, 0, zSquare * unitsPerSquare);
            selectedXIndex = (int)xSquare;
            selectedYIndex = 6 - (int)zSquare;
        }
        //Controls
        if(!isUpdatingPositions)
        {
            if(UserInput())
                StartCoroutine(UpdateGamePieceSharedDevicePositions());
            ScoreboardScript.scoreboardScript.UpdateScoreboard(p1StoneCount, p2StoneCount);
            CheckForGameEnd();
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

            if (yChange == 1 && turn == 2 && firstyIndex == 0)//Pulling(p2)
            {
                if (xChange == 0 )//p2 Pulls column
                {
                    if(PullColumn(firstXIndex))
                    {
                        PlaceStone(0, firstXIndex, 2, 0, -1);
                        if(p1StoneCount > 0)
                            turn = 1;
                        return true;
                    }
                }
                else if (PullDiagonal(firstXIndex, xChange))
                {
                    PlaceStone(0, firstXIndex, 2,-xChange,-1);
                    if (p1StoneCount > 0)
                        turn = 1;
                    return true;
                }
            }
            else if (yChange == -1 && turn == 1 && firstyIndex == 6)//Pushing(p1)
            {
                if (xChange == 0)
                {
                    if(PushColumn(firstXIndex))
                    {
                        PlaceStone(6, firstXIndex, 1, 0, 1);
                        if (p2StoneCount > 0)
                            turn = 2;
                        return true;
                    }
                }
                else if (PushDiagonal(firstXIndex, xChange))
                {
                    PlaceStone(6, firstXIndex, 1,-xChange,1);
                    if (p2StoneCount > 0)
                        turn = 2;
                    return true;
                }
            }
        }
        return false;
    }


    private bool PullColumn(int columnIndex)//Pulls a column one down
    {
        int upTo = CheckIfColumnPullable(columnIndex);//Pulls all squares up to this(if -1, then return false since column is not pullable)
        if (upTo == -1)
            return false;


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
        return true;
    }

    private bool PushColumn(int columnIndex)//Pushes a column one up
    {
        int upTo = CheckIfColumnPushable(columnIndex);//Pushes all squares up to this(if -1, then return false since column is not pushable)
        if (upTo == -1)
            return false;

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
    private bool PullDiagonal(int columnIndex,int dir)//Pulls a diagonal one down in dir direction
    {
        int upTo = CheckIfDiagonalPullable(columnIndex,dir);//Pulls all squares up to this row upTo(if -1, then return false since column is not pullable)
        if (upTo == -1)
            return false;
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
        return true;
    }

    private bool PushDiagonal(int columnIndex, int dir)//Pushes a diagonal one down in dir direction
    {
        int upTo = CheckIfDiagonalPushable(columnIndex, dir);//Pushesall squares up to this row upTo(if -1, then return false since column is not pullable)
        if (upTo == -1)
            return false;
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
        int counter = 0;
        for(int x = 0;x < 7;x++)//Checks if any player 1 stones
        {
            if (board[0,x] != null && board[0, x].player == 1)
            {
                counter++;
            }
        }
        if (counter >= piecesToWin)
        {
            EndGame(1);
            return;
        }
        counter = 0;
        for (int x = 0; x < 7; x++)//Checks if any player 1 stones
        {
            if (board[6, x] != null && board[6, x].player == 2)
            {
                counter++;
            }
        }
        if (counter >= piecesToWin)
        {
            EndGame(2);
            return;
        }
        if (p1StoneCount == 0 && p2StoneCount == 0)
        {
            EndGame(0);
            return;
        }
    }

    void EndGame(int result)
    {
        gameEnded = true;
        ScoreboardScript.scoreboardScript.EndGame(result);
        turn = 0;
    }

    private void PlaceStone(int yIndex, int xIndex,int player,int xOffset, int yOffset)//Places stone(note, replaces any stone on the square, push/ pull first)
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

    IEnumerator UpdateGamePieceSharedDevicePositions()//Updates each GamePieceSharedDevice xSquare and ySquare to match positions, as well as Physics positions
    {
        isUpdatingPositions = true;
        for(int i = 0;i <= 20;i++)
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
        int tempTurn = turn;

        for(int x = 0;x < 60;x++)
        {
            if (tempTurn == 1)
            {
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, Quaternion.Euler(new Vector3(90, 360, 0)), 0.1f);
            }
            else
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, Quaternion.Euler(new Vector3(90, 180, 0)), 0.1f);
            yield return new WaitForFixedUpdate();
            if(x == 35)
            {
                isUpdatingPositions = false;
            }
        }
        if (tempTurn == 1)
        {
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }
        else
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90, 180, 0));
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
        //Shifts camera
        CenterCamera();
    }

    void CenterCamera()
    {
        Camera.main.transform.position = new Vector3(3f * unitsPerSquare, 17, 3f * unitsPerSquare);
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
}

