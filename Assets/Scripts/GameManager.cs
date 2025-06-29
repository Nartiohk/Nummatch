
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{

    public int stage = 1;
    public bool gameOver = false;
    public GridManager gridManager;
    public InputHandler inputHandler;
    public ScoreManager scoreManager;
    public Text stageText;
    public GameObject gameOverPanel;
    // public ScoreManager ScoreManager;

    public void Start()
    {
        // Initialize the game
        gameOver = false;
        gameOverPanel.SetActive(gameOver);
        inputHandler.enabled = true; // Enable input handling
        stage = 1;
        stageText.text = "Stage: " + stage;
        gridManager.TestStage(stage);
        scoreManager.score = 0; // Reset score at the start of the game
    }

    public void TileMatch()
    {
        // Increment the score when a tile matches
        scoreManager.IncrementScore();
        
        // Check if all tiles are matched
        // if (gridManager.AllTilesMatched())
        // {
        //     // Move to the next stage
        //     stage++;
        //     gridManager.GenerateNewStage(stage);
        // }
    }
    
    public void GameOver()
    {
        gameOver = true;
        gameOverPanel.SetActive(gameOver);
        inputHandler.enabled = false; // Disable input handling
        Debug.Log("Game Over! Final Score: " + scoreManager.score);
        // Here you can add logic to display a game over screen or reset the game
    }

    public void GameWon()
    {
        stage++;
        stageText.text = "Stage: " + stage;
        gridManager.TestStage(stage);
    }
}
