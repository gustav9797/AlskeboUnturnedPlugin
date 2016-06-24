using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using Rocket.Unturned.Chat;

namespace AlskeboUnturnedPlugin {
    public class DatabaseManager {
        protected static IMongoClient client;
        protected static IMongoDatabase database;

        public DatabaseManager() {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress("127.0.0.1");
            client = new MongoClient(settings);
            database = client.GetDatabase("unturned");
            UnturnedChat.Say("Pinging MongoDB database...");
            database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();
            UnturnedChat.Say("Got ping.");
        }

        public IMongoCollection<BsonDocument> getPlayersCollection() {
            return database.GetCollection<BsonDocument>("players");
        }
    }
}
