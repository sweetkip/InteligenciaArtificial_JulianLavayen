using System;

public class Node_WeightedRandomAction : Node_Decision
{
    private (float weight, Action<PKMNController> action)[] options;

    public Node_WeightedRandomAction((float weight, Action<PKMNController> action)[] options)
    {
        this.options = options;
    }

    public override void Evaluate(PKMNController pkmn, PKMNContext context)
    {
        float totalWeight = 0f;

        foreach (var option in options)
        {
            totalWeight += option.weight;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var option in options)
        {
            currentWeight += option.weight;

            if (randomValue <= currentWeight)
            {
                option.action(pkmn);
                return;
            }
        }
    }
}