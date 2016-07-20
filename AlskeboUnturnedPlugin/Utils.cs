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
    }
}
