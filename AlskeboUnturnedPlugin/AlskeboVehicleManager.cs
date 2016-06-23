using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlskeboUnturnedPlugin
{
    public class AlskeboVehicleManager
    {
        Dictionary<CSteamID, List<uint>> playerOwnedVehicles = new Dictionary<CSteamID, List<uint>>();
        Dictionary<uint, CSteamID> vehicleOwners = new Dictionary<uint, CSteamID>();

        public InteractableVehicle givePlayerOwnedCar(UnturnedPlayer player, ushort carId)
        {
            InteractableVehicle vehicle = giveVehicle(player, carId);
            if (vehicle == null)
            {
                UnturnedChat.Say(player, "Could not give you a vehicle. Please exit your current one.");
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

        public CSteamID getVehicleOwner(InteractableVehicle vehicle)
        {
            if (vehicleOwners.ContainsKey(vehicle.instanceID))
            {
                CSteamID id = vehicleOwners[vehicle.instanceID];
                return id;
            }
            return CSteamID.Nil;
        }

        public String getVehicleTypeName(ushort id)
        {
            Asset a = SDG.Unturned.Assets.find(EAssetType.VEHICLE, id);
            if (a != null)
                return a.name;
            return "Unknown vehicle";
        }

        private static InteractableVehicle giveVehicle(UnturnedPlayer player, ushort id)
        {
            InteractableVehicle vehicle = CustomVehicleManager.customSpawnVehicle(id, player.Position, new Quaternion(0, 0, 0, 0));
            return vehicle;
        }


    }
}
