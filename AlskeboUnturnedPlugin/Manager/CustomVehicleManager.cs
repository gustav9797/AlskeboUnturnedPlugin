using Rocket.Core.Logging;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

using System.Reflection;
using System.Text;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class CustomVehicleManager {
        public static byte customSAVEDATA_VERSION { get { return VehicleManager.SAVEDATA_VERSION; } }
        public static VehicleManager customInstance { get { return VehicleManager.Instance; } }
        public static List<InteractableVehicle> customVehicles { get { return VehicleManager.vehicles; } }

        public static uint custominstanceCount {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("instanceCount", BindingFlags.NonPublic | BindingFlags.Static);
                uint value = (uint)info.GetValue(null);
                return value;
            }
            set {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("instanceCount", BindingFlags.NonPublic | BindingFlags.Static);
                info.SetValue(null, value);
            }
        }

        public static ushort customrespawnVehicleIndex {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("respawnVehicleIndex", BindingFlags.NonPublic | BindingFlags.Static);
                ushort value = (ushort)info.GetValue(null);
                return value;
            }
            set {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("respawnVehicleIndex", BindingFlags.NonPublic | BindingFlags.Static);
                info.SetValue(null, value);
            }
        }

        public static float customlastTick {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("lastTick", BindingFlags.NonPublic | BindingFlags.Static);
                float value = (float)info.GetValue(null);
                return value;
            }
        }

        public static uint customseq {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("seq", BindingFlags.NonPublic | BindingFlags.Instance);
                uint value = (uint)info.GetValue(VehicleManager.Instance);
                return value;
            }
            set {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("seq", BindingFlags.NonPublic | BindingFlags.Instance);
                info.SetValue(CustomVehicleManager.customInstance, value);
            }
        }

        public static InteractableVehicle customSpawnVehicle(ushort id, Vector3 point, Quaternion angle) {
            VehicleAsset asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
            if (asset != null) {
                InteractableVehicle vehicle = customAddVehicle(id, point, angle, false, false, false, asset.fuel, false, asset.health, CSteamID.Nil, CSteamID.Nil, false, null, null, ++custominstanceCount);
                VehicleManager.Instance.channel.openWrite();
                VehicleManager.Instance.sendVehicle(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1]);
                VehicleManager.Instance.channel.closeWrite("tellVehicle", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
                BarricadeManager.askPlants(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1].transform);
                return vehicle;
            }
            return null;
        }

        private static InteractableVehicle customAddVehicle(ushort id, Vector3 point, Quaternion angle, bool sirens, bool headlights, bool taillights, ushort fuel, bool isExploded, ushort health, CSteamID owner, CSteamID group, bool locked, CSteamID[] passengers, byte[][] turrets, uint instanceID) {
            VehicleAsset asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
            if (asset != null) {
                Transform transform;
                if (Dedicator.isDedicated && (asset.clip != null)) {
                    transform = UnityEngine.Object.Instantiate<GameObject>(asset.clip).transform;
                } else {
                    transform = UnityEngine.Object.Instantiate<GameObject>(asset.vehicle).transform;
                }
                transform.name = id.ToString();
                transform.parent = LevelVehicles.models;
                transform.position = point;
                transform.rotation = angle;
                transform.GetComponent<Rigidbody>().useGravity = true;
                transform.GetComponent<Rigidbody>().isKinematic = false;
                InteractableVehicle item = transform.gameObject.AddComponent<InteractableVehicle>();
                item.instanceID = instanceID;
                item.id = id;
                item.fuel = fuel;
                item.isExploded = isExploded;
                item.health = health;
                item.init();
                item.tellSirens(sirens);
                item.tellHeadlights(headlights);
                item.tellTaillights(taillights);
                item.tellLocked(owner, group, locked);
                if (Provider.isServer) {
                    if (turrets != null) {
                        for (byte i = 0; i < item.turrets.Length; i = (byte)(i + 1)) {
                            item.turrets[i].state = turrets[i];
                        }
                    } else {
                        for (byte j = 0; j < item.turrets.Length; j = (byte)(j + 1)) {
                            ItemAsset asset2 = (ItemAsset)Assets.find(EAssetType.ITEM, asset.turrets[j].itemID);
                            if (asset2 != null) {
                                item.turrets[j].state = asset2.getState();
                            } else {
                                item.turrets[j].state = null;
                            }
                        }
                    }
                }
                if (passengers != null) {
                    for (byte k = 0; k < passengers.Length; k = (byte)(k + 1)) {
                        if (passengers[k] != CSteamID.Nil) {
                            item.addPlayer(k, passengers[k]);
                        }
                    }
                }
                VehicleManager.vehicles.Add(item);
                BarricadeManager.waterPlant(transform);
                return item;
            } else if (!Provider.isServer) {
                Provider.connectionFailureInfo = ESteamConnectionFailureInfo.VEHICLE;
                Provider.disconnect();
            }
            return null;
        }

        public static void sendForceVehicleLock(InteractableVehicle vehicle, CSteamID owner, CSteamID group, bool locked) {
            SteamChannel channel = VehicleManager.Instance.channel;
            object[] objArray = new object[4];
            objArray[0] = vehicle.instanceID;
            objArray[1] = owner;
            objArray[2] = group;
            objArray[3] = locked;
            channel.send("tellVehicleLock", (ESteamCall)1, (ESteamPacket)15, objArray);
        }

        public static void sendVehicleHeadlights(InteractableVehicle vehicle) {
            bool newHeadlights = !vehicle.headlightsOn;
            vehicle.tellHeadlights(newHeadlights);
            SteamChannel channel = VehicleManager.Instance.channel;
            string name = "tellVehicleHeadlights";
            object[] objArray = new object[2];
            objArray[0] = vehicle.instanceID;
            objArray[1] = (newHeadlights ? 1 : 0);
            channel.send(name, (ESteamCall)1, (ESteamPacket)15, objArray);
        }

        public static void sendVehicleHeadlights(InteractableVehicle vehicle, bool on) {
            vehicle.tellHeadlights(on);
            SteamChannel channel = VehicleManager.Instance.channel;
            string name = "tellVehicleHeadlights";
            object[] objArray = new object[2];
            objArray[0] = vehicle.instanceID;
            objArray[1] = (on ? 1 : 0);
            channel.send(name, (ESteamCall)1, (ESteamPacket)15, objArray);
        }

        public static void spawnNaturalVehicles() {
            VehicleSpawnpoint spawn = null;
            if (AlskeboUnturnedPlugin.vehicleManager.NaturalVehicleCount < (Level.vehicles + (LevelVehicles.spawns.Count - Level.vehicles) / 2)) {
                spawn = LevelVehicles.spawns[UnityEngine.Random.Range(0, LevelVehicles.spawns.Count)];
                for (ushort i = 0; i < VehicleManager.vehicles.Count; i = (ushort)(i + 1)) {
                    Vector3 vector2 = VehicleManager.vehicles[i].transform.position - spawn.point;
                    if (vector2.sqrMagnitude < 64f) {
                        return;
                    }
                }
            }
            if (spawn != null) {
                Vector3 point = spawn.point;
                point.y++;
                ushort id = LevelVehicles.getVehicle(spawn);
                if (((VehicleAsset)Assets.find(EAssetType.VEHICLE, id)) != null) {
                    InteractableVehicle vehicle = AlskeboUnturnedPlugin.vehicleManager.spawnNaturalVehicle(point, Quaternion.Euler(0f, spawn.angle, 0f), id);
                    vehicle.fuel = (ushort)UnityEngine.Random.Range((int)vehicle.asset.fuelMin, (int)vehicle.asset.fuelMax);
                    vehicle.health = (ushort)UnityEngine.Random.Range((int)vehicle.asset.healthMin, (int)vehicle.asset.healthMax);
                    VehicleManager.sendVehicleFuel(vehicle, vehicle.fuel);
                    VehicleManager.sendVehicleHealth(vehicle, vehicle.health);
                }
            }
        }
    }
}
