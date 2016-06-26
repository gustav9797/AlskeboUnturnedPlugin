using Rocket.Core.Logging;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class CustomVehicleManager {
        public static byte customSAVEDATA_VERSION { get { return VehicleManager.SAVEDATA_VERSION; } }
        public static VehicleManager customInstance { get { return VehicleManager.Instance; } }
        public static List<InteractableVehicle> customVehicles { get { return VehicleManager.vehicles; } }

        private static uint custominstanceCount {
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

        private static ushort customrespawnVehicleIndex {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("respawnVehicleIndex", BindingFlags.NonPublic | BindingFlags.Static);
                ushort value = (ushort)info.GetValue(null);
                return value;
            }
        }

        private static float customlastTick {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("lastTick", BindingFlags.NonPublic | BindingFlags.Static);
                float value = (float)info.GetValue(null);
                return value;
            }
        }

        private uint customseq {
            get {
                Type type = typeof(VehicleManager);
                FieldInfo info = type.GetField("seq", BindingFlags.NonPublic | BindingFlags.Static);
                uint value = (uint)info.GetValue(null);
                return value;
            }
        }

        public static InteractableVehicle customSpawnVehicle(ushort id, Vector3 point, Quaternion angle) {
            VehicleAsset vehicleAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
            if (vehicleAsset == null)
                return null;

            InteractableVehicle vehicle = customAddVehicle(id, point, angle, false, false, false, vehicleAsset.fuel, false, vehicleAsset.health, (CSteamID[])null, ++custominstanceCount);
            VehicleManager.Instance.channel.openWrite();
            VehicleManager.Instance.sendVehicle(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1]);
            VehicleManager.Instance.channel.closeWrite("tellVehicle", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
            BarricadeManager.askPlants(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1].transform);
            return vehicle;
        }

        private static InteractableVehicle customAddVehicle(ushort id, Vector3 point, Quaternion angle, bool sirens, bool headlights, bool taillights, ushort fuel, bool isExploded, ushort health, CSteamID[] passengers, uint instanceID) {
            VehicleAsset vehicleAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
            if (vehicleAsset != null) {
                Transform parent = !Dedicator.isDedicated || !((UnityEngine.Object)vehicleAsset.clip != (UnityEngine.Object)null) ? UnityEngine.Object.Instantiate<GameObject>(vehicleAsset.vehicle).transform : UnityEngine.Object.Instantiate<GameObject>(vehicleAsset.clip).transform;
                parent.name = id.ToString();
                parent.parent = LevelVehicles.models;
                parent.position = point;
                parent.rotation = angle;
                parent.GetComponent<Rigidbody>().useGravity = true;
                parent.GetComponent<Rigidbody>().isKinematic = false;
                InteractableVehicle interactableVehicle = parent.gameObject.AddComponent<InteractableVehicle>();
                interactableVehicle.instanceID = instanceID;
                interactableVehicle.id = id;
                interactableVehicle.fuel = fuel;
                interactableVehicle.isExploded = isExploded;
                interactableVehicle.health = health;
                interactableVehicle.init();
                interactableVehicle.tellSirens(sirens);
                interactableVehicle.tellHeadlights(headlights);
                interactableVehicle.tellTaillights(taillights);
                if (passengers != null) {
                    for (byte seat = (byte)0; (int)seat < passengers.Length; ++seat) {
                        if (passengers[(int)seat] != CSteamID.Nil)
                            interactableVehicle.addPlayer(seat, passengers[(int)seat]);
                    }
                }
                VehicleManager.vehicles.Add(interactableVehicle);
                BarricadeManager.waterPlant(parent);
                return interactableVehicle;
            } else {
                Logger.LogError("customAddVehicle: Could not find asset with id " + id);
                if (Provider.isServer)
                    return null;
                Provider.connectionFailureInfo = ESteamConnectionFailureInfo.VEHICLE;
                Provider.disconnect();
            }
            return null;
        }
    }
}
