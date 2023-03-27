using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandboxFamily
{
    public class SandboxFamilyBehavior : CampaignBehaviorBase
    {
        private bool isCreated = false;
        private bool isLeveled = false;

        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(AddPlayerFamily));
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(LevelPlayerFamily));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("PlayerFamilyCreated", ref isCreated);
            dataStore.SyncData("PlayerFamilyLeveled", ref isLeveled);
        }

        public void AddPlayerFamily(MobileParty party, Settlement settlement, Hero hero)
        {
            if (!isCreated && Hero.MainHero == hero)
            {
                isCreated = true;
                foreach(var familyMemberData in SandboxFamilyModel.Family)
                {
                    Hero familyMemberHero = familyMemberData.Create();
                    ResetToLevel1(familyMemberHero);
                    RandomizeTraits(familyMemberHero, Hero.MainHero);
                    CampaignEventDispatcher.Instance.OnHeroCreated(familyMemberHero);
                    familyMemberHero.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(familyMemberHero, settlement);
                }
            }
        }

        public void LevelPlayerFamily(MobileParty party, Settlement settlement)
        {
            if (isCreated && !isLeveled && party.IsMainParty)
            {
                isLeveled = true;
                foreach (var kin in Hero.MainHero.Clan.Heroes.Where(x => !x.IsHumanPlayerCharacter && x.Level == 0))
                {
                    foreach (var skill in Skills.All)
                    {
                        int focus = kin.HeroDeveloper.GetFocus(skill);
                        kin.HeroDeveloper.ChangeSkillLevel(skill, focus * 10, false);
                    }
                }
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
            return heroComesOfAge < hero.Age || (heroComesOfAge == hero.Age && CampaignTime.Now.GetDayOfYear < hero.BirthDay.GetDayOfYear);
        }

        // taken from HeroGenerator.AddRandomVarianceToTraits
        private static void RandomizeTraits(Hero hero, Hero template)
        {
            foreach (TraitObject trait in TraitObject.All)
            {
                if (trait != DefaultTraits.Honor && trait != DefaultTraits.Mercy && trait != DefaultTraits.Generosity && trait != DefaultTraits.Valor && trait != DefaultTraits.Calculating)
                {
                    continue;
                }

                int num = template.GetTraitLevel(trait);
                float num2 = MBRandom.RandomFloat;

                if ((double)num2 < 0.20)
                {
                    num--;
                    if (num < -1)
                    {
                        num = -1;
                    }
                }

                if ((double)num2 > 0.80)
                {
                    num++;
                    if (num > 1)
                    {
                        num = 1;
                    }
                }

                num = MBMath.ClampInt(num, trait.MinValue, trait.MaxValue);
                hero.SetTraitLevel(trait, num);
            }
        }
    }
}
