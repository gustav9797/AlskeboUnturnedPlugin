using Rocket.API;
using Rocket.Core.Assets;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;

namespace AlskeboUnturnedPlugin {
    public class InfoCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
        }

        public string Name {
            get { return "Info"; }
        }

        public string Help {
            get { return "Help!"; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            foreach (String message in AlskeboUnturnedPlugin.advertiser.messages)
                UnturnedChat.Say(caller, message);
            UnturnedChat.Say(caller, "Also visit Alskebo.com for additional information.");
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.info");
                return list;
            }
        }
    }
}
