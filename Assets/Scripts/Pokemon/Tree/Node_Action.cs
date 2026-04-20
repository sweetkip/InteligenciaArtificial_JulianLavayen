using System;

public class Node_Action : Node_Decision
{
    private Action<PKMNController> action;

    public Node_Action(Action<PKMNController> action)
    {
        this.action = action;
    }

    public override void Evaluate(PKMNController pkmn, PKMNContext context)
    {
        action(pkmn);
    }
}