using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using TerrariaCommunicator.Extensions;
using TerrariaCommunicator_NetworkPlugin.Managers;
using TerrariaCommunicator_NetworkPlugin.Models;
using TerrariaCommunicator_NetworkPlugin.Packets;

namespace TerrariaCommunicator
{
    public class TerrariaCommunicatorMod : Mod
    {

        public static class Colors
        {
            public static readonly Color discord = new Color(114, 137, 218);
        }

        public override void Load()
        {
            if (!Main.dedServ) return;

            Logger.Info($"{nameof(TerrariaCommunicatorMod)} loading!");

            CommunicationManager.Instance.OnMessageReceivedEvent += CommunicationManager_OnMessageReceivedEvent;
            CommunicationManager.Instance.OnDisconnectedEvent += CommunicationManager_OnDisconnectedEvent;
            CommunicationManager.Instance.LogAction = Logger.Info;
            CommunicationManager.Instance.Initialize(tModLoader: true);
            
            On.Terraria.NetMessage.BroadcastChatMessage += NetMessage_BroadcastChatMessage;
            On.Terraria.Chat.ChatCommandProcessor.ProcessReceivedMessage += ChatCommandProcessor_ProcessReceivedMessage;
        }

        public static bool IntentionalBroadcastPleaseDontHook { get; private set; } = false;

        private void CommunicationManager_OnMessageReceivedEvent(DiscordMessagePacket.Content content)
        {
            var text = Terraria.Localization.NetworkText.FromLiteral($"[c/7289DA:{content.Username} >] {content.Message}");

            IntentionalBroadcastPleaseDontHook = true;
            Terraria.NetMessage.BroadcastChatMessage(text, Color.White);
            IntentionalBroadcastPleaseDontHook = false;
        }

        private void CommunicationManager_OnDisconnectedEvent()
        {

        }

        private void OnChatMessage(string message, Player sender)
        {
            var text = message;

            if (string.IsNullOrWhiteSpace(text)) return;

            if (text.Length > 3700) return;

            if (text.StartsWith("/"))
            {
                // Maybe add command packet, idk
                return;
            }

            var textSnippets = ChatManager.ParseMessage(text, Color.White);

            var sb = new StringBuilder();
            foreach(var snip in textSnippets)
            {
                sb.Append(snip.Text);
            }

            text = sb.ToString();

            CommunicationManager.Instance?.SendPacket(new ChatMessagePacket()
            {
                PacketData = new ChatMessagePacket.Content
                {
                    Message = text,
                    PlayerInfo = sender.ToComsPlayerInfo()
                }
            });
        }

        private void OnServerBroadcast(string message, Color color)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            CommunicationManager.Instance?.SendPacket(new BroadcastMessagePacket()
            {
                PacketData = new BroadcastMessagePacket.Content
                {
                    Message = message,
                    Color = color.ToComsColor()
                }
            });
        }

        private bool ChatCommandProcessor_ProcessReceivedMessage(On.Terraria.Chat.ChatCommandProcessor.orig_ProcessReceivedMessage orig, Terraria.Chat.ChatCommandProcessor self, Terraria.Chat.ChatMessage message, int clientId)
        {
            Logger.Info($"{nameof(ChatCommandProcessor_ProcessReceivedMessage)}: {message?.Text}");

            Player sender = null;
            try
            {
                sender = Main.player[clientId];
            }
            catch(Exception)
            {

            }

            OnChatMessage(message.Text, sender);

            return orig(self, message, clientId);
        }

        // Only for system messages etc
        private void NetMessage_BroadcastChatMessage(On.Terraria.NetMessage.orig_BroadcastChatMessage orig, Terraria.Localization.NetworkText text, Color color, int excludedPlayer)
        {
            if(!IntentionalBroadcastPleaseDontHook)
            {
                Logger.Info($"{nameof(NetMessage_BroadcastChatMessage)}: {text} :excludedPlayer={excludedPlayer}");

                OnServerBroadcast(text.ToString(), color);
            }

            orig(text, color, excludedPlayer);
        }

        public override void Unload()
        {
            if (!Main.dedServ) return;

            On.Terraria.NetMessage.BroadcastChatMessage -= NetMessage_BroadcastChatMessage;
            On.Terraria.Chat.ChatCommandProcessor.ProcessReceivedMessage -= ChatCommandProcessor_ProcessReceivedMessage;

            CommunicationManager.Instance.Dispose();
        }

    }
}
