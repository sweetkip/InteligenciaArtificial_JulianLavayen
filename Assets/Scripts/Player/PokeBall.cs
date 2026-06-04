using UnityEngine;

public class PokeBall : MonoBehaviour
{
    [SerializeField] private float captureChance = 0.5f;
    [SerializeField] private GameObject captureVFX;

    private void OnCollisionEnter(Collision collision)
    {
        PKMNController pkmn = collision.gameObject.GetComponent<PKMNController>();

        if (pkmn != null)
            TryCapture(pkmn);

        Destroy(gameObject);
    }

    private void TryCapture(PKMNController pkmn)
    {
        if (Random.value <= captureChance)
        {
            Debug.Log("Gotcha!");
            pkmn.OnCaptured();

            if (captureVFX)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-15f, 15f),
                        Random.Range(-5f, 5f),
                        Random.Range(-15f, 15f)
                    );

                    GameObject star = Instantiate(
                        captureVFX,
                        pkmn.transform.position + Vector3.up * 25f + offset,
                        Quaternion.Euler(0, 0, Random.Range(0f, 360f))
                    );

                    star.transform.localScale *= Random.Range(0.5f, 1.2f);

                    Destroy(star, 2f);
                }
            }
        }
        else
        {
            Debug.Log("It escaped!");
        }
    }
}