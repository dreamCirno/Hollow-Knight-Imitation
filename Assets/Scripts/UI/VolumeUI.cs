using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeUI : MonoBehaviour
{
    public Slider[] Sliders; //滑块集合
    public int[] minVolume;
    public Text[] volText;
    public CanvasGroup mainCanvasGroup;
    public Animator logoTitleAnimator;
    public Animator mainMenuScreenAnimator;

    private AudioManager audioManager;
    private Animator animator;

    private void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        foreach (Slider item in Sliders)
        {
            item.onValueChanged.AddListener((float value) => OnSliderValueChange(value, item));
        }
    }

    private void Update()
    {
        if (mainCanvasGroup != null)
        {
            if (mainCanvasGroup.interactable && Input.GetKeyUp(KeyCode.Escape))
            {
                animator.Play("FadeOut");
            }
        }
    }

    private void OnSliderValueChange(float value, Slider EventSender)
    {
        float volume;
        switch (EventSender.name)
        {
            case "MasterSlider":
                volume = minVolume[0] / 10 * (10 - value);
                audioManager.SetMasterVolume(volume);
                volText[0].text = value.ToString();
                break;
            case "SoundSlider":
                volume = minVolume[1] / 10 * (10 - value);
                audioManager.SetSoundVolume(volume);
                volText[1].text = value.ToString();
                break;
            case "MusicSlider":
                volume = minVolume[2] / 10 * (10 - value);
                audioManager.SetMusicVolume(volume);
                volText[2].text = value.ToString();
                break;
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        PauseMenu.gameIsPaused = false;
        if (logoTitleAnimator != null && mainMenuScreenAnimator != null)
        {
            logoTitleAnimator.Play("FadeIn");
            mainMenuScreenAnimator.Play("FadeIn");
        }
    }
}
