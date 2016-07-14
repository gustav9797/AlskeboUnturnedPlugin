using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class VehiclesCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "Vehicles"; }
        }

        public string Help {
            get { return "View others' vehicles."; }
        }

        public string Syntax {
            get { return "<player>"; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (command.Length >= 1) {
                UnturnedPlayer target = UnturnedPlayer.FromName(command[0]);
                if (target != null) {
                    UnturnedChat.Say(player, target.DisplayName + "'s vehicles:");
                    List<VehicleInfo> vehicles = AlskeboUnturnedPlugin.vehicleManager.getOwnedVehicles(target.CSteamID);
                    foreach (VehicleInfo info in vehicles) {
                        InteractableVehicle vehicle = VehicleManager.getVehicle(info.instanceId);
                        if (vehicle != null) {
                            String pos = Math.Round(vehicle.transform.position.x) + "|" + Math.Round(vehicle.transform.position.y) + "|" + Math.Round(vehicle.transform.position.z);
                            UnturnedChat.Say(player, "#" + vehicle.instanceID + " " + vehicle.asset.Name + " - " + vehicle.health + " HP - " + (info.isLocked ? "Locked" : "Unlocked") + "\n Last seen near " + AlskeboUnturnedPlugin.getClosestLocation(vehicle.transform.position).Name);
                        }
                    }
                    if (vehicles.Count <= 0)
                        UnturnedChat.Say(player, target.DisplayName + " doesn't own any vehicles.");
                } else
                    UnturnedChat.Say(player, "Could not find the specified player.");
            } else
                UnturnedChat.Say(player, "Usage: /vehicles " + Syntax);
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.vehicles");
                return list;
            }
        }
    }
}
