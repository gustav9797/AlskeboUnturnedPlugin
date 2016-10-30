using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace AlskeboUnturnedPlugin {
    public class Lottery {
        private static Timer timer;
        private static DateTime lastDraw = DateTime.Now;
        private static Color color = Color.magenta;
        private static Dictionary<CSteamID, int> tickets = new Dictionary<CSteamID, int>();
        private static Dictionary<int, CSteamID> players = new Dictionary<int, CSteamID>();
        private static List<ushort> winnableItems = new List<ushort>();
        public static int maxNumber = 20;

        public Lottery() {
            winnableItems.Add(297);
            winnableItems.Add(18);
            winnableItems.Add(300);
            winnableItems.Add(488);
            winnableItems.Add(1230);
            winnableItems.Add(1219);
            winnableItems.Add(1240);
            winnableItems.Add(1244);
            winnableItems.Add(1229);
            winnableItems.Add(1228);
            winnableItems.Add(1111);
            winnableItems.Add(1055);
            winnableItems.Add(1194);
            winnableItems.Add(1440);
            winnableItems.Add(1436);

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += tick;
            timer.Start();
        }

        private void tick(object sender, ElapsedEventArgs e) {
            TimeSpan timeToNextDraw = Lottery.timeToNextDraw();
            if (Math.Round(timeToNextDraw.TotalSeconds) % 60 != 0)
                return;
            int minutesToNextDraw = (int)Math.Round(timeToNextDraw.TotalSeconds) / 60;
            if (minutesToNextDraw <= 0) {
                lastDraw = DateTime.Now;

                foreach (CSteamID id in players.Values.ToArray()) {
                    Player player = PlayerTool.getPlayer(id);
                    if (player == null) {
                        int ticket = tickets[id];
                        players.Remove(ticket);
                        tickets.Remove(id);
                    }
                }

                int winner = AlskeboUnturnedPlugin.r.Next(0, maxNumber + 1);
                UnturnedChat.Say("Winning lottery number: " + winner, color);
                if (players.ContainsKey(winner)) {
                    UnturnedPlayer player = UnturnedPlayer.FromCSteamID(players[winner]);
                    win(player);
                } else
                    UnturnedChat.Say("Nobody picked the right number this time.", color);

                tickets.Clear();
                players.Clear();
                UnturnedChat.Say("The next lottery draw is in 30 minutes. (/buyticket)", color);
            } else if (minutesToNextDraw == 1) {
                UnturnedChat.Say("The next lottery draw is in 1 minute. (/buyticket)", color);
            } else if (minutesToNextDraw == 5) {
                UnturnedChat.Say("The next lottery draw is in 5 minutes. (/buyticket)", color);
            } else if (minutesToNextDraw == 15) {
                UnturnedChat.Say("The next lottery draw is in 15 minutes. (/buyticket)", color);
            }
        }

        public static void win(UnturnedPlayer player) {
            for (ushort i = 80; i <= 90; ++i)
                EffectManager.sendEffect(i, 500, player.Position);
            ushort niceItem = winnableItems[AlskeboUnturnedPlugin.r.Next(0, winnableItems.Count)];
            string niceItemName = UnturnedItems.GetItemAssetById(niceItem).Name;
            UnturnedChat.Say("Congratulations to " + player.DisplayName + " who won a " + niceItemName + " + more stuff!", color);
            for (int i = 0; i <= AlskeboUnturnedPlugin.r.Next(7, 10); ++i)
                player.Inventory.forceAddItem(new Item((ushort)AlskeboUnturnedPlugin.r.Next(1, 522), true), true);
            for (int i = 0; i <= AlskeboUnturnedPlugin.r.Next(7, 10); ++i)
                player.Inventory.forceAddItem(new Item((ushort)AlskeboUnturnedPlugin.r.Next(1000, 1323), true), true);
            player.Inventory.forceAddItem(new Item(niceItem, true), true);
            player.Inventory.sendStorage();
            UnturnedChat.Say(player, "You won the lottery! Check your inventory.");
        }

        public static void onCommand(UnturnedPlayer player, int ticket) {
            if (tickets.ContainsKey(player.CSteamID)) {
                UnturnedChat.Say(player, "You already bought a ticket. (" + tickets[player.CSteamID] + ")");
                return;
            } else if (players.ContainsKey(ticket)) {
                UnturnedChat.Say(player, "This number is taken.");
                return;
            }

            int ticketCost = 1;
            if (!EconomyManager.hasBalance(player, ticketCost)) {
                UnturnedChat.Say(player, "You need $" + ticketCost + " to enter the lottery.");
                return;
            }

            EconomyManager.setBalance(player, EconomyManager.getBalance(player) - ticketCost);
            tickets.Add(player.CSteamID, ticket);
            players.Add(ticket, player.CSteamID);
            AlskeboUnturnedPlugin.databaseManager.logPlayerAsync(player.CSteamID, PlayerLogType.BUY_TICKET);
            UnturnedChat.Say(player, "You bought a ticket with number " + ticket + "!");
        }

        public static TimeSpan timeToNextDraw() {
            return lastDraw.Add(new TimeSpan(0, 30, 0)) - DateTime.Now;
        }
    }
}
