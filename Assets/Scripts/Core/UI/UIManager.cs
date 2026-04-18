using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] private GameObject gameOverScreen;    
    [SerializeField] private AudioClip gameOverSound;

    [Header("Pause")]
    [SerializeField] private GameObject pauseScreen;

    private void Awake()
    {
        gameOverScreen.SetActive(false);
        pauseScreen.SetActive(false);
    }
    private void OnEnable()
    {
        GameInput.Instance.PausePressed += Pause;
    }

    // #region Game Over
    // //Activate game over screen
    // public void GameOver()
    // {
    //     gameOverScreen.SetActive(true);
    //     SoundManager.instance.PlaySound(gameOverSound);
    // }

    // //Game Over functions
    // public void Restart()
    // {
    //     // Returns the currently active scene and reloads it
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    // }

    // public void MainMenu()
    // {
    //     // Assumes main menu is the first scene in the build settings, so loads scene with index 0
    //     SceneManager.LoadScene(0);
    // }

    // public void Quit()
    // {
    //     Application.Quit(); //Quits the game, (only works in a built version, not in the editor)

    //     #if UNITY_EDITOR
    //     UnityEditor.EditorApplication.isPlaying = false; //Exits play mode in the editor (will only be executed in the editor)
    //     #endif
    // }
    // #endregion

    #region Pause
    public void Pause() // gets called when pause button is pressed
    {
        //If pause screen is active, unpause, else pause
            if (pauseScreen.activeInHierarchy)
                PauseGame(false);
            else
                PauseGame(true);
    }
    public void PauseGame(bool status)
    {
        // if status == true, pause else unpause
        pauseScreen.SetActive(status);
        if (status)
            Time.timeScale = 0; // set time scale to 0 to pause the game
        else
            Time.timeScale = 1; // set time scale back to 1 to unpause the game
    }

    public void SoundVolume()
    {
        SoundManager.instance.ChangeSoundVolume(0.2f);
    }

    public void MusicVolume()
    {
        SoundManager.instance.ChangeMusicVolume(0.2f);
    }
    #endregion
}
