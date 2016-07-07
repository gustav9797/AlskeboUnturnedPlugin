using Steamworks;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class VehicleInfo {
        public uint instanceId;
        public long databaseId;
        public CSteamID ownerId;
        public CSteamID groupId;
        public String ownerName;
        public bool isLocked;
        public bool shouldHeadlights = false;
        public DateTime lastHeadlights;

        public bool isNatural { get { return ownerId.m_SteamID == 0; } }
    }
}
