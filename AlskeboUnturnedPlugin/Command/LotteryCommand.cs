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
    public class LotteryCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Both; }
        }

        public string Name {
            get { return "Lottery"; }
        }

        public string Help {
            get { return "Show info about the lottery."; }
        }

        public string Syntax {
            get { return ""; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            TimeSpan timeToNextDraw = Lottery.timeToNextDraw();
            UnturnedChat.Say(caller, "The next lottery draw is in " + timeToNextDraw.Minutes + " minutes and " + timeToNextDraw.Seconds + " seconds. Buy a ticket with \"/buyticket\".");
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.lottery");
                return list;
            }
        }
    }
}
