using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace BetterChefPrimary
{
    public class AddCleaverToTracker : MonoBehaviour
    {
        public void Start()
        {
            var esm = EntityStateMachine.FindByCustomName(
                GetComponent<ProjectileController>().owner,
                BetterDiceMainStateMachine.EntityStateMachineCustomName
            );
            if (esm && esm.state != null && esm.state is BetterDiceMainStateMachine betterDiceEsm && betterDiceEsm.isAuthority)
            {
                betterDiceEsm.AddCleaver(GetComponent<CleaverProjectile>());
            }
        }
    }
}
