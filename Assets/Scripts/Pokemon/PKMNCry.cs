using UnityEngine;

[RequireComponent (typeof(AudioSource))]
public class PKMNCry : MonoBehaviour
{
    [Header("Cry")]
    [SerializeField] private AudioClip cry;
    [SerializeField] private float minTime = 5f;
    [SerializeField] private float maxTime = 10f;

    private AudioSource audioSource;
    private float timer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ResetTime();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PlayCry();
            ResetTime();
        }
    }

    private void PlayCry()
    {
        audioSource.PlayOneShot(cry);
    }

    private void ResetTime()
    {
        timer = Random.Range(minTime, maxTime);
    }
}
