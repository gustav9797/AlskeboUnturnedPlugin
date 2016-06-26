using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class AlskeboVehicleManager {

        Dictionary<CSteamID, List<VehicleInfo>> playerOwnedVehicles = new Dictionary<CSteamID, List<VehicleInfo>>();
        Dictionary<uint, VehicleInfo> vehicleOwners = new Dictionary<uint, VehicleInfo>();
        Dictionary<uint, DestroyingVehicleInfo> vehiclesToBeDestroyed = new Dictionary<uint, DestroyingVehicleInfo>();
        Dictionary<uint, DatabaseVehicle> lastSave = new Dictionary<uint, DatabaseVehicle>();

        List<ushort> vehicleDestroySounds = new List<ushort> { 19, 20, 35, 36, 37, 53, 61, 62, 63, 65, 69, 71, 72, 81, 89 };
        System.Random r = new System.Random();

        public AlskeboVehicleManager() {
            System.Timers.Timer timer = new System.Timers.Timer(200);
            timer.Elapsed += tick;
            timer.Start();

            System.Timers.Timer saveTimer = new System.Timers.Timer(1000 * 60); //Every minute
            saveTimer.Elapsed += saveVehicles;
            saveTimer.Start();

            Provider.onServerShutdown += onServerShutdown;
            foreach (Delegate d in Provider.onServerShutdown.GetInvocationList()) {
                if (!d.Method.DeclaringType.ToString().Contains("Alskebo")) {
                    Provider.onServerShutdown -= (Provider.ServerShutdown)d;
                    Provider.onServerShutdown += (Provider.ServerShutdown)d;
                }
            }
            Provider.onServerShutdown += onLateServerShutdown;
        }

        public void onPluginLoaded() {
            Level.onLevelLoaded += onLevelLoaded;
        }

        public void onLevelLoaded(int level) {
            Level.onLevelLoaded -= onLevelLoaded;

            Logger.Log("Removing all loaded vehicles (" + VehicleManager.Vehicles.Count + ")...");
            CustomVehicleManager.customseq = 0U;
            VehicleManager.Vehicles = new List<InteractableVehicle>();
            CustomVehicleManager.custominstanceCount = 0U;
            CustomVehicleManager.customrespawnVehicleIndex = (ushort)0;
            BarricadeManager.clearPlants();

            Logger.Log("Receiving owned vehicles from database...");
            List<DatabaseVehicle> vehicles = AlskeboUnturnedPlugin.databaseManager.receiveOwnedVehicles();
            foreach (DatabaseVehicle dbv in vehicles) {
                /*Logger.Log("type " + dbv.type);
                Logger.Log("x " + dbv.x);
                Logger.Log("y " + dbv.y);
                Logger.Log("z " + dbv.z);
                Logger.Log("rx " + dbv.rx);
                Logger.Log("ry " + dbv.ry);
                Logger.Log("rz " + dbv.rz);
                Logger.Log("rw " + dbv.rw);
                Logger.Log("fuel " + dbv.fuel);
                Logger.Log("health " + dbv.health);*/

                InteractableVehicle vehicle = CustomVehicleManager.customSpawnVehicle(dbv.type, new Vector3(dbv.x, dbv.y, dbv.z), new Quaternion(dbv.rx, dbv.ry, dbv.rz, dbv.rw));

                vehicle.tellFuel(dbv.fuel);
                VehicleManager.sendVehicleFuel(vehicle, dbv.fuel);

                vehicle.tellHealth(dbv.health);
                VehicleManager.sendVehicleHealth(vehicle, dbv.health);

                CSteamID ownerId = new CSteamID(dbv.steamId);
                if (!playerOwnedVehicles.ContainsKey(ownerId))
                    playerOwnedVehicles.Add(ownerId, new List<VehicleInfo>());

                List<VehicleInfo> list = playerOwnedVehicles[ownerId];
                VehicleInfo info = new VehicleInfo();
                info.instanceId = vehicle.instanceID;
                info.databaseId = dbv.id;
                info.ownerId = ownerId;
                info.ownerName = "TEMPORARY NAME";
                list.Add(info);
                playerOwnedVehicles[ownerId] = list;
                vehicleOwners.Add(vehicle.instanceID, info);
            }
            Logger.Log("Done.");
        }

        public void onPluginUnloaded() {
        }

        public void onServerShutdown() {
            saveVehicles(null, null);
            Logger.Log("Removing owned vehicles to prevent duplicates on next server start (" + vehicleOwners.Count + "/" + VehicleManager.Vehicles.Count + ")...");
            foreach (uint instanceId in vehicleOwners.Keys) {
                // Removes vehicle completely and destroys stuff built on it
                VehicleManager.Instance.tellVehicleDestroy(Provider.server, instanceId);
            }
        }

        public void onLateServerShutdown() {
            String fileName = "Level/" + Level.info.name + "/Vehicles.dat";
            if (File.Exists(fileName)) {
                File.Delete(fileName);
                Logger.Log("Deleted Vehicles.dat.");
            } else
                Logger.Log("Could not delete Vehicles.dat.");
        }

        public void onPlayerConnected(UnturnedPlayer player) {

        }

        public void onPlayerDisconnected(UnturnedPlayer player) {

        }

        public void onPlayerEnterVehicle(UnturnedPlayer player, InteractableVehicle vehicle) {
            if (vehiclesToBeDestroyed.ContainsKey(vehicle.instanceID))
                vehiclesToBeDestroyed.Remove(vehicle.instanceID);
        }

        public void onPlayerExitVehicle(UnturnedPlayer player, InteractableVehicle vehicle) {
            if (vehicle != null) {
                VehicleInfo info = getOwnedVehicleInfo(vehicle.instanceID);
                if (info == null || info.ownerId == null || info.ownerId == CSteamID.Nil) {
                    if (vehicle.fuel <= 5 && !vehiclesToBeDestroyed.ContainsKey(vehicle.instanceID)) {
                        DestroyingVehicleInfo destroyingInfo = new DestroyingVehicleInfo();
                        destroyingInfo.vehicle = vehicle;
                        destroyingInfo.lastActivity = DateTime.Now;
                        vehiclesToBeDestroyed.Add(vehicle.instanceID, destroyingInfo);
                    }
                } else if (!lastSave.ContainsKey(vehicle.instanceID) || !isSimilar(vehicle, lastSave[vehicle.instanceID])) {
                    DatabaseVehicle dbv = DatabaseVehicle.fromInteractableVehicle(info.databaseId, info.ownerId.m_SteamID, vehicle);
                    AlskeboUnturnedPlugin.databaseManager.updateVehicle(dbv);
                    if (lastSave.ContainsKey(vehicle.instanceID))
                        lastSave[vehicle.instanceID] = dbv;
                    else
                        lastSave.Add(vehicle.instanceID, dbv);
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

        private void saveVehicles(object sender, ElapsedEventArgs e) {
            foreach (KeyValuePair<uint, VehicleInfo> pair in vehicleOwners) {
                InteractableVehicle vehicle = VehicleManager.getVehicle(pair.Key);
                if (vehicle.isDead || vehicle.isDrowned) {
                    deleteOwnedVehicle(pair.Value.databaseId, pair.Key);
                }

                if (!lastSave.ContainsKey(pair.Key) || !isSimilar(vehicle, lastSave[pair.Key])) {
                    DatabaseVehicle dbv = DatabaseVehicle.fromInteractableVehicle(pair.Value.databaseId, pair.Value.ownerId.m_SteamID, vehicle);
                    AlskeboUnturnedPlugin.databaseManager.updateVehicle(dbv);
                    if (lastSave.ContainsKey(vehicle.instanceID))
                        lastSave[vehicle.instanceID] = dbv;
                    else
                        lastSave.Add(vehicle.instanceID, dbv);
                }
            }

            /*foreach (InteractableVehicle vehicle in VehicleManager.Vehicles) {
                if(!vehicleOwners.ContainsKey(vehicle.instanceID)) {
                    // Vehicle was spawned with /v or unturned respawned it
                }
            }*/
        }

        private bool isSimilar(InteractableVehicle first, DatabaseVehicle second) {
            if (first.transform.position.x == second.x && first.transform.position.y == second.y && first.transform.position.z == second.z
                && first.transform.rotation.x == second.rx && first.transform.rotation.y == second.ry && first.transform.rotation.z == second.rz && first.transform.rotation.x == second.rw) {
                if (first.fuel == second.fuel && first.health == second.health)
                    return true;
            }
            return false;
        }

        public InteractableVehicle givePlayerOwnedVehicle(UnturnedPlayer player, ushort carId) {
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
            info.databaseId = AlskeboUnturnedPlugin.databaseManager.insertOwnedVehicle(player.CSteamID, vehicle);
            info.ownerId = player.CSteamID;
            info.ownerName = player.DisplayName;
            list.Add(info);
            playerOwnedVehicles[player.CSteamID] = list;
            vehicleOwners.Add(vehicle.instanceID, info);
            return vehicle;
        }

        public CSteamID getVehicleOwner(InteractableVehicle vehicle) {
            if (vehicleOwners.ContainsKey(vehicle.instanceID))
                return vehicleOwners[vehicle.instanceID].ownerId;
            return CSteamID.Nil;
        }

        public List<VehicleInfo> getOwnedVehicles(CSteamID id) {
            if (playerOwnedVehicles.ContainsKey(id))
                return playerOwnedVehicles[id];
            return new List<VehicleInfo>();
        }

        public VehicleInfo getOwnedVehicleInfo(uint instanceId) {
            if (vehicleOwners.ContainsKey(instanceId))
                return vehicleOwners[instanceId];
            return null;
        }

        public void deleteOwnedVehicle(long databaseId, uint instanceId) {
            AlskeboUnturnedPlugin.databaseManager.deleteVehicle(databaseId);

            if (vehicleOwners.ContainsKey(instanceId)) {
                VehicleInfo info = vehicleOwners[instanceId];
                if (playerOwnedVehicles.ContainsKey(info.ownerId)) {
                    List<VehicleInfo> infos = playerOwnedVehicles[info.ownerId];
                    if (!infos.Remove(info))
                        Logger.LogWarning("Could not remove from playerOwnedVehicles");
                    else
                        playerOwnedVehicles[info.ownerId] = infos;
                }
                if (!vehicleOwners.Remove(instanceId)) {
                    Logger.LogWarning("Could not remove from vehicleOwners");
                }
            }

            if (vehiclesToBeDestroyed.ContainsKey(instanceId))
                vehiclesToBeDestroyed.Remove(instanceId);

            if (lastSave.ContainsKey(instanceId))
                lastSave.Remove(instanceId);
        }

        public String getVehicleTypeName(ushort id) {
            Asset a = SDG.Unturned.Assets.find(EAssetType.VEHICLE, id);
            if (a != null)
                return ((VehicleAsset)a).Name;
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
