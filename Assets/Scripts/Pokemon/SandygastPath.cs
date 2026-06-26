using System.Collections.Generic;
using UnityEngine;
//<>
public class SandygastPath : MonoBehaviour
{
    //public enum SandygastMode
    //{
    //    Surface,
    //    Burrowing,
    //    Travelling,
    //    Emerging
    //}
    //public SandygastMode mode;

    //[Header("References")]
    //[SerializeField] private SandygastNodeManager nodeManager;
    //[SerializeField] private Node currentNode;

    //[Header("Surface")]
    //public bool isBuried;
    //public float maxSurfaceTime = 5f;

    //[HideInInspector]
    //public float surfaceTimer;

    //private List<Node> path = new();
    //private int currentIndex;
    
    //public bool HasPath => path.Count > 0 && currentIndex < path.Count;
    //public bool FinishedPath => currentIndex >= path.Count;
    //public Node CurrentNode => currentNode;

    //public void CreateRandomPath()
    //{
    //    Node goal = nodeManager.GetRandomNode(currentNode);

    //    path = AStar.Run(
    //        currentNode,
    //        n => n == goal,
    //        n => n.neighbours,
    //        (a,b) => Vector3.Distance(a.transform.position, b.transform.position),
    //        n => Vector3.Distance(n.transform.position, goal.transform.position)
    //        );

    //    if (path.Count <= 1)
    //    {
    //        currentIndex = path.Count;
    //        return;
    //    }
    //    currentIndex = 1;

    //    Debug.Log($"Nuevo camino hacia {goal.name}. Largo: {path.Count}");
    //}

    //public Vector3 GetCurrentTarget()
    //{
    //    if (!HasPath)
    //        return transform.position;

    //    return path[currentIndex].transform.position;
    //}

    //public void AdvanceNode()
    //{
    //    currentIndex++;

    //    if (currentIndex >= path.Count)
    //        currentNode = path [path.Count - 1];
    //}

    //public void SetCurrentNode(Node node)
    //{
    //    currentNode = node;
    //}

    //public void Tick(PKMNController controller)
    //{
    //    switch (mode)
    //    {
    //        case SandygastMode.Surface:
    //            Surface(controller);
    //            break;

    //        case SandygastMode.Burrowing:
    //            Burrowing(controller);
    //            break;

    //        case SandygastMode.Travelling:
    //            Travelling(controller);
    //            break;

    //        case SandygastMode.Emerging:
    //            Emerging(controller);
    //            break;
    //    }
    //}

    //private void Surface(PKMNController controller)
    //{
    //    surfaceTimer += Time.deltaTime;

    //    if (controller.PlayerDetected())
    //    {
    //        mode = SandygastMode.Burrowing;
    //        return;
    //    }

    //    if (surfaceTimer >= maxSurfaceTime)
    //    {
    //        mode = SandygastMode.Burrowing;
    //    }
    //}

    //private void Burrowing(PKMNController controller)
    //{
    //    bool finished = controller.MoveHeight(false);

    //    if (!finished)
    //        return;

    //    isBuried = true;

    //    CreateRandomPath();

    //    mode = SandygastMode.Travelling;
    //}
}