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
                Instantiate(captureVFX, pkmn.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("It escaped!");
        }
    }
}