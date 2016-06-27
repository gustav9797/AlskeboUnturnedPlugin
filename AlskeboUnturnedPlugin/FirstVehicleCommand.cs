using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class FirstVehicleCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
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
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            if (caller is UnturnedPlayer) {
                UnturnedPlayer player = (UnturnedPlayer)caller;

                if (!AlskeboUnturnedPlugin.databaseManager.playerHasReceivedVehicle(player.CSteamID)) {
                    AlskeboUnturnedPlugin.vehicleManager.givePlayerOwnedVehicle(player, (ushort)AlskeboUnturnedPlugin.r.Next(69, 75));
                    AlskeboUnturnedPlugin.databaseManager.setPlayerReceivedVehicle(player.CSteamID, true);
                    UnturnedChat.Say(caller, "Enjoy your noob-car!");
                } else {
                    UnturnedChat.Say(caller, "You have already received your noob-car.");
                }
            } else {
                UnturnedChat.Say(caller, "You must be in-game to execute this command.");
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("alskebo.firstvehicle");
                return list;
            }
        }
    }
}
