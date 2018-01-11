using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    public GameObject MainMenuHolder;
    public GameObject OptionsMenuHolder;

    public Dropdown resolutionDropdown;
    public Slider[] volumeSliders;

    public Toggle fullscreenToggle;

    public Image bgImage;

    Resolution[] screenResolutions;
    Resolution[] uniqueScreenResolutions;

    void Start() {
        LoadPlayerPrefs();
        SetupResolutions();
        MainMenu();
    }

    void Update() {
        bgImage.transform.Rotate(0, 0, Time.deltaTime);
    }

    public void Play() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void Quit() {
        Application.Quit();
    }

    public void OptionsMenu() {
        MainMenuHolder.SetActive(false);
        OptionsMenuHolder.SetActive(true);
    }

    public void MainMenu() {
        MainMenuHolder.SetActive(true);
        OptionsMenuHolder.SetActive(false);
    }

    void LoadPlayerPrefs() {
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        int width = PlayerPrefs.GetInt("ScreenWidth", Screen.width);
        int height = PlayerPrefs.GetInt("ScreenHeight", Screen.height);
        SetResolution(width, height, fullscreenToggle.isOn);

        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;
    }

    void SetupResolutions() {
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(delegate {
            SetFullscreen();
        });

        screenResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> resolutionOptions = new List<string>();
        List<Resolution> uniqueResolutionsList = new List<Resolution>();

        int resolutionIndex = 0;
        int activeResolutionIndex = 0;

        foreach (Resolution res in screenResolutions) {
            string resolutionText = res.width + " x " + res.height;

            if (resolutionOptions.Contains(resolutionText)) {
                continue;
            }

            resolutionOptions.Add(resolutionText);
            uniqueResolutionsList.Add(res);
            if (res.width == Screen.width && res.height == Screen.height) {
                activeResolutionIndex = resolutionIndex;
            }
            resolutionIndex++;
        }
        uniqueScreenResolutions = uniqueResolutionsList.ToArray();
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = activeResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(delegate { SetResolution(resolutionDropdown.value); });
    }

    public void SetResolution(int resolutionIndex) {
        Resolution res = uniqueScreenResolutions[resolutionIndex];
        SetResolution(res.width, res.height, fullscreenToggle.isOn);
    }

    public void SetFullscreen() {
        SetResolution(Screen.width, Screen.height, fullscreenToggle.isOn);
    }

    void SetResolution(int width, int height, bool fullscreen) {
        Screen.SetResolution(width, height, fullscreen);
        PlayerPrefs.SetInt("ScreenWidth", width);
        PlayerPrefs.SetInt("ScreenHeight", height);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float percent) {
        AudioManager.instance.SetVolume(percent, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float percent) {
        AudioManager.instance.SetVolume(percent, AudioManager.AudioChannel.Music);
    }

    public void SetSoundVolume(float percent) {
        AudioManager.instance.SetVolume(percent, AudioManager.AudioChannel.Sfx);
    }
}