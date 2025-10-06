using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Image _speaker;
    [SerializeField] private Sprite[] _speakerSprites;
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private Toggle _speakerToggle;
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private GameObject _settingsMenu;

    private float _previousVolume;
    private string _mainVolumeStr = "MainVolume";

    private Resolution[] _resolutions;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;

    private void Start()
    {
        _fullscreenToggle.isOn = Screen.fullScreen;
        _audioMixer.GetFloat(_mainVolumeStr, out _previousVolume);

        _resolutions = Screen.resolutions;
        _resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = _resolutions[i].width + " x " + _resolutions[i].height;
            options.Add(option);

            if (_resolutions[i].width == Screen.width &&
                _resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResolutionIndex;
        _resolutionDropdown.RefreshShownValue();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _settingsMenu.activeInHierarchy)
        {
            ToMainMenu();
        }
    }

    public void ToggleSound(bool isSoundOn)
    {
        _speaker.sprite = _speakerSprites[isSoundOn ? 0 : 1];
        _audioMixer.SetFloat(_mainVolumeStr, isSoundOn ? _previousVolume : -80);
    }

    public void SetVolume(float volume)
    {
        _previousVolume = volume;
        _audioMixer.SetFloat(_mainVolumeStr, Mathf.Log10(volume) * 20); 
        //_audioMixer.SetFloat(_mainVolumeStr, volume);
        if (volume == -80)
        {
            _speakerToggle.isOn = false;
        }
        else
        {
            _speakerToggle.isOn = true;
        }
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = _resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void ToMainMenu()
    {
        _settingsMenu.SetActive(false);
    }
}
