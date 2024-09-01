#if DEBUG
using System.Reflection;
using BetterChefPrimary;
using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace HotCompilerNamespace
{
    public class HotReloadMain
    {
        const BindingFlags allFlags = (BindingFlags)(-1);

        public static void HotReloadEntryPoint()
        {
            // This is just for being able to call self.OnEnter() inside hooks.
            {
                new ILHook(typeof(HotReloadMain).GetMethod(nameof(BaseStateOnEnterCaller), allFlags), BaseStateOnEnterCallerMethodModifier);
            }

            {
                var methodToReload = typeof(BetterDice).GetMethod(nameof(BetterDice.OnEnter), allFlags);
                var newMethod = typeof(HotReloadMain).GetMethod(nameof(BetterDiceOnEnterHotReloaded), allFlags);
                new Hook(methodToReload, newMethod);
            }
        }

        // This is just for being able to call self.OnEnter() inside hooks.
        private static void BaseStateOnEnterCaller(BaseState self)
        {

        }

        // This is just for being able to call self.OnEnter() inside hooks.
        private static void BaseStateOnEnterCallerMethodModifier(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(BaseState).GetMethod(nameof(BaseState.OnEnter), allFlags));
        }

        private static void BetterDiceOnEnterHotReloaded(BetterDice self)
        {
            self.CacheOriginalFields();

            BaseStateOnEnterCaller(self);

            if (!self.chefController)
            {
                self.chefController = self.GetComponent<ChefController>();
            }

            self.chefController.characterBody = self.characterBody;

            self.chefController.spreadBloom = self.bloom;

            self.hasBoost = self.characterBody.HasBuff(DLC2Content.Buffs.Boosted);

            if (self.hasBoost)
            {
                self.damageCoefficient = self.boostedDamageCoefficient;
                if (NetworkServer.active)
                {
                    self.characterBody.RemoveBuff(DLC2Content.Buffs.Boosted);
                }
            }

            self.chefController.NetworkcatchDirtied = false;
            self.chefController.recallCleaver = false;

            Ray aimRay = self.GetAimRay();
            TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref aimRay, self.approximateCleaverDistance, self.gameObject, 1f);

            self.duration = self.baseDuration / self.attackSpeedStat;

            self.StartAimMode(self.duration + 2f, false);

            self.PlayThrowCleaverAnimation();

            self.AddRecoil(-1f * self.recoilAmplitude, -1.5f * self.recoilAmplitude, -0.25f * self.recoilAmplitude,
                0.25f * self.recoilAmplitude);

            self.DoMouthMuzzle();

            self.FireCleaverProjectile(aimRay);
        }
    }
}
#endif