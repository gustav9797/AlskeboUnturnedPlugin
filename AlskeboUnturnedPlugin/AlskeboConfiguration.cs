using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlskeboUnturnedPlugin {
    public class AlskeboConfiguration : IRocketPluginConfiguration {
        public string databaseIP;
        public string database;
        public string username;
        public string password;

        public void LoadDefaults() {
            databaseIP = "";
            database = "unturned";
            username = "unturned";
            password = "13421342";
        }
    }
}
