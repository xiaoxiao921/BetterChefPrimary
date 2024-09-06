using System.Collections.Generic;
using EntityStates;
using RoR2.Projectile;
using RoR2.Skills;

namespace BetterChefPrimary
{
    public class BetterDiceMainStateMachine : BaseState
    {
        public const string EntityStateMachineCustomName = "IDEATHHD_BETTERCHEFPRIMARY_ESM";

        private List<CleaverProjectile> _activeCleavers = [];

        public void AddCleaver(CleaverProjectile cleaverProjectile)
        {
            _activeCleavers.Add(cleaverProjectile);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.skillLocator.primary.skillDef != ChefSkillDefs.BetterPrimary)
            {
                return;
            }

            for (int i = _activeCleavers.Count - 1; i >= 0; i--)
            {
                var cleaver = _activeCleavers[i];
                if (cleaver)
                {
                    if (cleaver.NetworkboomerangState == CleaverProjectile.BoomerangState.Stopped)
                    {
                        cleaver.NetworkboomerangState = CleaverProjectile.BoomerangState.FlyBack;
                        _activeCleavers.RemoveAt(i);
                    }
                }
            }
        }

        public static class ChefSkillDefs
        {
            public static SkillDef BetterPrimary;
        }
    }
}
