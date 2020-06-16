using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuButtonController : MonoBehaviour
{
    public enum PauseMenuButtonConstant
    {
        RESUME, OPTIONS, BACKTOMENU,
    }
    public int index;

    [Header("Aniamtor")]
    public Animator pauseMenuAnimator;
    public Animator audioMenuAnimator;

    [SerializeField] private int maxIndex;
    private GameSceneManager gameSceneManager;
    private PauseMenu pauseMenu;
    private PauseMenuButton[] pauseMenuButtons;

    private void Awake()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();
        pauseMenu = FindObjectOfType<PauseMenu>();
    }

    void Start()
    {
        pauseMenuButtons = GetComponentsInChildren<PauseMenuButton>();
    }

    private void OnEnable()
    {
        InputManager.InputControl.UI.Navigate.performed += Navigate_performed;
        InputManager.InputControl.UI.Submit.performed += Submit_performed;
    }

    private void OnDisable()
    {
        InputManager.InputControl.UI.Navigate.performed -= Navigate_performed;
        InputManager.InputControl.UI.Submit.performed -= Submit_performed;
    }

    private void Navigate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!PauseMenu.gameIsPaused)
            return;
            Vector2 input = obj.ReadValue<Vector2>();
            if (input.y == 1)
            {
                if (index > 0)
                {
                    index--;
                }
                else
                {
                    index = maxIndex;
                }
            }
            else if (input.y == -1)
            {
                if (index < maxIndex)
                {
                    index++;
                }
                else
                {
                    index = 0;
                }
            }
    }

    private void Submit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Pressed();
    }

    public void Pressed()
    {
        switch (index)
        {
            case (int)PauseMenuButtonConstant.RESUME:
                pauseMenu.Resume();
                break;
            case (int)PauseMenuButtonConstant.OPTIONS:
                pauseMenuAnimator.Play("FadeOut");
                audioMenuAnimator.Play("FadeIn");
                break;
            case (int)PauseMenuButtonConstant.BACKTOMENU:
                SceneManager.LoadSceneAsync(1);
                pauseMenu.Resume();
                break;
        }
    }
}
