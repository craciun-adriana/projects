using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Slider levelSlider;

    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI highScoreText2;
    public TextMeshProUGUI highScoreText3;
    public TextMeshProUGUI lastScore;

    // Start is called before the first frame update
    private void Start()
    {
        if (levelText != null)
        {
            levelText.text = "0";
        }
        // Daca nu exista obiectul asociat nu face nimic, pt scena GameOver.
        if (highScoreText != null)
        {
            highScoreText.text = PlayerPrefs.GetInt("highscore").ToString();
        }
        if (highScoreText2 != null)
        {
            highScoreText2.text = PlayerPrefs.GetInt("highscore2").ToString();
        }
        if (highScoreText3 != null)
        {
            highScoreText3.text = PlayerPrefs.GetInt("highscore3").ToString();
        }
        if (lastScore != null)
        {
            lastScore.text = PlayerPrefs.GetInt("lastscore").ToString();
        }
    }

    //apelam de la buton
    public void PlayGame()
    {
        Game.startingLevelZero = (Game.startingLevel == 0);
        Application.LoadLevel("Level");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeValue()
    {
        Game.startingLevel = (int)levelSlider.value;
        levelText.text = levelSlider.value.ToString();
    }

    public void LaunchGameMenu()
    {
        Application.LoadLevel("GameMenu");
    }
}