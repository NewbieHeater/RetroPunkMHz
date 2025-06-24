using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManger : MonoBehaviour
{
    public Ball ball;
    public paddle playerPaddle;
    public paddle computerPaddle;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI computerScoreText;
    private int playerScore;

    private int computerScore;
    private string ScoreChar = "Win";
    public void PlayerScores()
    {
        playerScore++;
        if(playerScore > 4) // 5점내기
        {
            this.playerScoreText.text = ScoreChar.ToString();
            GameEnd();
        }
        else
        {
            this.playerScoreText.text = playerScore.ToString();
            ResetRound();
        }
            
    }

    public void ComputerScores()
    {
        
        computerScore++;
        if (computerScore > 4) // 5점내기
        {
            this.playerScoreText.text = ScoreChar.ToString();
            GameEnd();
        }
        else
        {
            this.computerScoreText.text = computerScore.ToString();
            ResetRound();
        }
            
    }
    private void ResetRound()
    {
        this.playerPaddle.ResetPosition();
        this.computerPaddle.ResetPosition();
        this.ball.ResetPosition();
        this.ball.AddStartingForce();

    }

    public void Reset()
    {
        playerScore=0;
        this.playerScoreText.text = playerScore.ToString();
        computerScore =0;
        this.computerScoreText.text = computerScore.ToString();
        ResetRound();
    }

    private void GameEnd()
    {
        this.playerPaddle.ResetPosition();
        this.computerPaddle.ResetPosition();
        this.ball.ResetPosition();
    }
}
   

