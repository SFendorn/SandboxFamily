using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandboxFamily
{
    public class SandboxFamily : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign))
                return;
            AddBehaviors((CampaignGameStarter) gameStarterObject);
        }

        private void AddBehaviors(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddBehavior(new SandboxFamilyBehavior());
        }
    }
}
