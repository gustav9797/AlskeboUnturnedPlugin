using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlskeboUnturnedPlugin {
    public class AlskeboConfiguration : IRocketPluginConfiguration {
        public string databaseIP;

        public void LoadDefaults() {
            databaseIP = "";
        }
    }
}
