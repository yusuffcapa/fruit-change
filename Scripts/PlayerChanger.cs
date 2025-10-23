using UnityEngine; 
using UnityEngine.UI; 
using TMPro; 
using UnityEngine.EventSystems; 
using System.Collections.Generic;

public class PlayerChanger : MonoBehaviour
{
    [System.Serializable]
    public class PlayerForm
    {
        public string tag;
        public Sprite sprite;
    }

    [SerializeField] private GameManager gm;
    [SerializeField] private PlayerForm[] forms;
    [SerializeField] private Image bombTimerBar;
    [SerializeField] private float fakeFormDuration;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI GameOverHighScoreText;
    [SerializeField] private TextMeshProUGUI GameOverScoreText;
    [SerializeField] private TextMeshProUGUI GameStartHighScoreText;
    private SpriteRenderer spriteRenderer;

    public int score = 0;
    private int highScore = 0;
    [SerializeField] public int nextSpeedUpScore;
    [SerializeField] public int nextSpawnRateScore;

    public bool isGameOver = false;
    private bool canChangeForm = true;
    public bool isTouchingObject = false;
    public bool isFakeFormActive = false;
    private float fakeFormTimer = 0f;
    private PlayerForm realFormAfterFake;
    private bool bombUsedRecently = false;
    private bool isTiktokSoundPlaying = false;
    private string lastFormTag = "";
    private int bombSpawnerThreshold;
    public bool isSoundOn = true;
    public AudioSource audioSource;  
    public AudioClip changeSound;
    public AudioClip fleshSound;
    public AudioClip tiktokSound;   

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateScoreUI();
        isFakeFormActive = false;
        bombSpawnerThreshold = Random.Range(15, 25);

