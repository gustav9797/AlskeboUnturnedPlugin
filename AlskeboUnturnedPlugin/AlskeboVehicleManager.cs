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

        Dictionary<CSteamID, List<VehicleInfo>> playerOwnedVehicles = new Dictionary<CSteamID, List<VehicleInfo>>();
        Dictionary<uint, CSteamID> vehicleOwners = new Dictionary<uint, CSteamID>();
        Dictionary<uint, DestroyingVehicleInfo> vehiclesToBeDestroyed = new Dictionary<uint, DestroyingVehicleInfo>();
        List<ushort> vehicleDestroySounds = new List<ushort> { 19, 20, 35, 36, 37, 53, 61, 62, 63, 65, 69, 71, 72, 81, 89 };
        System.Random r = new System.Random();

        public AlskeboVehicleManager() {
            Timer timer = new Timer(200);
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
                    DestroyingVehicleInfo info = new DestroyingVehicleInfo();
                    info.vehicle = vehicle;
                    info.lastActivity = DateTime.Now;
                    vehiclesToBeDestroyed.Add(vehicle.instanceID, info);
                }
            }
        }

        private void tick(object sender, ElapsedEventArgs e) {
            for (int i = 0; i < vehiclesToBeDestroyed.Count; ++i) {
                KeyValuePair<uint, DestroyingVehicleInfo> pair = vehiclesToBeDestroyed.ElementAt(i);
                if (!pair.Value.vehicle.isDead && (DateTime.Now - pair.Value.lastActivity).Minutes >= 10) {
                    // fun stuff here!
                    InteractableVehicle vehicle = pair.Value.vehicle;
                    if (pair.Value.timesHonked >= 50) {
                        pair.Value.vehicle.askDamage(ushort.MaxValue, false);
                    } else if (!pair.Value.lastHonked) {
                        VehicleManager.sendVehicleHeadlights(vehicle);
                        EffectManager.sendEffect(vehicleDestroySounds[r.Next(vehicleDestroySounds.Count - 1)], 200, vehicle.transform.position);
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
                playerOwnedVehicles.Add(player.CSteamID, new List<VehicleInfo>());

            List<VehicleInfo> list = playerOwnedVehicles[player.CSteamID];
            VehicleInfo info = new VehicleInfo();
            info.instanceId = vehicle.instanceID;
            info.ownerId = player.CSteamID;
            info.ownerName = player.DisplayName;
            list.Add(info);
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

        public List<VehicleInfo> getOwnedVehicles(CSteamID id) {
            if (playerOwnedVehicles.ContainsKey(id))
                return playerOwnedVehicles[id];
            return new List<VehicleInfo>();
        }

        public VehicleInfo getOwnedVehicleInfoByInstanceId(CSteamID id, uint instanceId) {
            if (playerOwnedVehicles.ContainsKey(id)) {
                List<VehicleInfo> vehicleInfos = playerOwnedVehicles[id];
                foreach (VehicleInfo info in vehicleInfos) {
                    if (info.instanceId == instanceId) {
                        return info;
                    }
                }
            }
            return null;
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
