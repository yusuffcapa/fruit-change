using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public bool gameStarted = false;
    public GameObject gameStartPanel;
    public GameObject gameOverPanel;
    public GameObject gamePanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public SpawnerManager spawnerManager;
    public ObjectSpawner spawnerScript;
    public SpecialObject specialSpawnerScript;
    public FormChangingSpawner formChangingSpawner;
    [SerializeField] private PlayerChanger playerScript;
    public AudioClip[] backgroundMusics;
    public AudioSource audioSource;
    private int lastPlayedMusicIndex = -1;


    private bool isPaused = false;
    private bool canRestartAfterGameOver = true;
    private bool isGameOver = false;

    void Start()
    {
        Time.timeScale = 0f;
        gameStarted = false;
        isGameOver = false;

        settingsPanel?.SetActive(false);
        gameStartPanel?.SetActive(true);
        gamePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        pausePanel?.SetActive(false);

        if (spawnerManager != null)
            spawnerManager.gameObject.SetActive(false);

        if (backgroundMusics.Length > 0)
        {
            PlayRandomMusic();
        }
    }

    void Update()
    {
        if (!gameStarted && isGameOver && canRestartAfterGameOver)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (EventSystem.current != null)
                {
                    if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        RestartGame();
                    }
                    else
                    {
                        Debug.Log("Buton üzerinde tıklama yapıldı");
                    }
                }
            }
        }

        if (!audioSource.isPlaying && gameStarted)
        {
            PlayRandomMusic();
        }
    }

    private void PlayRandomMusic()
    {
        if (backgroundMusics.Length == 0) return;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, backgroundMusics.Length);
        } while (backgroundMusics.Length > 1 && randomIndex == lastPlayedMusicIndex);

        lastPlayedMusicIndex = randomIndex;

        audioSource.clip = backgroundMusics[randomIndex];
        audioSource.Play();
    }

    public void StartGame()
    {
        gameStarted = true;
        isGameOver = false;
        Time.timeScale = 1f;

        gameStartPanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        gamePanel?.SetActive(true);

        Camera.main.backgroundColor = GenerateLightColor();

        if (spawnerManager != null)
        {
            spawnerManager.gameObject.SetActive(true);
            spawnerManager.StartSpawning();  // Ana spawner'ı başlat
        }

        if (playerScript != null)
        {
            playerScript.ResetState();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            pausePanel?.SetActive(true);
        }
        else
        {
            pausePanel?.SetActive(false);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameStarted = false;
        isGameOver = true;
        canRestartAfterGameOver = false;

        gamePanel?.SetActive(false);

        if (specialSpawnerScript != null)
        {
            specialSpawnerScript.StopSpawning();
            specialSpawnerScript.ClearSpawnedObjects();
        }

        if (spawnerManager != null)
        {
            spawnerManager.StopSpawning();
            spawnerManager.ClearSpawnedObjects();
        }

        if (playerScript != null)
            playerScript.UpdateScoreUI();

        gameOverPanel?.SetActive(true);
        Debug.Log("💀 Oyun Bitti");

        StartCoroutine(EnableRestartAfterDelay());
    }

    private System.Collections.IEnumerator EnableRestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Debug.Log("✅ Restart artık aktif");
        canRestartAfterGameOver = true;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 0f;
        gameStarted = false;
        isGameOver = false;
        isPaused = false;

        gameOverPanel?.SetActive(false);
        gameStartPanel?.SetActive(true);
        gamePanel.SetActive(false);
        pausePanel?.SetActive(false);
        settingsPanel?.SetActive(false);

        if(specialSpawnerScript != null)
        {
            specialSpawnerScript.StopSpawning();
            specialSpawnerScript.ClearSpawnedObjects();
        } 

        if (spawnerManager != null)
        {
            spawnerManager.StopSpawning();
            spawnerManager.ClearSpawnedObjects();
            spawnerManager.gameObject.SetActive(false);
            spawnerManager.ResetSpawner();
        }

        if (playerScript != null)
        {
            playerScript.ResetScore();
            playerScript.ChangeForm();
        }
        PlayRandomMusic();
    }

    public void RestartGame()
    {
        gameStartPanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        gamePanel?.SetActive(true);
        spawnerManager.ResetSpawner();

        if (specialSpawnerScript != null) 
            specialSpawnerScript.ClearSpawnedObjects();

        if (spawnerManager != null)
            spawnerManager.ClearSpawnedObjects();

        if (playerScript != null)
            playerScript.ResetState();

        StartGame();
    }

    public void Settings()
    {
        settingsPanel?.SetActive(true);
        gameStartPanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        gamePanel?.SetActive(false);
    }

    Color GenerateLightColor()
    {
        float r = Random.Range(0.6f, 1f);
        float g = Random.Range(0.6f, 1f);
        float b = Random.Range(0.6f, 1f);
        return new Color(r, g, b);
    }
}
