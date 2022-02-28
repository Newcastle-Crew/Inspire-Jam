#region 'Using' information
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
#endregion

public class SettingsMenu : MonoBehaviour
{
    Resolution[] resolutions;
    List<string> options = new List<string>();
    public TMP_Dropdown resolutionDropdown;

    public AudioMixer musicMixer;   // The mixer that controls music volume.
    public AudioMixer SFXMixer;     // The mixer that controls sound effect volume.
    public AudioMixer voiceMixer;   // The mixer that controls voice line volume.

    public Slider musicSlider; // The slider that controls music volume.
    public Slider SFXSlider; // The slider that controls sound effect volume.
    public Slider voiceSlider; // The slider that controls voice line volume.

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    private void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", .5f); // Gets the float value of musicVolume, or uses .5f if it isn't found.
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume", .5f); // Gets the float value of SFXVolume, or uses .5f if it isn't found.
        voiceSlider.value = PlayerPrefs.GetFloat("voiceVolume", .5f); // Gets the float value of voiceVolume, or uses .5f if it isn't found.

        #region Screen Resolution stuff
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i] + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            { currentResolutionIndex = i; }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        #endregion
    }

    public void SetFullscreen(bool isFullscreen)
    { Screen.fullScreen = isFullscreen; }  // Makes the game fullscreen on startup.

    public void SetMusicVolume(float musicVol)
    {
        musicMixer.SetFloat("musicVolume", Mathf.Log10(musicVol) * 20);
        PlayerPrefs.SetFloat("musicVolume", musicVol); // Sets the value of SliderVolume to the music volume value.

        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float sfxVol)
    {
        PlayerPrefs.SetFloat("SFXVolume", sfxVol); // Sets the value of SliderVolume to the sound effect volume value.
        SFXMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVol) * 20);

        PlayerPrefs.Save();
    }

    public void SetVoiceVolume(float voiceVol)
    {
        PlayerPrefs.SetFloat("voiceVolume", voiceVol); // Sets the value of SliderVolume to the voice volume value.
        voiceMixer.SetFloat("voiceVolume", Mathf.Log10(voiceVol) * 20);

        PlayerPrefs.Save();
    }
}