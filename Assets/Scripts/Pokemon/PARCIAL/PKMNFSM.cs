using UnityEngine;
//{}
public class PKMNFSM : MonoBehaviour
{
    private PKMNContext context;
    private PKMNControls controller;

    public PKMNFSM(PKMNControls controller, PKMNContext context)
    {
        this.controller = controller;
        this.context = context;
    }

    public void UpdateState()
    {
        bool hasLOS = context.los.LOS(context.self, context.player);

        switch(controller.CurrentPersonality) 
        {
            case PKMNControls.Personality.Sandygast:
                UpdateSandygast(hasLOS);
                break;
            case PKMNControls.Personality.Gimmighoul:
                UpdateGimmighoul();
                break;
        }
    }
    
    private void UpdateSandygast(bool hasLOS)
    {
        switch(controller.CurrentState)
        {
            case PKMNControls.State.Sandygast_Idle:
                if (hasLOS && controller.CanSearchPlayer)
                    controller.SandySubmerge();
                break;
            case PKMNControls.State.Sandygast_Moving:
                break;
        }
    }

    private void UpdateGimmighoul()
    {
        switch(controller.CurrentState)
        {
            case PKMNControls.State.Gimmighoul_SearchCoin:
                if (controller.HasTargetCoin)
                    controller.SetState(PKMNControls.State.Gimmighoul_MovingToCoin);
                break;
            case PKMNControls.State.Gimmighoul_MovingToCoin:
                if (!controller.HasTargetCoin)
                    controller.SetState(PKMNControls.State.Gimmighoul_SearchCoin);
                break;
        }
    }
}