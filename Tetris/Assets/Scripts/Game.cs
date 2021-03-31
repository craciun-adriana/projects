using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static int gridWidth = 10;
    public static int gridHight = 20;
    public static Transform[,] grid = new Transform[gridWidth, gridHight];

    public static float fallSpeed = 1.0f;
    public static int currentScore = 0;

    public static bool startingLevelZero;
    public static int startingLevel;
    public static bool isPause = false;

    public int scoreOneLine = 40;
    public int scoreTwoLine = 100;
    public int scoreThreeLine = 300;
    public int scoreFourLine = 1200;

    public int currentLevel = 0;

    public int maxSwaps = 2;

    public AudioSource gameScript;
    public Text hud_score;
    public Text hud_level;
    public Text hud_lines;
    public AudioClip clearedLineSource;
    public Canvas hud_canvas;
    public Canvas pause_canvas;

    private int numLineCleared = 0;
    private int startingHighScore;
    private int startingHighScore2;
    private int startingHighScore3;
    private int currentSwaps = 0;
    private int numberOfRowsThisTurn = 0;

    private GameObject previewTetromino;
    private GameObject nextTetromino;
    private GameObject savedTetromino;
    private GameObject ghostTetromino;

    private bool gameStarted = false;

    private Vector2 previewTetrominoPosition = new Vector2(-6.5f, 17);
    private Vector2 savedTetrominoPosition = new Vector2(-6.5f, 10);

    private AudioSource audioSource;
    private Vector2 tetraminoStartPosition = new Vector2(5.5f, 20.5f);

    // Start is called before the first frame update
    private void Start()
    {
        currentLevel = startingLevel;
        currentScore = 0;
        audioSource = GetComponent<AudioSource>();
        startingHighScore = PlayerPrefs.GetInt("highscore");
        startingHighScore2 = PlayerPrefs.GetInt("highscore2");
        startingHighScore3 = PlayerPrefs.GetInt("highscore3");
        SpawnNextTetromino();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateScore();
        UpdateLevel();
        UpdateSpeed();
        UpdateUI();
        CheckUserInput();
    }

    public bool CheckIsAboveGrid(Tetromino tetromino)
    {
        foreach (Transform mino in tetromino.transform)
        {
            Vector2 pos = Round(mino.position);
            if (pos.y > gridHight - 1)
            {
                return true;
            }
        }
        return false;
    }

    public void DeleteRow()
    {
        for (int i = 0; i < gridHight; i++)
        {
            if (IsFullRowAt(i))
            {
                DeleteMinoAt(i);
                MoveAllRowsDown(i + 1);
                i--;
            }
        }
    }

    public void UpdateGrid(Tetromino tetromino)
    {
        for (int y = 0; y < gridHight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    if (grid[x, y].parent == tetromino.transform)
                    {
                        grid[x, y] = null;
                    }
                }
            }
        }

        foreach (Transform mino in tetromino.transform)
        {
            Vector2 pos = Round(mino.position);
            if (pos.y < gridHight)
            {
                grid[(int)pos.x, (int)pos.y] = mino;
            }
        }
    }

    public Transform GetTransformAtGridPosition(Vector2 pos)
    {
        if (pos.y > gridHight - 1)
        {
            return null;
        }
        else
        {
            return grid[(int)pos.x, (int)pos.y];
        }
    }

    public void SpawnNextTetromino()     //spawn-to cause something new, or many new things, to grow or start suddenly
    {
        if (!gameStarted)
        {
            gameStarted = true;
            nextTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), tetraminoStartPosition, Quaternion.identity); //5.0f il face din double float, Quaternion.identity-curent rotation
        }
        else
        {
            previewTetromino.transform.localPosition = tetraminoStartPosition;  //mutam pozitia ca previewTetromino sa devina next
            nextTetromino = previewTetromino;
            nextTetromino.GetComponent<Tetromino>().enabled = true;
        }
        //facem un nou previewTetromino
        previewTetromino = (GameObject)Instantiate(Resources.Load(GetRandomTetromino(), typeof(GameObject)), previewTetrominoPosition, Quaternion.identity);
        previewTetromino.GetComponent<Tetromino>().enabled = false;
        nextTetromino.tag = "currentActivTetromino";

        SpawnGhostTetromino();
        currentSwaps = 0;
    }

    public bool CheckIsInsideGrid(Vector2 pos)
    {
        return ((int)pos.x >= 0 && (int)pos.x < gridWidth && (int)pos.y >= 0);
    }

    public Vector2 Round(Vector2 pos)
    {
        return new Vector2(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));  //Mathf.Round -rotunjeste valorile, de ex 10.5 la 11, 10.2 la 10 / Mathf.FloorToInnt le rotunjeste in jos
    }

    private void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (Time.timeScale == 1)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            GameObject tempNextTetromino = GameObject.FindGameObjectWithTag("currentActivTetromino");
            SaveTetromino(tempNextTetromino.transform);
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        gameScript.Pause();
        audioSource.Pause();
        isPause = true;
        hud_canvas.enabled = false;
        pause_canvas.enabled = true;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
        gameScript.Play();
        audioSource.Play();
        isPause = false;
        hud_canvas.enabled = true;
        pause_canvas.enabled = false;
    }

    private void UpdateLevel()
    {
        if (startingLevelZero || (startingLevelZero == false && numLineCleared / 10 > startingLevel))
        {
            currentLevel = numLineCleared / 10;
        }
    }

    private void UpdateSpeed()
    {
        fallSpeed = 1.0f - ((float)currentLevel * 0.1f);
    }

    private void UpdateUI()
    {
        hud_score.text = currentScore.ToString();
        hud_level.text = currentLevel.ToString();
        hud_lines.text = numLineCleared.ToString();
    }

    private void UpdateScore()
    {
        if (numberOfRowsThisTurn > 0)
        {
            ClearedRows(numberOfRowsThisTurn);

            numberOfRowsThisTurn = 0;
            audioSource.PlayOneShot(clearedLineSource);
        }
    }

    private void ClearedRows(int numberOfRows)
    {
        switch (numberOfRows)
        {
            case 1:
                currentScore += scoreOneLine;
                break;

            case 2:
                currentScore += scoreTwoLine;
                break;

            case 3:
                currentScore += scoreThreeLine;
                break;

            case 4:
                currentScore += scoreFourLine;
                break;
        }
        numLineCleared += numberOfRows;
    }

    private void UpdateHighScore()
    {
        if (currentScore > startingHighScore)
        {
            PlayerPrefs.SetInt("highscore", currentScore);
            PlayerPrefs.SetInt("highscore2", startingHighScore);
            PlayerPrefs.SetInt("highscore3", startingHighScore2);
        }
        else if (currentScore > startingHighScore2)
        {
            PlayerPrefs.SetInt("highscore2", currentScore);
            PlayerPrefs.SetInt("highscore3", startingHighScore2);
        }
        else if (currentScore > startingHighScore3)
        {
            PlayerPrefs.SetInt("highscore3", currentScore);
        }
        PlayerPrefs.SetInt("lastscore", currentScore);
    }

    public bool CheckIsValidPosition(GameObject tetromino)
    {
        foreach (Transform mino in tetromino.transform)
        {
            Vector2 pos = Round(mino.position);
            if (!CheckIsInsideGrid(pos))
            {
                return false;
            }
            if (GetTransformAtGridPosition(pos) != null && GetTransformAtGridPosition(pos).parent != tetromino.transform)
            {
                return false;
            }
        }
        return true;
    }

    private bool IsFullRowAt(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        numberOfRowsThisTurn++;
        return true;
    }

    private void DeleteMinoAt(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            DestroyImmediate(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    private void MoveRowDown(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] != null)
            {
                grid[x, y - 1] = grid[x, y];
                grid[x, y] = null;
                grid[x, y - 1].position -= new Vector3(0, 1, 0);
            }
        }
    }

    private void MoveAllRowsDown(int y)
    {
        for (int i = y; i < gridHight; ++i)
        {
            MoveRowDown(i);
        }
    }

    private void SpawnGhostTetromino()
    {
        if (GameObject.FindGameObjectWithTag("currentGhostTetromino") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("currentGhostTetromino"));
        }
        ghostTetromino = Instantiate(nextTetromino, nextTetromino.transform.position, Quaternion.identity);
        Destroy(ghostTetromino.GetComponent<Tetromino>());
        ghostTetromino.AddComponent<GhostTetromino>();
    }

    private void SaveTetromino(Transform t)
    {
        currentSwaps++;
        if (currentSwaps > maxSwaps)
        {
            return;
        }

        if (savedTetromino != null)
        {
            // There is a tetromino being hold.
            GameObject tempSavedTetromino = GameObject.FindGameObjectWithTag("currentSavedTetromino");
            tempSavedTetromino.transform.localPosition = tetraminoStartPosition;

            if (!CheckIsValidPosition(tempSavedTetromino))
            {
                tempSavedTetromino.transform.localPosition = savedTetrominoPosition;
                return;
            }

            savedTetromino = Instantiate(t.gameObject);
            savedTetromino.GetComponent<Tetromino>().enabled = false;
            savedTetromino.transform.localPosition = savedTetrominoPosition;
            savedTetromino.tag = "currentSavedTetromino";

            nextTetromino = Instantiate(tempSavedTetromino);
            nextTetromino.GetComponent<Tetromino>().enabled = true;
            nextTetromino.transform.localPosition = tetraminoStartPosition;
            nextTetromino.tag = "currentActivTetromino";

            DestroyImmediate(t.gameObject);
            DestroyImmediate(tempSavedTetromino);

            SpawnGhostTetromino();
        }
        else
        {
            savedTetromino = Instantiate(GameObject.FindGameObjectWithTag("currentActivTetromino"));
            savedTetromino.GetComponent<Tetromino>().enabled = false;
            savedTetromino.transform.localPosition = savedTetrominoPosition;
            savedTetromino.tag = "currentSavedTetromino";

            DestroyImmediate(GameObject.FindGameObjectWithTag("currentActivTetromino"));

            SpawnNextTetromino();
        }
    }

    private string GetRandomTetromino()
    {
        int randomTetromino = UnityEngine.Random.Range(1, 8);
        string randomTetrominoName;
        switch (randomTetromino)
        {
            case 1:
            default:
                randomTetrominoName = "Prefabs/Tetromino_T"; //avem Prefabs/ deoarece noi il luam din folderul resouces->prefabs->Tetromino_T
                break;

            case 2:
                randomTetrominoName = "Prefabs/Tetromino_Long";
                break;

            case 3:
                randomTetrominoName = "Prefabs/Tetromino_Square";
                break;

            case 4:
                randomTetrominoName = "Prefabs/Tetromino_J";
                break;

            case 5:
                randomTetrominoName = "Prefabs/Tetromino_L";
                break;

            case 6:
                randomTetrominoName = "Prefabs/Tetromino_S";
                break;

            case 7:
                randomTetrominoName = "Prefabs/Tetromino_Z";
                break;
        }
        return randomTetrominoName;
    }

    public void GameOver()
    {
        UpdateHighScore();
        Application.LoadLevel("GameOver");
    }
}