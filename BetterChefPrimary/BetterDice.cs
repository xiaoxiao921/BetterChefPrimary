using EntityStates;
using EntityStates.Chef;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterChefPrimary
{
    public class BetterDice : BaseState
    {
        public static GameObject CleaverPrefab;
        public static GameObject BoostedCleaverPrefab;

        // start serde fields

        public GameObject effectPrefab;

        public GameObject effectEnhancedPrefab;

        public float baseDuration = 2f;

        public float damageCoefficient = 1.2f;

        public float boostedDamageCoefficient = 1.2f;

        public float force = 20f;

        public string attackString;

        public static string returnString;

        public string yesChefAttackString;

        public static string yesChefReturnString;

        public float recoilAmplitude;

        public float bloom;

        public float recallAnimationTransitionTime = 0.2f;

        public float approximateCleaverDistance = 80f;

        // end serde fields

        internal float duration;

        internal ChefController chefController;

        internal bool hasBoost;

        internal void CacheOriginalFields()
        {
            var originalDice = new Dice();

            effectPrefab = originalDice.effectPrefab;
            effectEnhancedPrefab = originalDice.effectEnhancedPrefab;
            baseDuration = originalDice.baseDuration;
            damageCoefficient = originalDice.damageCoefficient;
            boostedDamageCoefficient = originalDice.boostedDamageCoefficient;
            force = originalDice.force;
            attackString = originalDice.attackString;
            returnString = originalDice.returnString;
            yesChefAttackString = originalDice.yesChefAttackString;
            yesChefReturnString = originalDice.yesChefReturnString;
            recoilAmplitude = originalDice.recoilAmplitude;
            bloom = originalDice.bloom;
            recallAnimationTransitionTime = originalDice.recallAnimationTransitionTime;
            approximateCleaverDistance = originalDice.approximateCleaverDistance;
        }

        internal void FireCleaverProjectile(Ray aimRay)
        {
            if (!base.isAuthority)
            {
                return;
            }

            GameObject projectilePrefabToUse;
            int[] cleaverBatch;

            if (this.hasBoost)
            {
                projectilePrefabToUse = BetterDice.BoostedCleaverPrefab;
                cleaverBatch =
                [
                    8,
                    4,
                    4
                ];
            }
            else
            {
                projectilePrefabToUse = BetterDice.CleaverPrefab;
                cleaverBatch =
                [
                    1
                ];
            }


            int cleaverBatchCount = cleaverBatch.Length;
            for (int i = 0; i < cleaverBatchCount; i++)
            {
                int cleaverCount = cleaverBatch[i];

                float num3 = (float)(i % 2) * (0.5f / (float)cleaverCount);

                for (int j = 0; j < cleaverCount; j++)
                {
                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.projectilePrefab = projectilePrefabToUse;
                    fireProjectileInfo.position = aimRay.origin;
                    float f = (num3 + (float)j / (float)cleaverCount) * 3.1415927f * 2f;
                    float f2 = Mathf.Acos(0.02f + (float)i / (float)cleaverBatchCount);
                    float x = Mathf.Sin(f2) * Mathf.Sin(f);
                    float y = Mathf.Cos(f2);
                    float z = Mathf.Sin(f2) * Mathf.Cos(f);
                    Quaternion rhs = Quaternion.LookRotation(new Vector3(x, y, z));
                    fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction) * rhs;
                    fireProjectileInfo.owner = base.gameObject;
                    fireProjectileInfo.damage = this.damageStat * this.damageCoefficient;
                    fireProjectileInfo.damageTypeOverride = new DamageTypeCombo?(DamageType.Generic);
                    fireProjectileInfo.force = this.force;
                    fireProjectileInfo.crit = Util.CheckRoll(this.critStat, base.characterBody.master);
                    if (!NetworkServer.active && this.chefController)
                    {
                        this.chefController.CacheCleaverProjectileFireInfo(fireProjectileInfo);
                    }
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
        }

        public override void OnEnter()
        {
            CacheOriginalFields();

            base.OnEnter();

            if (!this.chefController)
            {
                this.chefController = base.GetComponent<ChefController>();
            }

            this.chefController.characterBody = base.characterBody;

            this.chefController.spreadBloom = this.bloom;

            this.hasBoost = base.characterBody.HasBuff(DLC2Content.Buffs.Boosted);

            if (this.hasBoost)
            {
                this.damageCoefficient = this.boostedDamageCoefficient;
                if (NetworkServer.active)
                {
                    base.characterBody.RemoveBuff(DLC2Content.Buffs.Boosted);
                }
            }

            this.chefController.NetworkcatchDirtied = false;
            this.chefController.recallCleaver = false;

            Ray aimRay = base.GetAimRay();
            TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref aimRay, this.approximateCleaverDistance, base.gameObject, 1f);

            this.duration = this.baseDuration / this.attackSpeedStat;

            base.StartAimMode(this.duration + 2f, false);

            PlayThrowCleaverAnimation();

            base.AddRecoil(-1f * this.recoilAmplitude, -1.5f * this.recoilAmplitude, -0.25f * this.recoilAmplitude,
                0.25f * this.recoilAmplitude);

            DoMouthMuzzle();

            FireCleaverProjectile(aimRay);
        }

        internal void PlayThrowCleaverAnimation()
        {
            if (this.hasBoost)
            {
                base.PlayAnimation("Gesture, Override", "FireSliceAndDice", "FireSliceAndDice.playbackRate", this.duration, 0f);
                base.PlayAnimation("Gesture, Additive", "FireSliceAndDice", "FireSliceAndDice.playbackRate", this.duration, 0f);
                Util.PlaySound(this.yesChefAttackString, base.gameObject);
            }
            else
            {
                base.PlayAnimation("Gesture, Override", "FireDice", "FireDice.playbackRate", this.duration, 0f);
                base.PlayAnimation("Gesture, Additive", "FireDice", "FireDice.playbackRate", this.duration, 0f);
                Util.PlaySound(this.attackString, base.gameObject);
            }
        }

        internal void DoMouthMuzzle()
        {
            const string muzzleName = "MouthMuzzle";
            GameObject exists = this.hasBoost ? this.effectEnhancedPrefab : this.effectPrefab;
            if (exists)
            {
                EffectManager.SimpleMuzzleFlash(exists, base.characterBody.aimOriginTransform.gameObject, muzzleName, false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && this.fixedAge > this.duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            this.chefController.SetYesChefHeatState(false);

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(DLC2Content.Buffs.boostedFireEffect);
            }

            if (base.isAuthority)
            {
                this.chefController.ClearSkillOverrides();
            }

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
