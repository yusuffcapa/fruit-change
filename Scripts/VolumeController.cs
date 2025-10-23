using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public AudioSource audioSource;
    public Slider volumeSlider;
    public AudioClip[] backgroundMusics;
    public PlayerChanger playerChanger;


    private float lastVolume = 1f;
    private const string VolumePrefKey = "GameVolume";
    private int lastPlayedIndex = -1;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        audioSource.volume = savedVolume;
        volumeSlider.value = savedVolume;
        lastVolume = (savedVolume > 0f) ? savedVolume : 1f;

        volumeSlider.onValueChanged.AddListener(VolumeChanged);

        PlayRandomMusic();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayRandomMusic();
        }
    }

    void VolumeChanged(float value)
    {
        audioSource.volume = value;
        if (value > 0f)
        {
            lastVolume = value;
        }

        PlayerPrefs.SetFloat(VolumePrefKey, value);
        PlayerPrefs.Save();

        UpdateAllAudioVolumes(value);
    }

    public void VolumeClose()
    {
        if (audioSource.volume > 0f)
        {
            audioSource.volume = 0f;
            volumeSlider.value = 0f;
            PlayerPrefs.SetFloat(VolumePrefKey, 0f);
        }
        else
        {
            audioSource.volume = lastVolume;
            volumeSlider.value = lastVolume;
            PlayerPrefs.SetFloat(VolumePrefKey, lastVolume);
        }

        PlayerPrefs.Save();

        UpdateAllAudioVolumes(audioSource.volume);
    }

    private void UpdateAllAudioVolumes(float value)
    {
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var source in allSources)
        {
            if (source != audioSource)
                source.volume = value;
        }
    }

    public void PlayRandomMusic()
    {
        if (backgroundMusics.Length == 0) return;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, backgroundMusics.Length);
        } while (backgroundMusics.Length > 1 && randomIndex == lastPlayedIndex);

        lastPlayedIndex = randomIndex;

        AudioClip clipToPlay = backgroundMusics[randomIndex];
        audioSource.clip = clipToPlay;
        audioSource.Play();
    }
}
