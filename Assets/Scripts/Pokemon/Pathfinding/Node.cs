using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neighbours;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        foreach(var neighbour in neighbours)
        {
            if (neighbour != null)
            {
                Gizmos.DrawLine(transform.position, neighbour.transform.position);
            }
        }
    }
}