using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlskeboUnturnedPlugin
{
    public class AlskeboUnturnedPlugin : RocketPlugin
    {
        public override void LoadPlugin()
        {
            base.LoadPlugin();
            Logger.LogWarning("\tAlskeboPlugin Loaded Sucessfully");
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStat += onPlayerUpdateStat;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture += UnturnedPlayerEvents_OnPlayerUpdateGesture;
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateGesture(UnturnedPlayer player, Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture gesture)
        {
            UnturnedChat.Say(player, "You gestured");
        }

        private void onPlayerUpdateStat(UnturnedPlayer player, EPlayerStat status)
        {
            Logger.LogWarning("\ttest");
            if (status == SDG.Unturned.EPlayerStat.TRAVEL_VEHICLE)
            {
                UnturnedChat.Say(player, "You probably entered a vehicle?");
            }
            else
                UnturnedChat.Say(player, "Something happened! " + status.ToString());
        }

    }
}
