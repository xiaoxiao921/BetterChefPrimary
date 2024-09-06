using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace BetterChefPrimary
{
    /// <summary>
    /// Component attached to the Cleaver projectile.
    /// Trace back to the owner gameobject and add the cleaver instance to the tracker, which is handled by a custom entity state machine (attached to the chef body)
    /// </summary>
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
