using Rocket.API;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class LockCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "Lock"; }
        }

        public string Help {
            get { return "Lock and unlock your closest vehicle."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get {
                List<string> a = new List<string>();
                a.Add("Unlock");
                return a;
            }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            List<VehicleInfo> vehicles = AlskeboUnturnedPlugin.vehicleManager.getAllVehicles(player.CSteamID, player.SteamGroupID);
            if (vehicles.Count <= 0) {
                UnturnedChat.Say(player, "You don't own any vehicles.");
                return;
            }

            InteractableVehicle closest = null;
            VehicleInfo closestInfo = null;
            float closestDist = float.MaxValue;
            foreach (VehicleInfo info in vehicles) {
                InteractableVehicle vehicle = VehicleManager.getVehicle(info.instanceId);
                float dist = Vector3.Distance(player.Position, vehicle.transform.position);
                if (vehicle != null && dist < 30 && dist < closestDist) {
                    closest = vehicle;
                    closestInfo = info;
                    closestDist = dist;
                }
            }

            if (closest == null) {
                UnturnedChat.Say(player, "Couldn't find any close vehicles.");
                return;
            }

            AlskeboUnturnedPlugin.vehicleManager.setOwnedVehicleLocked(closest, !closestInfo.isLocked);
            CustomVehicleManager.sendVehicleHeadlights(closest);

            new Thread(delegate () {
                Thread.Sleep(400);
                TaskDispatcher.QueueOnMainThread(new System.Action(delegate () {
                    if (closest != null)
                        CustomVehicleManager.sendVehicleHeadlights(closest);
                }));
            }).Start();
            UnturnedChat.Say(player, "Your " + closest.asset.Name + " is now " + (closest.isLocked ? "locked" : "unlocked") + ".", AlskeboVehicleManager.vehicleManagerPrefix);
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.lock");
                return list;
            }
        }
    }
}
