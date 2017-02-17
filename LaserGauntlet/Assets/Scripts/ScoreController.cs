using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    public int playerScore = 1000;
    public Text gradeText;


	void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    public void SubtractScore(int amount)
    {
        playerScore = playerScore - amount;
    }

    public void DetermineLetterGrade()
    {
        if(playerScore <= 0)
        {
            gradeText.text = "C-";
        }
        if (playerScore == 100)
        {
            gradeText.text = "C";
        }
        if (playerScore == 200)
        {
            gradeText.text = "C+";
        }
        if (playerScore == 300)
        {
            gradeText.text = "B-";
        }
        if (playerScore == 400)
        {
            gradeText.text = "B";
        }
        if (playerScore == 500)
        {
            gradeText.text = "B+";
        }
        if (playerScore == 600)
        {
            gradeText.text = "A-";
        }
        if (playerScore == 700)
        {
            gradeText.text = "A";
        }
        if (playerScore == 800)
        {
            gradeText.text = "A+";
        }
        if (playerScore == 900)
        {
            gradeText.text = "A+";
        }
        if (playerScore == 1000)
        {
            gradeText.text = "PERFECT!";
            gradeText.fontSize = 90;
        }
    }
}
