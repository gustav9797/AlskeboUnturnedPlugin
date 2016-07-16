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
        public bool isNoob;

        public VehicleInfo(uint instanceId, long databaseId, CSteamID ownerId, CSteamID groupId, String ownerName, bool isLocked, bool isNoob) {
            this.instanceId = instanceId;
            this.databaseId = databaseId;
            this.ownerId = ownerId;
            this.groupId = groupId;
            this.ownerName = ownerName;
            this.isLocked = isLocked;
            this.isNoob = isNoob;
        }

        public bool isNatural { get { return ownerId.m_SteamID == 0; } }

        public bool hasGroup { get { return !isNatural && groupId.m_SteamID != 0; } }
    }
}
