﻿using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace SandboxFamily
{
    static public class SandboxFamilyModel
    {
        public class FamilyMemberData
        {
            public enum RelationToMain { Spouse, Child, Sibling };
            public bool isFemale;
            public float ageOffset;
            public RelationToMain relationToMain;
            public string template;

            private static readonly List<int> s_femaleHairStyles = new List<int>
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
            private static readonly List<int> s_maleHairStyles = new List<int>
            {
                (int)FacesMaleHair.Ponytail,
                (int)FacesMaleHair.LongAndBushy,
                (int)FacesMaleHair.TiedAcrossBack,
                (int)FacesMaleHair.SlickedShort,
                (int)FacesMaleHair.PartedAndCombedBack,
                (int)FacesMaleHair.Tousled,
                (int)FacesMaleHair.BraidedAndLong,
                (int)FacesMaleHair.Short,
                (int)FacesMaleHair.HighPonytail,
                (int)FacesMaleHair.PageBoy,
                (int)FacesMaleHair.ShortAndThin,
                (int)FacesMaleHair.SlickedLong
            };

            public FamilyMemberData(bool isFemale, float ageOffset, RelationToMain relationToMain)
            {
                this.isFemale = isFemale;
                this.ageOffset = ageOffset;
                this.relationToMain = relationToMain;
                if (relationToMain == RelationToMain.Spouse)
                {
                    template = Hero.MainHero.IsFemale ? "spc_wanderer_empire_1" : "spc_wanderer_empire_7";
                }
                else
                {
                    template = isFemale ? "player_sister_template" : "player_brother_template";
                }
            }

            public Hero Create()
            {
                Hero main = Hero.MainHero;
                HeroCreator.CreateBasicHero(CharacterObject.Find(template), out Hero hero);
                hero.CharacterObject.Culture = main.Culture;
                hero.BornSettlement = main.BornSettlement;
                hero.Clan = main.Clan;
                switch (relationToMain)
                {
                    case RelationToMain.Spouse:
                        hero.Spouse = main;
                        break;
                    case RelationToMain.Child:
                        hero.Mother = main.IsFemale ? main : main.Spouse;
                        hero.Father = main.IsFemale ? main.Spouse : main;
                        break;
                    case RelationToMain.Sibling:
                        hero.Mother = main.Mother;
                        hero.Father = main.Father;
                        break;
                }
                NameGenerator.Current.GenerateHeroNameAndHeroFullName(hero, out var firstName, out var fullName, false);
                hero.SetName(fullName, firstName);
                hero.SetBirthDay(main.BirthDay - CampaignTime.Years(ageOffset));
                // We do not need to change the spouse here, because it uses a fixed preset and is unique.
                if (relationToMain != RelationToMain.Spouse)
                {
                    // The random generation which is attempted here does not work currently.
                    // This seems to be a bug in the main game, which has been confirmed by TaleWorlds.
                    // This code is similar to the code that is used during HeroCreator.DeliverOffSpring and HeroCreator.CreateRelativeNotableHero.
                    // Those two functions should produce randomized characters, but the characters look very similar to their same-gender parent.
                    // See https://forums.taleworlds.com/index.php?threads/same-gender-children-of-the-same-parents-are-all-clones.450790/
                    BodyProperties bodyPropertiesMin = new BodyProperties(new DynamicBodyProperties(hero.Age, hero.Weight + MBRandom.RandomFloatRanged(-0.1f, 0f), hero.Build + MBRandom.RandomFloatRanged(-0.1f, 0)), hero.Mother.BodyProperties.StaticProperties);
                    BodyProperties bodyPropertiesMax = new BodyProperties(new DynamicBodyProperties(hero.Age, hero.Weight + MBRandom.RandomFloatRanged(0f, 0.1f), hero.Build + MBRandom.RandomFloatRanged(0, 0.1f)), hero.Father.BodyProperties.StaticProperties);
                    BodyProperties randomBodyProperties = BodyProperties.GetRandomBodyProperties(hero.CharacterObject.Race, hero.IsFemale, bodyPropertiesMin, bodyPropertiesMax, 1, MBRandom.RandomInt(), hero.HairTags, hero.BeardTags, hero.TattooTags);
                    hero.ModifyPlayersFamilyAppearance(randomBodyProperties.StaticProperties);
                    hero.Weight = randomBodyProperties.Weight;
                    hero.Build = randomBodyProperties.Build;
                    hero.ModifyHair(isFemale ? s_femaleHairStyles.GetRandomElement() : s_maleHairStyles.GetRandomElement(), 0, 0);
                }
                return hero;
            }
         }

        private static List<FamilyMemberData> family;

        public static List<FamilyMemberData> Family {
            get {
                if (family == null)
                    GenerateFamilyData();
                return family;
            }
        }

        private static void GenerateFamilyData()
        {
            family = new List<FamilyMemberData>();
            bool isDefault = true;
            if (isDefault)
            {
                if (Hero.MainHero.Age < 35)
                {
                    family.Add(new FamilyMemberData(true, 2.1f, FamilyMemberData.RelationToMain.Sibling));
                    family.Add(new FamilyMemberData(true, -1.5f, FamilyMemberData.RelationToMain.Sibling));
                }
                else
                {
                    family.Add(new FamilyMemberData(!Hero.MainHero.IsFemale, 2f, FamilyMemberData.RelationToMain.Spouse));
                    family.Add(new FamilyMemberData(true, -18.5f, FamilyMemberData.RelationToMain.Child));
                    family.Add(new FamilyMemberData(true, -20f, FamilyMemberData.RelationToMain.Child));
                    family.Add(new FamilyMemberData(true, -21.9f, FamilyMemberData.RelationToMain.Child));
                }
            }
        }
    }
}