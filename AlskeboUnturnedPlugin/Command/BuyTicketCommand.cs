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
    public class BuyTicketCommand : IRocketCommand {
        public AllowedCaller AllowedCaller {
            get { return AllowedCaller.Player; }
        }

        public string Name {
            get { return "BuyTicket"; }
        }

        public string Help {
            get { return "Buy a lottery ticket."; }
        }

        public string Syntax {
            get { return "<number from 0 to " + Lottery.maxNumber + ">"; }
        }

        public List<string> Aliases {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command) {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            int ticket = 0;
            if (command.Length >= 1 && int.TryParse(command[0], out ticket) && ticket >= 0 && ticket <= Lottery.maxNumber) {
                Lottery.onCommand(player, ticket);
            } else
                UnturnedChat.Say(player, "Usage: /buyticket " + Syntax);
        }

        public List<string> Permissions {
            get {
                List<String> list = new List<string>();
                list.Add("a.buyticket");
                return list;
            }
        }
    }
}
