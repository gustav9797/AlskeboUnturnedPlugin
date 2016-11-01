using Rocket.API;
using Rocket.Core.Assets;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;

using System.Text;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class ColorCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
        }

        public string Name {
            get { return "color"; }
        }

        public string Help {
            get { return ""; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            byte r = 255;
            byte g = 255;
            byte b = 255;
            string message = "Test message.";
            if (command.Length >= 3 && byte.TryParse(command[0], out r) && byte.TryParse(command[1], out g) && byte.TryParse(command[2], out b)) {
                if (command.Length >= 4) {
                    message = command[3];
                }
                UnturnedChat.Say(player, message, UnturnedChat.GetColorFromRGB(r, g, b));
            } else if (command.Length >= 1) {
                message = command[0];
                UnturnedChat.Say(player, message);
            }
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.color");
                return list;
            }
        }
    }
}
