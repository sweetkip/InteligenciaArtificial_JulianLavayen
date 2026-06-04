using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance {  get; private set; }

    [Header("UI Objects")]
    [SerializeField] private GameObject victory;
    [SerializeField] private GameObject defeat;

    private int totalPokemon = 3;
    private int caughtPokemon = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }    
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (victory != null) victory.SetActive(false);
        if (defeat != null) defeat.SetActive(false);
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void TriggerDefeat()
    {
        if (defeat != null)
        {
            defeat.SetActive(true);
            UnlockCursor();
        }    
    }

    public void TriggerVictory()
    {
        if (victory != null)
        {
            victory.SetActive(true);
            UnlockCursor();
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}