using UnityEngine;

public class Tetromino : MonoBehaviour
{
    public bool allowRotation = true;  //majoritatea pieselor nu au rotatia limitata
    public bool limitRotation = false;

    public AudioClip moveSound;
    public AudioClip rotateSound;
    public AudioClip landSound;

    public int individualScore = 100;

    private float individualScoreTime;
    private AudioSource audioSource;

    private float fall = 0; //timer
    private float fallSpeed;  //daca era =1 adica 1 unit/sec daca era 0.8  unit pe sec

    private readonly float continuousVerticalSpeed = 0.05f; //viteza cu care se muta tetromino in jos
    private readonly float continuousHorizontalSpeed = 0.1f; //viteza pt stanga si dreapta

    private float verticalTimer = 0;
    private float horizontalTimer = 0;
    private readonly float buttonDownWaitMax = 0.2f; //ca sa porneasca miscarea la un interval de timp dupa ce am apasat o sageata, nu chiar la momentul apasarii
    private float buttonDownWaitTimerHorizontal = 0;
    private float buttonDownWaitTimerVertical = 0;

    private bool moveImediateHorizontal = false;
    private bool moveImediateVertical = false;

    // Start is called before the first frame update
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Game.isPause)
        {
            CheckUserInput();
            UpdateIndividualScore();
            UpdateFallSpeed();
        }
    }

    private void UpdateFallSpeed()
    {
        fallSpeed = Game.fallSpeed;
    }

    // Scade scorul cu 10 puncte la fiecare secunda daca tetramino-ul nu a ajuns jos
    // ca sa incurajeze jucatorul sa joace rapid
    private void UpdateIndividualScore()
    {
        if (individualScoreTime < 1)
        {
            // Time.deltaTime = timpul intre doua frame-uri. Daca fps = 120 Time.deltaTime = 1/120.
            individualScoreTime += Time.deltaTime;
        }
        else
        {
            individualScoreTime = 0;
            individualScore = Mathf.Max(individualScore - 10, 0);
        }
    }

    private void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            horizontalTimer = 0;
            buttonDownWaitTimerHorizontal = 0;
            moveImediateHorizontal = false;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            verticalTimer = 0;
            buttonDownWaitTimerVertical = 0;
            moveImediateVertical = false;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveHorizontally(Direction.Right);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveHorizontally(Direction.Left);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate();
        }
        if (Input.GetKey(KeyCode.DownArrow) || Time.time - fall >= fallSpeed)
        {
            MoveDown();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SlamDown();
        }
    }

    private void SlamDown()
    {
        while (CheckIsValidPosition())
        {
            transform.position -= new Vector3(0, 1, 0);
        }
        if (!CheckIsValidPosition())
        {
            transform.position += new Vector3(0, 1, 0);
            Game game = FindObjectOfType<Game>();
            game.UpdateGrid(this);

            game.DeleteRow();

            if (game.CheckIsAboveGrid(this))
            {
                game.GameOver();
            }

            PlayLandAudio();
            game.SpawnNextTetromino();
            Game.currentScore += individualScore;

            enabled = false;
            tag = "Untagged";
        }
    }

    private void MoveHorizontally(Direction direction)
    {
        if (moveImediateHorizontal)
        {
            if (buttonDownWaitTimerHorizontal < buttonDownWaitMax)
            {
                buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }
            if (horizontalTimer < continuousHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return;
            }
        }
        else
        {
            moveImediateHorizontal = true;
        }

        horizontalTimer = 0;
        transform.position += new Vector3((int)direction, 0, 0);

        if (CheckIsValidPosition())
        {
            FindObjectOfType<Game>().UpdateGrid(this);
            PlayMoveAudio();
        }
        else
        {
            transform.position += new Vector3((int)direction * -1, 0, 0);
        }
    }

    private void MoveDown()
    {
        if (moveImediateVertical)
        {
            if (buttonDownWaitTimerVertical < buttonDownWaitMax)
            {
                buttonDownWaitTimerVertical += Time.deltaTime;
                return;
            }
            if (verticalTimer < continuousVerticalSpeed)
            {
                verticalTimer += Time.deltaTime;
                return;
            }
        }
        else
        {
            moveImediateVertical = true;
        }

        verticalTimer = 0;
        transform.position -= new Vector3(0, 1, 0);

        Game game = FindObjectOfType<Game>();

        if (CheckIsValidPosition())
        {
            game.UpdateGrid(this);
            PlayMoveAudio();
        }
        else
        {
            transform.position += new Vector3(0, 1, 0);
            game.DeleteRow();

            if (game.CheckIsAboveGrid(this))
            {
                game.GameOver();
            }

            PlayLandAudio();
            game.SpawnNextTetromino();
            Game.currentScore += individualScore;

            enabled = false;
            tag = "Untagged";
        }
        fall = Time.time;
    }

    private void Rotate()
    {
        if (allowRotation)
        {
            if (limitRotation && transform.rotation.eulerAngles.z >= 90)
            {
                transform.Rotate(0, 0, -90);
            }
            else
            {
                transform.Rotate(0, 0, 90);
            }

            if (CheckIsValidPosition())
            {
                FindObjectOfType<Game>().UpdateGrid(this);
                PlayRotateAudio();
            }
            else
            {
                if (limitRotation && transform.rotation.eulerAngles.z < 90)
                {
                    transform.Rotate(0, 0, 90);
                }
                else
                {
                    transform.Rotate(0, 0, -90);
                }
            }
        }
    }

    private void PlayMoveAudio()
    {
        audioSource.PlayOneShot(moveSound);  //move right down or left
    }

    private void PlayRotateAudio()
    {
        audioSource.PlayOneShot(rotateSound);
    }

    private void PlayLandAudio() //land - a ateriza
    {
        audioSource.PlayOneShot(landSound);
    }

    private bool CheckIsValidPosition()
    {
        return FindObjectOfType<Game>().CheckIsValidPosition(gameObject);
    }

    private enum Direction
    {
        Left = -1,
        Right = 1
    }
}