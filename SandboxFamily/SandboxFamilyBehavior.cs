using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandboxFamily
{
    public class SandboxFamilyBehavior : CampaignBehaviorBase
    {
        private bool isCreated = false;
        private Dictionary<string, bool> familyMemberHasLeveled = new Dictionary<string, bool>();

        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, new Action(CreatePlayerFamily));
            CampaignEvents.HeroLevelledUp.AddNonSerializedListener(this, new Action<Hero, bool>(ApplyInitialSkillLevels));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("PlayerFamilyCreated", ref isCreated);
            dataStore.SyncData("PlayerFamilyLeveled", ref familyMemberHasLeveled);
        }

        public void CreatePlayerFamily()
        {
            if (!isCreated)
            {
                isCreated = true;
                foreach(var familyMemberData in SandboxFamilyModel.Family)
                {
                    Hero familyMemberHero = familyMemberData.Create();
                    ResetToLevel1(familyMemberHero);
                    RandomizeTraits(familyMemberHero);
                    CampaignEventDispatcher.Instance.OnHeroCreated(familyMemberHero);
                    familyMemberHero.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(familyMemberHero, Hero.MainHero.BornSettlement);
                    familyMemberHasLeveled.Add(familyMemberHero.StringId, false);
                }
            }
        }

        public void ApplyInitialSkillLevels(Hero hero, bool shouldNotify)
        {
            if (isCreated && hero.Level == 1 && hero.Clan == Clan.PlayerClan && familyMemberHasLeveled.TryGetValue(hero.StringId, out bool hasLeveled) && !hasLeveled)
            {
                foreach (var skill in Skills.All)
                {
                    int focus = hero.HeroDeveloper.GetFocus(skill);
                    hero.HeroDeveloper.ChangeSkillLevel(skill, focus * 10, false);
                }
                familyMemberHasLeveled[hero.StringId] = true;
            }
        }

        private void ResetToLevel1(Hero hero)
        {
            hero.HeroDeveloper.ClearHero();
            hero.HeroDeveloper.SetInitialLevel(1);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Vigor, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Control, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Endurance, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Cunning, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Social, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Intelligence, 2, false);
            if (50 <= hero.Age)
            {
                hero.HeroDeveloper.UnspentAttributePoints = 9;
                hero.HeroDeveloper.UnspentFocusPoints = 18;
            }
            else if (40 <= hero.Age)
            {
                hero.HeroDeveloper.UnspentAttributePoints = 8;
                hero.HeroDeveloper.UnspentFocusPoints = 16;
            }
            else if (30 <= hero.Age)
            {
                hero.HeroDeveloper.UnspentAttributePoints = 7;
                hero.HeroDeveloper.UnspentFocusPoints = 14;
            }
            else if (IsAdult(hero))
            {
                hero.HeroDeveloper.UnspentAttributePoints = 6;
                hero.HeroDeveloper.UnspentFocusPoints = 12;
            }
        }

        private static bool IsAdult(Hero hero)
        {
            int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
            return heroComesOfAge < hero.Age || (heroComesOfAge == hero.Age && hero.BirthDay.GetDayOfYear < CampaignTime.Now.GetDayOfYear);
        }

        // Similar to HeroGenerator.AddRandomVarianceToTraits
        private static void RandomizeTraits(Hero hero)
        {
            Hero template = hero;
            foreach (TraitObject trait in TraitObject.All)
            {
                if (trait != DefaultTraits.Honor && trait != DefaultTraits.Mercy && trait != DefaultTraits.Generosity && trait != DefaultTraits.Valor && trait != DefaultTraits.Calculating)
                {
                    continue;
                }

                if (hero.Mother != null && hero.Father != null)
                    template = MBRandom.RandomInt(0, 100) > 50 ? hero.Mother : hero.Father;

                int traitLevel = template.GetTraitLevel(trait);
                float randomPercent = MBRandom.RandomInt(0, 100);
                if (randomPercent < 15)
                {
                    traitLevel = Math.Max(traitLevel - 1, -1);
                }
                else if (randomPercent > 85)
                {
                    traitLevel = Math.Min(traitLevel + 1, 1);
                }

                traitLevel = MBMath.ClampInt(traitLevel, trait.MinValue, trait.MaxValue);
                hero.SetTraitLevel(trait, traitLevel);
            }
        }
    }
}
