using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

            // Make sure to get server shutdown before vehicles are saved to file
            Provider.onServerShutdown += onServerShutdown;
            foreach (Delegate d in Provider.onServerShutdown.GetInvocationList()) {
                if (!d.Method.DeclaringType.ToString().Contains("Alskebo")) {
                    Provider.onServerShutdown -= (Provider.ServerShutdown)d;
                    Provider.onServerShutdown += (Provider.ServerShutdown)d;
                }
            }
            Provider.onServerShutdown += onLateServerShutdown;

            // onPrePreLevelLoaded run before vehicles are loaded
            Level.onPrePreLevelLoaded += onPrePreLevelLoaded;
            foreach (Delegate d in Level.onPrePreLevelLoaded.GetInvocationList()) {
                if (!d.Method.DeclaringType.ToString().Contains("Alskebo")) {
                    Level.onPrePreLevelLoaded -= (PrePreLevelLoaded)d;
                    Level.onPrePreLevelLoaded += (PrePreLevelLoaded)d;
                }
            }
            Level.onLevelLoaded += onLateLevelLoaded;
        }

        public void onPluginLoaded() {

        }

        public void onPrePreLevelLoaded(int level) {
            Level.onPrePreLevelLoaded -= onPrePreLevelLoaded;
            String fileName = ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Level/" + Level.info.name + "/Vehicles.dat";
            if (File.Exists(fileName)) {
                File.Delete(fileName);
                Logger.Log("Deleted Vehicles.dat.");
            } else
                Logger.Log("Could not find Vehicles.dat.");
        }

        public void onLateLevelLoaded(int level) {
            Level.onLevelLoaded -= onLateLevelLoaded;

            Logger.Log("Receiving owned vehicles from database...");
            List<DatabaseVehicle> vehicles = AlskeboUnturnedPlugin.databaseManager.receiveOwnedVehicles();
            int naturalCount = 0;
            foreach (DatabaseVehicle v in vehicles) {
                if (v.steamId == 0)
                    naturalCount++;
            }

            int defaultVehicleCount = 24;
            if (naturalCount >= defaultVehicleCount) {
                Logger.Log("Removing all default vehicles (" + VehicleManager.Vehicles.Count + ")...");
                CustomVehicleManager.customseq = 0U;
                VehicleManager.Vehicles = new List<InteractableVehicle>();
                CustomVehicleManager.custominstanceCount = 0U;
                CustomVehicleManager.customrespawnVehicleIndex = (ushort)0;
                BarricadeManager.clearPlants();
            } else {
                Logger.Log("There are (" + naturalCount + "/" + defaultVehicleCount + ") natural vehicles, not removing default vehicles.");
            }

            foreach (DatabaseVehicle dbv in vehicles) {
                InteractableVehicle vehicle = CustomVehicleManager.customSpawnVehicle(dbv.type, new Vector3(dbv.x, dbv.y, dbv.z), new Quaternion(dbv.rx, dbv.ry, dbv.rz, dbv.rw));

                vehicle.tellFuel(dbv.fuel);
                VehicleManager.sendVehicleFuel(vehicle, dbv.fuel);

                vehicle.tellHealth(dbv.health);
                VehicleManager.sendVehicleHealth(vehicle, dbv.health);

                storeOwnedVehicle(new CSteamID(dbv.steamId), vehicle, dbv.id);
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
            String fileName = ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Level/" + Level.info.name + "/Vehicles.dat";
            if (File.Exists(fileName)) {
                File.Delete(fileName);
                Logger.Log("Deleted Vehicles.dat.");
            } else
                Logger.Log("Could not find Vehicles.dat.");
        }

        public void onPlayerConnected(UnturnedPlayer player) {

        }

        public void onPlayerDisconnected(UnturnedPlayer player) {

        }

        public void onPlayerEnterVehicle(UnturnedPlayer player, InteractableVehicle vehicle) {
            if (vehiclesToBeDestroyed.ContainsKey(vehicle.instanceID))
                vehiclesToBeDestroyed.Remove(vehicle.instanceID);

            if (!vehicleOwners.ContainsKey(vehicle.instanceID)) {
                // Vehicle was spawned with /v or unturned respawned it
                storeOwnedVehicle(new CSteamID(0), vehicle);
            }

            CSteamID owner = getVehicleOwner(player.CurrentVehicle);
            String vehicleName = getVehicleTypeName(player.CurrentVehicle.id);
            if (owner.m_SteamID != 0) {
                VehicleInfo info = getOwnedVehicleInfo(player.CurrentVehicle.instanceID);
                String whoNickname = (info != null ? info.ownerName : "Unknown player");

                if (player.CSteamID.Equals(owner))
                    UnturnedChat.Say(player, "Welcome back to your " + vehicleName + ", " + player.DisplayName + "!");
                else
                    UnturnedChat.Say(player, "This " + vehicleName + " belongs to " + whoNickname + ".");
            } else
                UnturnedChat.Say(player, "This natural " + vehicleName + " will despawn when its fuel level is low.");

        }

        public void onPlayerExitVehicle(UnturnedPlayer player, InteractableVehicle vehicle) {
            if (vehicle != null) {
                VehicleInfo info = getOwnedVehicleInfo(vehicle.instanceID);
                if (info != null) {
                    if (info.ownerId.m_SteamID == 0) {
                        if (vehicle.fuel <= 5 && !vehiclesToBeDestroyed.ContainsKey(vehicle.instanceID)) {
                            DestroyingVehicleInfo destroyingInfo = new DestroyingVehicleInfo();
                            destroyingInfo.vehicle = vehicle;
                            destroyingInfo.lastActivity = DateTime.Now;
                            vehiclesToBeDestroyed.Add(vehicle.instanceID, destroyingInfo);
                        }
                    }

                    if (!lastSave.ContainsKey(vehicle.instanceID) || !isSimilar(vehicle, lastSave[vehicle.instanceID])) {
                        DatabaseVehicle dbv = DatabaseVehicle.fromInteractableVehicle(info.databaseId, info.ownerId.m_SteamID, vehicle);
                        AlskeboUnturnedPlugin.databaseManager.updateVehicle(dbv);
                        if (lastSave.ContainsKey(vehicle.instanceID))
                            lastSave[vehicle.instanceID] = dbv;
                        else
                            lastSave.Add(vehicle.instanceID, dbv);
                    }
                }
            }
        }

        private void tick(object sender, ElapsedEventArgs e) {
            List<uint> toRemove = new List<uint>();
            foreach (KeyValuePair<uint, DestroyingVehicleInfo> pair in vehiclesToBeDestroyed) {
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
                    toRemove.Add(pair.Value.vehicle.instanceID);
                    break;
                }
            }
            foreach (uint r in toRemove) {
                vehiclesToBeDestroyed.Remove(r);
            }
        }

        private void saveVehicles(object sender, ElapsedEventArgs e) {
            foreach (InteractableVehicle vehicle in VehicleManager.Vehicles) {
                if ((vehicle.isDead && Time.realtimeSinceStartup - vehicle.lastExploded >= Provider.modeConfigData.Vehicles.Respawn_Time)
                    || (vehicle.isDrowned && Time.realtimeSinceStartup - vehicle.lastUnderwater >= Provider.modeConfigData.Vehicles.Respawn_Time)) {

                    // The vehicle should be removed!
                    if (vehicleOwners.ContainsKey(vehicle.instanceID)) {
                        VehicleInfo info = vehicleOwners[vehicle.instanceID];
                        deleteOwnedVehicle(info.databaseId, vehicle.instanceID);
                        Logger.Log("Deleted exploded/drowned/dead vehicle with ID " + info.databaseId + ".");
                    }
                } else if (!vehicleOwners.ContainsKey(vehicle.instanceID)) {
                    // Vehicle was spawned with /v or unturned respawned it
                    storeOwnedVehicle(new CSteamID(0), vehicle);
                }
            }

            foreach (KeyValuePair<uint, VehicleInfo> pair in vehicleOwners) {
                InteractableVehicle vehicle = VehicleManager.getVehicle(pair.Key);
                if (!lastSave.ContainsKey(pair.Key) || !isSimilar(vehicle, lastSave[pair.Key])) {
                    DatabaseVehicle dbv = DatabaseVehicle.fromInteractableVehicle(pair.Value.databaseId, pair.Value.ownerId.m_SteamID, vehicle);
                    AlskeboUnturnedPlugin.databaseManager.updateVehicle(dbv);
                    if (lastSave.ContainsKey(vehicle.instanceID))
                        lastSave[vehicle.instanceID] = dbv;
                    else
                        lastSave.Add(vehicle.instanceID, dbv);
                }
            }
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

        private void storeOwnedVehicle(CSteamID ownerId, InteractableVehicle vehicle, long databaseId = -1) {
            if (databaseId == -1)
                databaseId = AlskeboUnturnedPlugin.databaseManager.insertOwnedVehicle(ownerId, vehicle);
            if (!playerOwnedVehicles.ContainsKey(ownerId))
                playerOwnedVehicles.Add(ownerId, new List<VehicleInfo>());

            List<VehicleInfo> list = playerOwnedVehicles[ownerId];
            VehicleInfo info = new VehicleInfo();
            info.instanceId = vehicle.instanceID;
            info.databaseId = databaseId;
            info.ownerId = ownerId;
            info.ownerName = "TEMPORARY NAME";
            if (ownerId.m_SteamID == 0)
                info.ownerName = "Nature";
            list.Add(info);
            playerOwnedVehicles[ownerId] = list;
            vehicleOwners.Add(vehicle.instanceID, info);
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
