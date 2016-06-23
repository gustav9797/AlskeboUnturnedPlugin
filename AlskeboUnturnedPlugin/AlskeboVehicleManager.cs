using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class AlskeboVehicleManager {
        Dictionary<CSteamID, List<uint>> playerOwnedVehicles = new Dictionary<CSteamID, List<uint>>();
        Dictionary<uint, CSteamID> vehicleOwners = new Dictionary<uint, CSteamID>();
        Dictionary<uint, VehicleInfo> vehiclesToBeDestroyed = new Dictionary<uint, VehicleInfo>();

        public AlskeboVehicleManager() {
            Timer timer = new Timer(700);
            timer.Elapsed += tick;
            timer.Start();
        }

        public void onPlayerEnterVehicle(UnturnedPlayer player, InteractableVehicle vehicle) {
            if (vehiclesToBeDestroyed.ContainsKey(vehicle.instanceID))
                vehiclesToBeDestroyed.Remove(vehicle.instanceID);
        }

        public void onPlayerExitVehicle(UnturnedPlayer player, InteractableVehicle vehicle) {
            if (vehicle != null && getVehicleOwner(vehicle) == CSteamID.Nil) {
                if (vehicle.fuel <= 5 && !vehiclesToBeDestroyed.ContainsKey(vehicle.instanceID)) {
                    VehicleInfo info = new VehicleInfo();
                    info.vehicle = vehicle;
                    info.lastActivity = DateTime.Now;
                    vehiclesToBeDestroyed.Add(vehicle.instanceID, info);
                }
            }
        }

        private void tick(object sender, ElapsedEventArgs e) {
            for (int i = 0; i < vehiclesToBeDestroyed.Count; ++i) {
                KeyValuePair<uint, VehicleInfo> pair = vehiclesToBeDestroyed.ElementAt(i);
                if (!pair.Value.vehicle.isDead && (DateTime.Now - pair.Value.lastActivity).Minutes >= 0) {
                    // fun stuff here!
                    InteractableVehicle vehicle = pair.Value.vehicle;
                    if (pair.Value.timesHonked >= 5) {
                        pair.Value.vehicle.askDamage(ushort.MaxValue, false);
                    } else if (!pair.Value.lastHonked) {
                        VehicleManager.sendVehicleHorn(vehicle);
                        VehicleManager.sendVehicleHeadlights(vehicle);
                        pair.Value.lastHonked = true;
                        ++pair.Value.timesHonked;
                    } else {
                        pair.Value.lastHonked = false;
                    }

                } else if (pair.Value.vehicle.isDead) {
                    vehiclesToBeDestroyed.Remove(pair.Value.vehicle.instanceID);
                    break;
                }
            }
        }

        public InteractableVehicle givePlayerOwnedCar(UnturnedPlayer player, ushort carId) {
            InteractableVehicle vehicle = giveVehicle(player, carId);
            if (vehicle == null) {
                UnturnedChat.Say(player, "Could not give you a vehicle.");
                return null;
            }

            if (!playerOwnedVehicles.ContainsKey(player.CSteamID))
                playerOwnedVehicles.Add(player.CSteamID, new List<uint>());

            List<uint> list = playerOwnedVehicles[player.CSteamID];
            list.Add(vehicle.instanceID);
            playerOwnedVehicles[player.CSteamID] = list;
            vehicleOwners.Add(vehicle.instanceID, player.CSteamID);
            // TODO: store in database
            return vehicle;
        }

        public CSteamID getVehicleOwner(InteractableVehicle vehicle) {
            if (vehicleOwners.ContainsKey(vehicle.instanceID))
                return vehicleOwners[vehicle.instanceID];
            return CSteamID.Nil;
        }

        public String getVehicleTypeName(ushort id) {
            Asset a = SDG.Unturned.Assets.find(EAssetType.VEHICLE, id);
            if (a != null)
                return a.name;
            return "Unknown vehicle";
        }

        private static InteractableVehicle giveVehicle(UnturnedPlayer player, ushort id) {
            Vector3 point = player.Player.transform.position + player.Player.transform.forward * 6f;
            RaycastHit hitInfo;
            Physics.Raycast(point + Vector3.up * 16f, Vector3.down, out hitInfo, 32f, RayMasks.BLOCK_VEHICLE);
            if ((UnityEngine.Object)hitInfo.collider != (UnityEngine.Object)null)
                point.y = hitInfo.point.y + 16f;
            InteractableVehicle vehicle = CustomVehicleManager.customSpawnVehicle(id, point, player.Player.transform.rotation);
            return vehicle;
        }


    }
}
