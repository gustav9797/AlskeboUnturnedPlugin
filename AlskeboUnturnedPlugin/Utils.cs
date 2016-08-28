using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    class Utils {

        public static LocationNode getClosestLocation(Vector3 pos) {
            LocationNode closest = null;
            float closestDistance = float.MaxValue;
            foreach (Node node in LevelNodes.Nodes) {
                if (node.type == ENodeType.LOCATION) {
                    float dist = Vector3.Distance(node.Position, pos);
                    if (closest == null || dist < closestDistance) {
                        closest = (LocationNode)node;
                        closestDistance = dist;
                    }
                }
            }
            return closest;
        }

        public static String getVehicleTypeName(ushort id) {
            Asset a = SDG.Unturned.Assets.find(EAssetType.VEHICLE, id);
            if (a != null)
                return ((VehicleAsset)a).Name;
            return "Unknown vehicle";
        }

        public static int getFuelPercentage(InteractableVehicle vehicle) {
            return (int)Math.Floor(((float)vehicle.fuel / (float)vehicle.asset.fuel) * 100);
        }

        public static string toDelay(uint time) {
            uint input = Provider.time - time;

            uint days = input / 60 / 60 / 24;
            input -= days * 24 * 60 * 60;

            uint hours = input / 60 / 60;
            input -= hours * 60 * 60;

            uint minutes = input / 60;
            input -= minutes * 60;

            uint seconds = input;                      
           
            String s = "";
            s += (days > 0 ? days + " " + (days == 1 ? "day" : "days") + " " : "");
            s += (hours > 0 ? hours + " " + (hours == 1 ? "hour" : "hours") + " " : "");
            if (s == "") {
                s += (minutes > 0 ? minutes + " " + (minutes == 1 ? "minute" : "minutes") + " " : "");
                s += input + " " + (seconds == 1 ? "seconds" : "seconds") + " ";
            }
            s += "ago";
            return s;
        }
    }
}
