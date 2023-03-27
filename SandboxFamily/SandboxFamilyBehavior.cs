using System;
using System.Collections.Generic;
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
        private int unspentAttributePointsForSpouse = 0;
        private int unspentFocusPointsForSpouse = 0;

        private static readonly List<int> s_hairStyles = new List<int>
        {
            (int)FacesFemaleHair.Long_over_shoulder,
            (int)FacesFemaleHair.Tied_long_over_shoulder,
            //(int)FacesFemaleHair.Above_shoulder_length,
            (int)FacesFemaleHair.Tied_in_back,
            (int)FacesFemaleHair.Shoulder_length_tied,
            (int)FacesFemaleHair.Braided_above_ears,
            (int)FacesFemaleHair.Ukrainian,
            15,
            16,
        };

        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, new Action(OnStartUp));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(AddPlayerSister));
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(LevelPlayerSister));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("PlayerSisterCreated", ref isCreated);
            dataStore.SyncData("PlayerSisterLeveled", ref isLeveled);
            dataStore.SyncData("PlayerSpouseAttributePoints", ref unspentAttributePointsForSpouse);
            dataStore.SyncData("PlayerSpouseFocusPoints", ref unspentFocusPointsForSpouse);
        }

        public void OnStartUp()
        {
            unspentAttributePointsForSpouse = Hero.MainHero.HeroDeveloper.UnspentAttributePoints;
            unspentFocusPointsForSpouse = Hero.MainHero.HeroDeveloper.UnspentFocusPoints;
        }

        public void AddPlayerSister(MobileParty party, Settlement settlement, Hero hero)
        {
            if (!isCreated && Hero.MainHero == hero)
            {
                isCreated = true;

                if (Hero.MainHero.Age < 35)
                {
                    Hero sister1 = CreateSisterFrom(hero, 2.1f);
                    sister1.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(sister1, settlement);

                    Hero sister2 = CreateSisterFrom(hero, -1.5f);
                    sister2.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(sister2, settlement);
                }
                else
                {
                    Hero spouse = CreateSpouseFrom(hero, 2f);
                    spouse.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(spouse, settlement);

                    Hero daughter1 = CreateDaughterFrom(hero, -18.5f);
                    daughter1.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(daughter1, settlement);

                    Hero daughter2 = CreateDaughterFrom(hero, -20f);
                    daughter2.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(daughter2, settlement);

                    Hero daughter3 = CreateDaughterFrom(hero, -21.9f);
                    daughter3.ChangeState(Hero.CharacterStates.Active);
                    EnterSettlementAction.ApplyForCharacterOnly(daughter3, settlement);
                }
            }
        }

        public void LevelPlayerSister(MobileParty party, Settlement settlement)
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

        private Hero CreateSisterFrom(Hero main, float ageDifference)
        {
            HeroCreator.CreateBasicHero(CharacterObject.Find("player_sister_template"), out Hero sister);
            sister.CharacterObject.IsFemale = true;
            sister.CharacterObject.Culture = main.Culture;
            sister.BornSettlement = main.BornSettlement;
            sister.Clan = main.Clan;
            sister.Mother = main.Mother;
            sister.Father = main.Father;
            NameGenerator.Current.GenerateHeroNameAndHeroFullName(sister, out var firstName, out var fullName, false);
            sister.SetName(fullName, firstName);
            sister.SetBirthDay(main.BirthDay - CampaignTime.Years(ageDifference));
            sister.ModifyPlayersFamilyAppearance(BodyProperties.GetRandomBodyProperties(sister.CharacterObject.Race, sister.IsFemale, main.Mother.BodyProperties, main.Father.BodyProperties, 1, MBRandom.RandomInt(), sister.HairTags, sister.BeardTags, sister.TattooTags).StaticProperties);
            sister.ModifyHair(s_hairStyles.GetRandomElement(), 0, 0);
            ResetToLevel1(sister);
            RandomizeTraits(sister, main);
            CampaignEventDispatcher.Instance.OnHeroCreated(sister);
            return sister;
        }

        private Hero CreateSpouseFrom(Hero main, float ageDifference)
        {
            HeroCreator.CreateBasicHero(CharacterObject.Find(main.CharacterObject.IsFemale ? "spc_wanderer_empire_1" : "spc_wanderer_empire_7"), out Hero spouse);
            spouse.BornSettlement = Campaign.Current.Settlements.GetRandomElementWithPredicate(x => x.Culture == spouse.CharacterObject.Culture);
            spouse.Clan = main.Clan;
            spouse.Spouse = main;
            NameGenerator.Current.GenerateHeroNameAndHeroFullName(spouse, out var firstName, out var fullName, false);
            spouse.SetName(fullName, firstName);
            spouse.SetBirthDay(main.BirthDay - CampaignTime.Years(ageDifference));
            ResetToLevel1(spouse, true);
            RandomizeTraits(spouse, main);
            CampaignEventDispatcher.Instance.OnHeroCreated(spouse);
            return spouse;
        }

        private Hero CreateDaughterFrom(Hero main, float ageDifference)
        {
            ageDifference = Math.Min(-18.0f, ageDifference);
            HeroCreator.CreateBasicHero(CharacterObject.Find("player_sister_template"), out Hero daughter);
            daughter.CharacterObject.IsFemale = true;
            daughter.CharacterObject.Culture = main.Culture;
            daughter.BornSettlement = main.BornSettlement;
            daughter.Clan = main.Clan;
            daughter.Mother = main;
            daughter.Father = main.Spouse;
            NameGenerator.Current.GenerateHeroNameAndHeroFullName(daughter, out var firstName, out var fullName, false);
            daughter.SetName(fullName, firstName);
            daughter.SetBirthDay(main.BirthDay - CampaignTime.Years(ageDifference));
            daughter.ModifyPlayersFamilyAppearance(BodyProperties.GetRandomBodyProperties(daughter.CharacterObject.Race, daughter.IsFemale, main.BodyProperties, main.Spouse.BodyProperties, 1, MBRandom.RandomInt(), daughter.HairTags, daughter.BeardTags, daughter.TattooTags).StaticProperties);
            daughter.ModifyHair(s_hairStyles.GetRandomElement(), 0, 0);
            ResetToLevel1(daughter);
            RandomizeTraits(daughter, main);
            CampaignEventDispatcher.Instance.OnHeroCreated(daughter);
            return daughter;
        }

        private void ResetToLevel1(Hero hero, bool bCopyFromMain = false)
        {
            hero.HeroDeveloper.ClearHero();
            hero.HeroDeveloper.SetInitialLevel(1);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Vigor, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Control, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Endurance, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Cunning, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Social, 2, false);
            hero.HeroDeveloper.AddAttribute(DefaultCharacterAttributes.Intelligence, 2, false);
            hero.HeroDeveloper.UnspentAttributePoints = bCopyFromMain ? unspentAttributePointsForSpouse + 5 : 6; // same as main character creation
            hero.HeroDeveloper.UnspentFocusPoints = bCopyFromMain ? unspentFocusPointsForSpouse + 10 : 12; // same as main character creation
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
