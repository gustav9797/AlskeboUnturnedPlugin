using Rocket.API;
using Rocket.Core.Assets;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class EnterVehicleCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
        }

        public string Name {
            get { return "EnterVehicle"; }
        }

        public string Help {
            get { return ""; }
        }

        public string Syntax {
            get { return "<id>"; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            if (caller is UnturnedPlayer) {
                UnturnedPlayer player = (UnturnedPlayer)caller;

                uint id = 0;
                if (command.Length >= 1 && uint.TryParse(command[0], out id)) {
                    InteractableVehicle vehicle = VehicleManager.getVehicle(id);
                    if (vehicle != null)
                        VehicleManager.Instance.askEnterVehicle(player.CSteamID, id, vehicle.asset.hash, (byte)vehicle.asset.engine);
                    else
                        UnturnedChat.Say(caller, "Vehicle null.");
                } else {
                    UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                    throw new WrongUsageOfCommandException(caller, this);
                }
            } else
                UnturnedChat.Say(caller, "You must be in-game to execute this command.");
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("alskebo.entervehicle");
                return list;
            }
        }
    }
}
