using Microsoft.Xna.Framework;
using System;
using TerrariaCommunicator_NetworkPlugin.Managers;
using TerrariaCommunicator_NetworkPlugin.Models;
using TerrariaCommunicator_NetworkPlugin.Packets;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Utils = TShockAPI.Utils;

namespace TerrariaCommunicator_TShock
{
    [ApiVersion(2, 1)]
    public class TShockPlugin : TerrariaPlugin
    {
        public override string Name => "TerrariaCommunicator";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "AuriRex";
        public override string Description => "Description here TODO";

        public TShockPlugin(Main game) : base(game)
        {
            
        }

        public override void Initialize()
        {
            CommunicationManager.Instance.Initialize();
            CommunicationManager.Instance.OnMessageReceivedEvent += Instance_OnMessageReceivedEvent;

            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.ServerBroadcast.Register(this, OnServerBroadcast);
        }

        private void Instance_OnMessageReceivedEvent(DiscordMessagePacket.Content content)
        {
            SendMsg($"[c/7289DA:<D>] {content.Username} [c/7289DA:>] {content.Message}");
        }

        private void OnServerBroadcast(ServerBroadcastEventArgs args)
        {
            if (args != null)
            {
                if (args.Message == null) return;
                if (args.Color == null) return;
                Console.WriteLine($"Broadcast: {args.Message}");

                var str = args.Message.ToString().Split(new char[] { ':' }, 2);
                if (str.Length > 1 && Ignore.Player.Name == str[0] && Ignore.Text == str[1].Substring(1)) return;
                CommunicationManager.Instance?.SendPacket(new BroadcastMessagePacket()
                {
                    PacketData = new BroadcastMessagePacket.Content
                    {
                        Message = args.Message.ToString(),
                        Color = XNAColorToStruct(args.Color)
                    }
                });
            }

        }

        private ColorStruct XNAColorToStruct(Color color)
        {
            return new ColorStruct
            {
                R = color.R,
                G = color.G,
                B = color.B
            };
        }

        private void OnGamePostInitialize(EventArgs args)
        {
            CommunicationManager.Instance?.SendPacket(new BroadcastMessagePacket()
            {
                PacketData = new BroadcastMessagePacket.Content
                {
                    Message = "Server online!",
                    Color = new ColorStruct { R = 0, G = 255, B = 0 }
                }
            });
        }

        public void SendMsg(string msg)
        {
            if (TShock.Players == null) return;

            foreach (TSPlayer ply in TShock.Players)
            {
                if (ply == null) continue;

                ply.SendMessage(msg, 255, 255, 255);
            }
        }

        public TSPlayer[] GetOnlinePlayers()
        {
            return TShock.Players;
        }

        public string GetItemName(int id)
        {
            try
            {
                return Utils.Instance.GetItemById(id).Name;
            }
            catch (Exception)
            {
                return "Exception";
            }
        }

        public string GetPrefixName(int id)
        {
            try
            {
                return Utils.Instance.GetPrefixById(id);
            }
            catch (Exception)
            {
                return "Exception";
            }
        }

        void OnServerJoin(JoinEventArgs args)
        {
            if (args != null)
            {
                TSPlayer ply = TShock.Players[args.Who];
                if (ply != null)
                {
                    Console.WriteLine($"ServerJoin: {ply.Name}");
                    CommunicationManager.Instance?.SendPacket(new PlayerConnectionPacket()
                    {
                        PacketData = new PlayerConnectionPacket.Content
                        {
                            IsConnecting = true,
                            PlayerInfo = CreatePlayerInfo(ply)
                        }
                    });
                }
            }
        }

        void OnServerLeave(LeaveEventArgs args)
        {
            if (args != null)
            {
                TSPlayer ply = TShock.Players[args.Who];
                if (ply != null)
                {
                    CommunicationManager.Instance?.SendPacket(new PlayerConnectionPacket()
                    {
                        PacketData = new PlayerConnectionPacket.Content
                        {
                            IsConnecting = false,
                            PlayerInfo = CreatePlayerInfo(ply)
                        }
                    });
                }
            }
        }

        private class IgnoreThis
        {
            public TSPlayer Player { get; set; }
            public string Text { get; set; }
        }
        private IgnoreThis Ignore = new IgnoreThis();
        void OnServerChat(ServerChatEventArgs args)
        {
            if (args != null)
            {
                TSPlayer ply = TShock.Players[args.Who];
                if (ply != null)
                {
                    Console.WriteLine($"Chat: {args.Text}");

                    if (args.Text.StartsWith("/"))
                    {
                        // Maybe add command packet, idk
                        return;
                    }
                    CommunicationManager.Instance?.SendPacket(new ChatMessagePacket() {
                        PacketData = new ChatMessagePacket.Content
                        {
                            Message = args.Text,
                            PlayerInfo = CreatePlayerInfo(ply)
                        }
                    });

                    Ignore.Player = ply;
                    Ignore.Text = args.Text;
                }
            }
        }

        private PlayerInfo CreatePlayerInfo(TSPlayer ply)
        {
            return new PlayerInfo
            {
                IsDead = ply.Dead,
                Name = ply.Name,
                UUID = ply.UUID
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //unhook
                //dispose child objects
                //set large objects to null
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.ServerBroadcast.Deregister(this, OnServerBroadcast);

                CommunicationManager.Instance.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
