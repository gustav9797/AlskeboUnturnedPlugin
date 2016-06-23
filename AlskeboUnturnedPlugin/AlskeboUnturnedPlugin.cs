using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlskeboUnturnedPlugin {
    public class AlskeboUnturnedPlugin : RocketPlugin {
        class PlayerData {
            public bool isDriving = false;
        }

        Dictionary<string, PlayerData> playerDataMap = new Dictionary<string, PlayerData>();
        public static AlskeboVehicleManager vehicleManager = new AlskeboVehicleManager();

        public override void LoadPlugin() {
            base.LoadPlugin();
            U.Events.OnPlayerConnected += onPlayerConnected;
            U.Events.OnPlayerDisconnected += onPlayerDisconnected;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance += onPlayerUpdateStance;
            Logger.LogWarning("\tAlskeboPlugin Loaded Sucessfully");
        }

        private void onPlayerConnected(UnturnedPlayer player) {
            UnturnedChat.Say(player.DisplayName + " has connected.");
        }

        private void onPlayerDisconnected(UnturnedPlayer player) {
            UnturnedChat.Say(player.DisplayName + " has disconnected.");
        }

        private void onPlayerUpdateStance(UnturnedPlayer player, byte stance) {
            if (!playerDataMap.ContainsKey(player.Id) || playerDataMap[player.Id] == null)
                playerDataMap.Add(player.Id, new PlayerData());

            PlayerData playerData = playerDataMap[player.Id];

            if (stance == 6 && !playerData.isDriving) {
                // Player entered vehicle

                playerData.isDriving = true;
                playerDataMap[player.Id] = playerData;

                if (player.CurrentVehicle != null) {
                    CSteamID who = vehicleManager.getVehicleOwner(player.CurrentVehicle);
                    String vehicleName = vehicleManager.getVehicleTypeName(player.CurrentVehicle.id);
                    if (who != CSteamID.Nil) {
                        String whoNickname = SteamFriends.GetPlayerNickname(who);
                        if (whoNickname == null)
                            whoNickname = "Unknown player";

                        if (player.CSteamID.Equals(who))
                            UnturnedChat.Say(player, "Welcome back to your " + vehicleName + ", " + player.DisplayName + "!");
                        else
                            UnturnedChat.Say(player, "This " + vehicleName + " belongs to " + whoNickname + ".");
                    } else
                        UnturnedChat.Say(player, "This natural " + vehicleName + " will despawn when its fuel level is low.");
                }
            } else if (stance != 6 && playerData.isDriving) {
                // Player exited vehicle

                playerData.isDriving = false;
                playerDataMap[player.Id] = playerData;
            }
        }

    }
}
