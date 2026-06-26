using System;
using UnityEngine;

public class PKMNDecisionTree : MonoBehaviour
{
    private Node_Decision rootNode;

    public static Node_Decision CreateAggresiveTree()
    {
        //Actions
        Node_Action arrive = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Arrive));
        Node_Action pursue = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Pursue));
        Node_Action wander = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Wander));

        //If close, it stops. Then ask if it sees the player. If true pursuits it, if not wanders
        Node_Question closeCheck = new Node_Question(ctx => Vector3.Distance(ctx.self.position, ctx.player.position) < 3f,
                                                     arrive, pursue);
        return new Node_Question(ctx => ctx.los.LOS(ctx.self, ctx.player), closeCheck, wander);
    }

    public static Node_Decision CreateCowardTree()
    {
        //Actions
        Node_Action evade = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Evade));
        Node_Action wander = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Wander));

        //If sees the player, evades it. If not wanders
        return new Node_Question(ctx => ctx.los.LOS(ctx.self, ctx.player), evade, wander);
    }

    public static Node_Decision CreatePanicTree()
    {
        Node_Action flee = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Flee));
        Node_Action evade = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Evade));
        Node_Action wander = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Wander));

        Node_Question proximityCheck = new Node_Question(ctx => Vector3.Distance(ctx.self.position, ctx.player.position) < 4f, flee, evade);

        return new Node_Question(ctx => ctx.los.LOS(ctx.self, ctx.player), proximityCheck, wander);
    }

    public static Node_Decision CreateIdleTree()
    {
        var options = new (float weight, Action<PKMNController> action)[]
        {
            (0.7f, pkmn => pkmn.SetState(PKMNController.State.Arrive)),
            (0.3f, pkmn => pkmn.SetState(PKMNController.State.Wander))
        };

        return new Node_WeightedRandomAction(options);
    }

    public static Node_Decision CreateAttackTree(float attackRange)
    {
        Node_Action attack = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Attack));
        Node_Action pursue = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Pursue));
        Node_Action wander = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Wander));

        Node_Question distCheck = new Node_Question(ctx => Vector3.Distance(ctx.self.position, ctx.player.position) <= attackRange, attack, pursue);

        return new Node_Question(ctx => ctx.los.LOS(ctx.self, ctx.player), distCheck, wander);
    }

    public static Node_Decision CreateTowerTree()
    {
        Node_Action toLake = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.ToLake));
        Node_Action tower = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Tower));

        return new Node_Question(ctx => ctx.los.LOS(ctx.self, ctx.player), toLake, tower);
    }

    public static Node_Decision CreateSandygastTree()
    {
        Node_Action triggerDig = new Node_Action(pkmn => pkmn.StartSandygastSubmergeAndPathfind());
        Node_Action stayIdle = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Sandygast_Idle));

        Node_Question losCheck = new Node_Question(ctx => ctx.los.LOS(ctx.self, ctx.player), triggerDig, stayIdle);

        return new Node_Question(
            ctx =>
            {
                var controller = ctx.self.GetComponent<PKMNController>();
                return controller != null && controller.CanSearchPlayer;
            },
            losCheck,
            stayIdle);
    }

    public static Node_Decision CreateGimmighoulTree()
    {
        Node_Action search = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Gimmighoul_SearchCoin));
        Node_Action move = new Node_Action(pkmn => pkmn.SetState(PKMNController.State.Gimmighoul_MovingToCoin));

        return new Node_Question(
            ctx => {
                var controller = ctx.self.GetComponent<PKMNController>();
                return controller != null && controller.HasTargetCoin;
            },
            move, search
        );
    }
}