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
            get { return AllowedCaller.Player; }
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
            UnturnedPlayer player = (UnturnedPlayer)caller;

            uint id = 0;
            if (command.Length >= 1 && uint.TryParse(command[0], out id)) {
                InteractableVehicle vehicle = VehicleManager.getVehicle(id);
                if (vehicle != null) {
                    player.Teleport(vehicle.transform.position, 0);
                    VehicleManager.Instance.askEnterVehicle(player.CSteamID, id, vehicle.asset.hash, (byte)vehicle.asset.engine);
                } else
                    UnturnedChat.Say(caller, "Vehicle null.");
            } else {
                UnturnedChat.Say(caller, "Usage: /entervehicle " + Syntax);
                throw new WrongUsageOfCommandException(caller, this);
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.entervehicle");
                return list;
            }
        }
    }
}