        if (bombTimerBar != null)
            bombTimerBar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gm.gameStarted || isGameOver || Time.timeScale == 0f) return;

        // Fake form süresi kontrolü
        if (isFakeFormActive)
        {

            fakeFormTimer -= Time.deltaTime;

            if (bombTimerBar != null)
            {
                bombTimerBar.fillAmount = fakeFormTimer / fakeFormDuration;

                if (!isTiktokSoundPlaying)
                {
                    audioSource.clip = tiktokSound;
                    audioSource.Play();
                    isTiktokSoundPlaying = true;
                }

            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Touch touch = Input.GetTouch(0);
                Ray ray = Camera.main.ScreenPointToRay(touch.position);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("SpecialObject") && hit.collider.GetComponent<SpecialObject>() != null)
                    {
                        
                        return;
                    }
                    audioSource.clip = changeSound;
                    audioSource.Play();
                        
                    ChangeForm();
                    return;
                }

               
                gm.GameOver();
                return;
            }

            if (fakeFormTimer <= 0f)
            {
                isFakeFormActive = false;
                ApplyRealFormAfterFake();

                
                if (bombTimerBar != null)
                    bombTimerBar.gameObject.SetActive(false);

                if (isTiktokSoundPlaying)
                {
                    audioSource.Stop();
                    audioSource.clip = null;
                    isTiktokSoundPlaying = false;
                }
            }

            return;
        }

       
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && canChangeForm && !IsTouchOverUI(touch))
            {
                
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null && hit.collider.CompareTag("SpecialObject") && hit.collider.GetComponent<SpecialObject>() != null)
                    {
                        
                        return;
                    }
                }

                
                ChangeForm();
                canChangeForm = false;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                canChangeForm = true;
            }
        }
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;

        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource source in allAudioSources)
        {
            source.mute = !isSoundOn;
        }

    }

    private bool IsTouchOverUI(Touch touch)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touch.position;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        return raycastResults.Count > 0;
    }

    public void ChangeForm()
    {
        if (!gm.gameStarted || Time.timeScale == 0f || isTouchingObject || isFakeFormActive) return;

        if (forms == null || forms.Length == 0)
        {
            Debug.LogWarning("❗ forms dizisi boş!");
            return;
        }

        PlayerForm chosenForm;
        int safetyCounter = 0;

        bool canSpawnBomb = score > bombSpawnerThreshold;
        if (!canSpawnBomb)
        {
            do
            {
                chosenForm = forms[Random.Range(0, forms.Length)];
                safetyCounter++;
            } while ((chosenForm.tag == lastFormTag || chosenForm.tag == "Bomb") && safetyCounter < 20);
        }
        else
        {
            do
            {
                chosenForm = forms[Random.Range(0, forms.Length)];
                safetyCounter++;
            } while (chosenForm.tag == lastFormTag && safetyCounter < 20);
        }

        if (chosenForm.tag == "Bomb" && canSpawnBomb && !bombUsedRecently)
        {
            isFakeFormActive = true;
            fakeFormDuration = Random.Range(0.1f, 0.5f); 
            fakeFormTimer = fakeFormDuration;
            spriteRenderer.sprite = chosenForm.sprite;

            realFormAfterFake = GetRandomRealFormExcluding("Bomb");

            Time.timeScale = 0.2f;
            
            if (bombTimerBar != null)
            {
                bombTimerBar.gameObject.SetActive(true);
                bombTimerBar.fillAmount = 1f;
            }

            bombUsedRecently = true; 

            return;
        }

        gameObject.tag = chosenForm.tag;
        spriteRenderer.sprite = chosenForm.sprite;
        lastFormTag = chosenForm.tag;

        bombUsedRecently = false;

        audioSource.PlayOneShot(changeSound);
    }

    private PlayerForm GetRandomRealFormExcluding(string excludedTag)
    {
        PlayerForm newForm;
        int attempts = 0;
        do
        {
            newForm = forms[Random.Range(0, forms.Length)];
            attempts++;
        } while (newForm.tag == excludedTag && attempts < 10);

        return newForm;
    }

    private void ApplyRealFormAfterFake()
    {
        if (realFormAfterFake != null)
        {
            gameObject.tag = realFormAfterFake.tag;
            spriteRenderer.sprite = realFormAfterFake.sprite;
        }

        Time.timeScale = 1f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!gm.gameStarted || isTouchingObject) return; 

        if (isTouchingObject && other.CompareTag("Bomb"))
            gm.GameOver();

        isTouchingObject = true;  

        if (other.CompareTag("SpecialObject")) return;

        if (other.CompareTag(gameObject.tag))
        {
            bombUsedRecently = false;  
            score++;  
            audioSource.PlayOneShot(fleshSound);
            int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

            if (score > currentHighScore)
            {
                PlayerPrefs.SetInt("HighScore", score);
                PlayerPrefs.Save();
                highScore = score;
            }

            UpdateScoreUI();


            if (score >= nextSpeedUpScore)
            {
                gm.spawnerManager.IncreaseFallSpeed(0.2f);
            }
            if (score >= nextSpawnRateScore)
            {
                gm.spawnerManager.DecreaseSpawnInterval(0.4f);
            }
        }
        else
        {
            isGameOver = true;
            gm.GameOver();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        isTouchingObject = false;
    }
    public void UpdateScoreUI()
    {
        scoreText.text = "" + score;
        GameOverScoreText.text = "" + score;
        GameOverHighScoreText.text = "" + highScore;
        GameStartHighScoreText.text = "" + highScore;
    }
    public int GetScore()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    public void ResetState()
    {
        isGameOver = false;
        isFakeFormActive = false;
        fakeFormTimer = 0f;
        realFormAfterFake = null;

        ResetScore();
        SetInitialForm();

        if (bombTimerBar != null)
            bombTimerBar.gameObject.SetActive(false);
    }
    
    private void SetInitialForm()
    {
        PlayerForm initialForm = GetRandomRealFormExcluding("Bomb");

        gameObject.tag = initialForm.tag;
        spriteRenderer.sprite = initialForm.sprite;
        lastFormTag = initialForm.tag;
    }


}
