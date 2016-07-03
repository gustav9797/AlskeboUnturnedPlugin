using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class MyVehiclesCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "MyVehicles"; }
        }

        public string Help {
            get { return "View your owned vehicles."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            UnturnedChat.Say(player, "Your vehicles:");
            List<VehicleInfo> vehicles = AlskeboUnturnedPlugin.vehicleManager.getOwnedVehicles(player.CSteamID);
            foreach (VehicleInfo info in vehicles) {
                InteractableVehicle vehicle = VehicleManager.getVehicle(info.instanceId);
                if (vehicle != null) {
                    String pos = Math.Round(vehicle.transform.position.x) + "|" + Math.Round(vehicle.transform.position.y) + "|" + Math.Round(vehicle.transform.position.z);
                    UnturnedChat.Say(player, vehicle.asset.Name + " - HP:" + vehicle.health + " Locked: " + (info.isLocked ? "Yes" : "No") + " Pos:" + pos + " ID:" + vehicle.instanceID);
                }
            }
            if (vehicles.Count <= 0)
                UnturnedChat.Say(player, "You don't own any vehicles.");
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.myvehicles");
                return list;
            }
        }
    }
}
