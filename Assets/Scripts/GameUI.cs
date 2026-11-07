using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Slider sizeBar;

    [Header("Старт")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button startQuitButton;

    [Header("Меню (пауза)")]
    [SerializeField] private Button menuButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuRestartButton;
    [SerializeField] private Button quitButton;

    [Header("Кінець гри")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endText;
    [SerializeField] private Button endRestartButton;

    private void Awake()
    {
        menuPanel.SetActive(false);
        endPanel.SetActive(false);
        
        bool isRestart = GameManager.SkipStartScreen;
        startPanel.SetActive(!isRestart);
        menuButton.gameObject.SetActive(isRestart);

        playButton.onClick.AddListener(OnPlayClicked);
        menuButton.onClick.AddListener(() => SetMenu(true));
        resumeButton.onClick.AddListener(() => SetMenu(false));
        menuRestartButton.onClick.AddListener(gameManager.Restart);
        endRestartButton.onClick.AddListener(gameManager.Restart);
        quitButton.onClick.AddListener(Quit);
        startQuitButton.onClick.AddListener(Quit);

        gameManager.GameEnded += ShowEndPanel;
    }

    private void Update()
    {
        sizeBar.value = gameManager.SizeNormalized;
    }

    private void OnPlayClicked()
    {
        startPanel.SetActive(false);
        menuButton.gameObject.SetActive(true);
        gameManager.StartGame();
    }

    private void SetMenu(bool open)
    {
        menuPanel.SetActive(open);
        Time.timeScale = open ? 0f : 1f;
    }

    private void ShowEndPanel(string message)
    {
        menuButton.gameObject.SetActive(false);
        endText.text = message;
        endPanel.SetActive(true);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.GameEnded -= ShowEndPanel;
        }
    }
}
