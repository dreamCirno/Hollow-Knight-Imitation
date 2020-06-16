using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public Animator animator;
    public static bool gameIsPaused = false;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        gameIsPaused = true;
        animator.Play("FadeIn");
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        gameIsPaused = false;
        animator.Play("FadeOut");
    }
}
