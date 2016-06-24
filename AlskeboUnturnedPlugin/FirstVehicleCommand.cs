using MongoDB.Bson;
using MongoDB.Driver;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlskeboUnturnedPlugin {
    public class FirstVehicleCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
        }

        public string Name {
            get { return "FirstVehicle"; }
        }

        public string Help {
            get { return "Retrieve your first vehicle."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            if (caller is UnturnedPlayer) {
                UnturnedPlayer player = (UnturnedPlayer)caller;
                var collection = AlskeboUnturnedPlugin.databaseManager.getPlayersCollection();
                var filter = Builders<BsonDocument>.Filter.Eq("steamid", player.CSteamID.m_SteamID);
                var cursor = collection.FindSync(filter);
                if (cursor.MoveNext()) {
                    BsonDocument current = cursor.Current.First();
                    if (!current.GetValue("receivedvehicle").AsBoolean) {
                        AlskeboUnturnedPlugin.vehicleManager.givePlayerOwnedCar(player, (ushort)AlskeboUnturnedPlugin.r.Next(25, 33));
                        current.Set("receivedvehicle", true);
                        collection.UpdateOne(filter, current);
                    } else {
                        UnturnedChat.Say("You have already received your noob-car once.");
                    }
                } else
                    UnturnedChat.Say("There was a database problem. Please rejoin the server.");
            } else {
                UnturnedChat.Say(caller, "You must be in-game to execute this command.");
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("alskebo.firstvehicle");
                return list;
            }
        }
    }
}
