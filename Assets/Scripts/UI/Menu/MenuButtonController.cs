using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonController : MonoBehaviour
{
    public enum MainMenuButton
    {
        START, OPTIONS, CGREPLAY, QUIT,
    }
    public int index;
    public GameObject saveProfileScreen;

    [Header("Aniamtor")]
    public Animator logoTitleAnimator;
    public Animator mainMenuScreenAnimator;
    public Animator saveProfileScreenAnimator;
    public Animator audioMenuScreenAnimator;

    [SerializeField] private int maxIndex;
    private GameSceneManager gameSceneManager;
    private MenuButton[] menuButtons;

    private void Awake()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();
    }

    void Start()
    {
        menuButtons = GetComponentsInChildren<MenuButton>();
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
        menuButtons[index].animator.SetTrigger("pressed");
        Pressed();
    }

    public void Pressed()
    {
        switch (index)
        {
            case (int)MenuButtonController.MainMenuButton.START:
                StartCoroutine(DelayDisplaySaveProfileScreen());
                break;
            case (int)MenuButtonController.MainMenuButton.CGREPLAY:
                StartCoroutine(LoadAsyncCGScene());
                break;
            case (int)MenuButtonController.MainMenuButton.OPTIONS:
                StartCoroutine(DelayDisplayAudioMenu());
                break;
            case (int)MenuButtonController.MainMenuButton.QUIT:
                Application.Quit();
                break;
        }
    }

    IEnumerator DelayDisplaySaveProfileScreen()
    {
        logoTitleAnimator.Play("FadeOut");
        mainMenuScreenAnimator.Play("FadeOut");
        yield return new WaitForSeconds(.5f);
        saveProfileScreenAnimator.Play("FadeIn");
    }

    IEnumerator DelayDisplayAudioMenu()
    {
        logoTitleAnimator.Play("FadeOut");
        mainMenuScreenAnimator.Play("FadeOut");
        yield return new WaitForSeconds(.5f);
        audioMenuScreenAnimator.Play("FadeIn");
    }

    IEnumerator LoadAsyncCGScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Opening Sequence No Game");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
