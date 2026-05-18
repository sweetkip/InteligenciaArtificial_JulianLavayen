using System;
using System.Collections.Generic;

public class DFS_profe
{
    public static List<Node> Run(Node initialNode, Func<Node, bool> isSatisfied, Func<Node, List<Node>> getConnections)
    {
        Stack<Node> pending = new Stack<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        Dictionary<Node, Node> parents = new Dictionary<Node, Node>();

        pending.Push(initialNode);
        visited.Add(initialNode);

        while (pending.Count > 0)
        {
            Node node = pending.Pop();

            if (isSatisfied(node))
            {
                List<Node> path = new List<Node>();
                path.Add(node);
                Node current = node;

                while (parents.ContainsKey(current))
                {
                    path.Add(parents[current]);
                    current = parents[current];
                }

                path.Reverse();
                return path;
            }
            else
            {
                List<Node> children = getConnections(node);

                for (int i = 0; i < children.Count; ++i)
                {
                    if (visited.Contains(children[i]))
                    {
                        continue;
                    }
                    pending.Push(children[i]);
                    visited.Add(children[i]);
                    parents[children[i]] = node;

                }

            }
        }
        return new List<Node>();
    }
}