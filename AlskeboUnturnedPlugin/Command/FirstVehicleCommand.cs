using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class FirstVehicleCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "FirstVehicle"; }
        }

        public string Help {
            get { return "Retrieve your first vehicle."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get {
                List<string> a = new List<string>();
                a.Add("FirstCar");
                return a;
            }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (!AlskeboUnturnedPlugin.playerManager.getPlayerBoolean(player, "receivedvehicle")) {
                AlskeboUnturnedPlugin.vehicleManager.givePlayerOwnedVehicle(player, (ushort)AlskeboUnturnedPlugin.r.Next(69, 75));
                AlskeboUnturnedPlugin.databaseManager.setPlayerReceivedVehicle(player.CSteamID, true);
                AlskeboUnturnedPlugin.playerManager.setPlayerData(player, "receivedvehicle", true);
                AlskeboUnturnedPlugin.databaseManager.logPlayerAsync(player.CSteamID, PlayerLogType.FIRSTVEHICLE);
                UnturnedChat.Say(caller, "Enjoy your noob-car!");
            } else {
                UnturnedChat.Say(caller, "You have already received your noob-car.");
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.firstvehicle");
                return list;
            }
        }
    }
}
