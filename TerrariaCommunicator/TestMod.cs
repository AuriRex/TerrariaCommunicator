using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace TerrariaCommunicator
{
    public class TestMod : Mod
    {

        Color discord = new Color(114, 137, 218);

        private Thread _test;
        private List<TextSnippet> text;
        private int count = 0;
        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        public override void Load()
        {
            var a = Logging.LogPath;
            
            Logger.Info("LOGGGGGEGEGEGEGGERRR");
            if (Main.dedServ)
            {
                Logger.Info($"{nameof(TestMod)} loading!");
                _test = new Thread(Run);
                _test.Start();

                On.Terraria.NetMessage.BroadcastChatMessage += NetMessage_BroadcastChatMessage;
                On.Terraria.NetMessage.SendChatMessageFromClient += NetMessage_SendChatMessageFromClient;
                On.Terraria.NetMessage.SendChatMessageToClient += NetMessage_SendChatMessageToClient;
                On.Terraria.GameContent.NetModules.NetTextModule.DeserializeAsServer += NetTextModule_DeserializeAsServer;
            }

            
        }

        private bool NetTextModule_DeserializeAsServer(On.Terraria.GameContent.NetModules.NetTextModule.orig_DeserializeAsServer orig, Terraria.GameContent.NetModules.NetTextModule self, System.IO.BinaryReader reader, int senderPlayerId)
        {
            var test = self.Deserialize(reader, senderPlayerId);

            

            return orig(self, reader, senderPlayerId);
        }

        private void NetMessage_SendChatMessageToClient(On.Terraria.NetMessage.orig_SendChatMessageToClient orig, Terraria.Localization.NetworkText text, Color color, int playerId)
        {

            orig(text, color, playerId);
            orig(text, discord, playerId);
        }

        private void NetMessage_SendChatMessageFromClient(On.Terraria.NetMessage.orig_SendChatMessageFromClient orig, Terraria.Chat.ChatMessage text)
        {
            // Probably client side message

            var test = new Terraria.Chat.ChatMessage("test");

            orig(text);
            orig(test);
        }

        private void NetMessage_greetPlayer(On.Terraria.NetMessage.orig_greetPlayer orig, int plr)
        {

        }

        // Only for system messages etc
        private void NetMessage_BroadcastChatMessage(On.Terraria.NetMessage.orig_BroadcastChatMessage orig, Terraria.Localization.NetworkText text, Color color, int excludedPlayer)
        {
            //Logger.Info($"Chat: {text}");

            //text.

            orig(text, color, excludedPlayer);
            orig(text, new Color(255,0,0), excludedPlayer);
        }

        public override void Close()
        {
            Unload();
        }

        public override void Unload()
        {
            _shutdownEvent.Set();
            _test?.Abort();
            _test?.Join(200);
            On.Terraria.NetMessage.BroadcastChatMessage -= NetMessage_BroadcastChatMessage;
        }

        private void Run()
        {
            while (!_shutdownEvent.WaitOne(0))
            {
                Thread.Sleep(3000);
                text = new List<TextSnippet>();

                text.Add(new TextSnippet("DiscordName >", discord));
                text.Add(new TextSnippet(" The cool new message of that dood on discord.", Color.White));
                text.Add(new TextSnippet($" {count++} - IsDedicatedServer:{Main.dedServ} {Main.dedServFPS}", Color.Red));
                Main.NewText(text);
            }
        }

    }
}
