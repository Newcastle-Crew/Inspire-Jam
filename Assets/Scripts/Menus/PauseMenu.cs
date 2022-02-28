#region 'Using' information
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#endregion

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;

    private void Awake()
    { Resume(); } // Makes sure that the level isn't started in a paused state.
    
    IEnumerator LoadLevel(int levelIndex)
    {
        yield return new WaitForSeconds(0);
        SceneManager.LoadScene(levelIndex); // Loads the main menu.
    }

    public void BacktoMenu()
    { StartCoroutine(LoadLevel(0)); } // Make sure the buildindex has the main menu as its '0th' scene.

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Pause"))
        {
            if(GameIsPaused)
            { Resume(); }
            else
            { Pause(); }
        }
    }

    public void Resume()
    {
        SUPERCharacter.SUPERCharacterAIO.Instance.ExitGUIMode();
        Time.timeScale = 1f;
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
    }

    public void Pause()
    {
        SUPERCharacter.SUPERCharacterAIO.Instance.EnterGUIMode();
        Time.timeScale = 0f;
        GameIsPaused = true;
        pauseMenuUI.SetActive(true);
    }
}
