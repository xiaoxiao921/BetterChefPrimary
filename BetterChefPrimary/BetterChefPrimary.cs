using BepInEx;
#if DEBUG
using HotCompilerNamespace;
#endif
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static BetterChefPrimary.BetterDiceMainStateMachine;

namespace BetterChefPrimary
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class BetterChefPrimary : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "iDeathHD";
        public const string PluginName = "BetterChefPrimary";
        public const string PluginVersion = "1.0.2";

        internal static void AddBetterPrimary()
        {
            const string BetterDiceSkillDefToken = "IDEATHHD_BETTERCHEFPRIMARY_BETTERDICE";
            const string BetterDiceDescriptionSkillDefToken = "IDEATHHD_BETTERCHEFPRIMARY_BETTERDICE_DESC";

            var chefBody = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/ChefBody.prefab").WaitForCompletion();

            // add the better dice skill def to chef
            {

                LanguageAPI.Add(BetterDiceSkillDefToken, "Better Dice");
                LanguageAPI.Add(BetterDiceDescriptionSkillDefToken,
                    "Throw a cleaver through enemies for <style=cIsDamage>250% damage</style>. " +
                    "Dealing <style=cIsDamage>375% damage</style> on the return trip.");

                var skillLocator = chefBody.GetComponent<SkillLocator>();

                var originalDiceSkillDef = skillLocator.primary.skillFamily.variants[0].skillDef;

                var skillDef = GameObject.Instantiate(originalDiceSkillDef);

                skillDef.mustKeyPress = false;
                skillDef.hideStockCount = true;
                skillDef.stockToConsume = 0;
                skillDef.baseMaxStock = 1;
                skillDef.dontAllowPastMaxStocks = true;
                skillDef.rechargeStock = 0;
                skillDef.requiredStock = 0;

                skillDef.skillNameToken = BetterDiceSkillDefToken;
                (skillDef as ScriptableObject).name = skillDef.skillNameToken;
                skillDef.skillDescriptionToken = BetterDiceDescriptionSkillDefToken;

                skillDef.activationState = new(typeof(BetterDice));
                ContentAddition.AddSkillDef(skillDef);
                ContentAddition.AddEntityState(typeof(BetterDice), out _);

                BetterDice.BoostedCleaverPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/ChefDiceEnhanced.prefab").WaitForCompletion();
                BetterDice.BoostedCleaverPrefab.AddComponent<AddCleaverToTracker>();

                BetterDice.CleaverPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/ChefCleaver.prefab").WaitForCompletion();
                BetterDice.CleaverPrefab.AddComponent<AddCleaverToTracker>();

                HG.ArrayUtils.ArrayAppend(ref skillLocator.primary.skillFamily.variants, new SkillFamily.Variant
                {
                    skillDef = skillDef,
                    viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false)
                });

                ChefSkillDefs.BetterPrimary = skillDef;
            }

            // esm for tracking cleavers
            {
                var esm = chefBody.AddComponent<EntityStateMachine>();
                esm.customName = BetterDiceMainStateMachine.EntityStateMachineCustomName;
                esm.initialStateType = new(typeof(BetterDiceMainStateMachine));
                esm.mainStateType = new(typeof(BetterDiceMainStateMachine));

                ContentAddition.AddEntityState(typeof(BetterDiceMainStateMachine), out _);

                HG.ArrayUtils.ArrayAppend(ref chefBody.GetComponent<NetworkStateMachine>().stateMachines, esm);
            }
        }

        public void Awake()
        {
            Log.Init(Logger);

            AddBetterPrimary();

#if DEBUG
            HotCompiler.CompileIt();
#endif
        }

#if DEBUG
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                HotCompiler.CompileIt();
            }
        }
#endif
    }
}
