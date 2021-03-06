using Microsoft.Xna.Framework;
using System;
using TerrariaCommunicator_NetworkPlugin.Managers;
using TerrariaCommunicator_NetworkPlugin.Models;
using TerrariaCommunicator_NetworkPlugin.Packets;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Utils = TShockAPI.Utils;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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

        public const string kPermission = "tcom.reload";

        public override void Initialize()
        {
            CommunicationManager.Instance.OnMessageReceivedEvent += Instance_OnMessageReceivedEvent;
            CommunicationManager.Instance.OnDisconnectedEvent += Instance_OnDisconnectedEvent;
            CommunicationManager.Instance.Initialize();

            var cmd = new Command(kPermission, ReconnectCommand, "reconnect-coms");
            cmd.AllowServer = true;
            Commands.ChatCommands.Add(cmd);

            cmd = new Command(kPermission, DisconnectCommand, "disconnect-coms");
            cmd.AllowServer = true;
            Commands.ChatCommands.Add(cmd);

            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.ServerBroadcast.Register(this, OnServerBroadcast);
        }


        private void Instance_OnDisconnectedEvent()
        {
            Console.WriteLine("Disconnected from discord!");
        }

        private void ReconnectCommand(CommandArgs args)
        {
            if (args.Player == TSPlayer.Server || args.Player.HasPermission(kPermission))
            {
                Console.WriteLine("Trying to reconnect ...");
                CommunicationManager.Instance.TryConnect();
            }
           
        }

        private void DisconnectCommand(CommandArgs args)
        {
            if (args.Player == TSPlayer.Server || args.Player.HasPermission(kPermission))
            {
                Console.WriteLine("Trying to disconnect ...");
                CommunicationManager.Instance.TryDisconnect();
            }
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

                var str = args.Message.ToString().Split(new char[] { ':' }, 2);
                if (str.Length > 1 && Ignore.Player.Name == str[0] && Ignore.Text == str[1].Substring(1)) return;

                Console.WriteLine($"Broadcast: {args.Message}");

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

        private IgnoreThis Ignore { get; set; } = new IgnoreThis();

        public Regex ItemTagRegex { get; } = new Regex(@"\[i(\/[ps]\d+)?\:\d+\:?\]");

        void OnServerChat(ServerChatEventArgs args)
        {
            if (args != null)
            {
                TSPlayer ply = TShock.Players[args.Who];
                if (ply != null)
                {
                    if (string.IsNullOrWhiteSpace(args.Text)) return;

                    if (args.Text.Length > 3700) return;

                    if (args.Text.StartsWith("/"))
                    {
                        // Maybe add command packet, idk
                        return;
                    }

                    var matches = ItemTagRegex.Matches(args.Text);

                    var text = args.Text;

                    var listOfUniqueItemTags = new List<string>();
                    foreach (Match match in matches)
                    {
                        if (!listOfUniqueItemTags.Contains(match.Value))
                            listOfUniqueItemTags.Add(match.Value);
                    }

                    foreach (string itemTag in listOfUniqueItemTags)
                    {
                        try
                        {
                            var item = TShock.Utils.GetItemFromTag(itemTag);

                            if (item == null) continue;

                            string itemTagReplacementString = $"[{item.Name}";

                            if (item.stack > 1)
                            {
                                itemTagReplacementString += $" ({item.stack})";
                            }

                            itemTagReplacementString += "]";

                            text = text.Replace(itemTag, itemTagReplacementString);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to parse item tag \"{itemTag}\" - {ex}: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"Chat: {text}");

                    CommunicationManager.Instance?.SendPacket(new ChatMessagePacket() {
                        PacketData = new ChatMessagePacket.Content
                        {
                            Message = text,
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
