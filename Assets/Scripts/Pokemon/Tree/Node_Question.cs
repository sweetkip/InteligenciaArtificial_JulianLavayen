using System;

public class Node_Question : Node_Decision
{
    private Func<PKMNContext, bool> question;
    private Node_Decision trueNode;
    private Node_Decision falseNode;

    public Node_Question(Func<PKMNContext, bool> question,
                         Node_Decision trueNode,
                         Node_Decision falseNode)
    {
        this.question = question;
        this.trueNode = trueNode;
        this.falseNode = falseNode;
    }

    public override void Evaluate(PKMNController pkmn, PKMNContext context)
    {
        if (question(context))
        {
            trueNode.Evaluate(pkmn, context);
        }
        else
        {
            falseNode.Evaluate(pkmn, context);
        }
    }
}