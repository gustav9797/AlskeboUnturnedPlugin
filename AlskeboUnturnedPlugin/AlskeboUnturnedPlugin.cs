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
        class PlayerData
        {
            public bool isDriving = false;
        }

        Dictionary<string, InteractableVehicle> playerVehicles;
        Dictionary<ushort, UnturnedPlayer> vehiclePlayers;
        Dictionary<string, PlayerData> playerDataMap;

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
            PlayerData playerData = playerDataMap[player.Id];
            if (playerData == null)
            {
                playerData = new PlayerData();
                playerDataMap.Add(player.Id, playerData);
            }

           // player.id
            Logger.LogWarning("\ttest");
            if (status == SDG.Unturned.EPlayerStat.TRAVEL_VEHICLE && !playerData.isDriving)
            {
                if (getClaimedVehicle(player) != player.CurrentVehicle)
                {
                    UnturnedChat.Say(player, "You have claimed a new vehicle.");
                }
                else
                    UnturnedChat.Say(player, "You have entered your claimed vehicle.");
            }
            else if (status != SDG.Unturned.EPlayerStat.TRAVEL_VEHICLE && playerData.isDriving)
            {
                UnturnedChat.Say(player, "You have exited your claimed vehicle.");
            }
            else
                UnturnedChat.Say(player, "Something happened! " + status.ToString());
        }

        private void claimVehicle(UnturnedPlayer player, InteractableVehicle vehicle)
        {
            if (vehiclePlayers.ContainsKey(vehicle.id))
            {
                unclaimVehicle(vehiclePlayers[vehicle.id]);
            }
            if (getClaimedVehicle(player) != vehicle)
            {
                unclaimVehicle(player);
                playerVehicles.Add(player.Id, vehicle);
                vehiclePlayers.Add(vehicle.id, player);
            }
        }
        private void unclaimVehicle(UnturnedPlayer player)
        {
            InteractableVehicle vehicle = getClaimedVehicle(player);
            if (!vehicle)
                return;
            playerVehicles.Remove(player.Id);
            vehiclePlayers.Remove(vehicle.id);
            
            if (vehicle.fuel <= 0)
                UnturnedChat.Say(player, "TODO: Destroy old vehicle.");

        }
        private InteractableVehicle getClaimedVehicle(UnturnedPlayer player)
        {
            if (playerVehicles.ContainsKey(player.Id))
                return playerVehicles[player.Id];
            return null;
        }

    }
}
