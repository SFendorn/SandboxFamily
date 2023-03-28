# Sandbox Family
Configurable starting family mod in sandbox mode for Mount &amp; Blade II: Bannerlord

## Content
Ever dreamt of a family in sandbox mode? With this mod you can customize your starting family to your liking.

The default family consists of two sisters if the player character is 20 or 30 years old, or three daughters and a spouse if the player character is 40 or 50 years old. The family members are added during game startup and are placed in the town of birth of the player character, i.e. the town in front of which you start the game. They start at level 0 but get attribute and focus points based on their age, which is equivalent to the player character creation.

**Important**: If you want the new family members to have starting skill points in the same way as the main character gets them during character creation (10 skill points per focus point spent), you need to distribute the focus points of your family members before they level up from 0 to 1. At this point, the mod checks if the custom family members have any focus points distributed and assigns skill points accordingly. This is a unique event which is not repeatable. If you miss it, they keep their 0 skill points in all skills.

## Customize your Family
You can customize your family by placing a `config.txt` file into the mod folder next to `SubModule.xml`. Each line in `config.txt` represents a family member. There are no real requirements for syntax, but the following keywords need to be present: *spouse*, *sister*, *brother*, *daughter*, or *son*. Additionally, the line must contain a positive or negative floating point number, e.g., `0`, `1.9`, `-0.9`. This number represents the age difference between the player character and the family member. The following limitations must be met in order to create the family in game:
* If you have children, you must have a spouse,
* All family members must be at least 18 years old,
* The age difference between parents and their children must be at least 18 years,
* The age difference between siblings must be exactly 0 or at least 0.9,
* The age difference between children must be exactly 0 or at least 0.9.

Note: These restrictions aim at creating family relations which would also be possible in the base game. The restriction for family members to be 18 years or older is based on the coming of age system, which I do not want to mess around with. If the creation fails, the reason is stated in `log.txt` which will be placed next to `config.txt`.

## Compatibility
This mod mainly uses C# code and adds two XML character templates that are not necessary beyond the initial creation. Thus, it *may* be possible to remove the mod mid game, but I still would advise against it. It does not do anything beyond the initial creation and level up. It makes no sense to add the mod mid game, because the family members are added during the initial character creation.
