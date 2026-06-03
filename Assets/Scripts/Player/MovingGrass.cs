using System.Collections.Generic;
using UnityEngine;

public class MovingGrass : MonoBehaviour
{
    [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();
    private MaterialPropertyBlock mpb;

    void Start()
    {
        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        for (int i = 0; i < targetRenderers.Count; i++)
        {
            targetRenderers[i].GetPropertyBlock(mpb);
            mpb.SetVector("_SpherePos", this.transform.position);
            targetRenderers[i].SetPropertyBlock(mpb);
        }
    }
}
