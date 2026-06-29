using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private List<Node> allowedNodes = new List<Node>();

    private GameObject currentCoinInstance;

    private void Start()
    {
        SpawnNewCoin();
    }

    public void SpawnNewCoin()
    {
        if (allowedNodes == null || allowedNodes.Count == 0)
            return;

        Node randomNode = allowedNodes[Random.Range(0, allowedNodes.Count)];

        Vector3 spawnPos = randomNode.transform.position;
        spawnPos.y += 0.5f;

        if(currentCoinInstance != null)
            Destroy(currentCoinInstance);

        currentCoinInstance = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        currentCoinInstance.tag = "GimmighoulCoin";

        var trigger = currentCoinInstance.GetComponent<Collider>();
        if (trigger == null)
            trigger = currentCoinInstance.AddComponent<BoxCollider>();
        trigger.isTrigger = true;

        var detector = currentCoinInstance.AddComponent<CoinTriggerDetector>();
        detector.manager = this;
    }
}

public class CoinTriggerDetector : MonoBehaviour
{
    public CoinManager manager;

    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponent<PKMNController>();
        if (controller != null)
        {
            controller.OnCoinCollected();
            manager.SpawnNewCoin();
            Destroy(gameObject);
        }
    }
}