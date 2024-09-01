using System.Collections.Generic;
using EntityStates;
using RoR2.Projectile;

namespace BetterChefPrimary
{
    public class BetterDiceMainStateMachine : BaseState
    {
        public const string EntityStateMachineCustomName = "IDEATHHD_BETTERCHEFPRIMARY_ESM";

        public List<CleaverProjectile> ActiveCleavers = [];

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                for (int i = ActiveCleavers.Count - 1; i >= 0; i--)
                {
                    var cleaver = ActiveCleavers[i];
                    if (cleaver)
                    {
                        if (cleaver.boomerangState == CleaverProjectile.BoomerangState.Stopped)
                        {
                            cleaver.boomerangState = CleaverProjectile.BoomerangState.FlyBack;
                            ActiveCleavers.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
