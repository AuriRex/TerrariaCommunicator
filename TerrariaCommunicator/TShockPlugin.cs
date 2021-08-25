#if TSHOCK
extern alias TShockAssemblies;

using TShockAssemblies::Microsoft.Xna.Framework;
using System;
using TerrariaCommunicator_NetworkPlugin.Managers;
using TerrariaCommunicator_NetworkPlugin.Models;
using TerrariaCommunicator_NetworkPlugin.Packets;
using TShockAssemblies::Terraria;
using TShockAssemblies::TerrariaApi.Server;
using TShockAssemblies::TShockAPI;
using Utils = TShockAssemblies::TShockAPI.Utils;

namespace TerrariaCommunicator
{
    public class TShockPlugin : TerrariaPlugin
    {
        public override string Name => "Communicator";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "AuriRex";
        public override string Description => "Description here TODO";

        public TShockPlugin(Main game) : base(game)
        {
            
        }

        public override void Initialize()
        {
            CommunicationManager.Instance.Initialize();

            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.ServerBroadcast.Register(this, OnServerBroadcast);
        }

        private void OnServerBroadcast(ServerBroadcastEventArgs args)
        {
            if (args != null)
            {
                if (args.Message == null) return;
                if (args.Color == null) return;

                CommunicationManager.Instance.SendPacket(new BroadcastMessagePacket()
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
            //AsynchronousSocketSender.Send("GamePostInitialize: true");
            // TODO
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

        public string getItemName(int id)
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

        public string getPrefixName(int id)
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
                    Console.WriteLine($"ServerJoin: {TShock.Players[args.Who].Name}");
                    CommunicationManager.Instance.SendPacket(new PlayerConnectionPacket()
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
                    CommunicationManager.Instance.SendPacket(new PlayerConnectionPacket()
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

        void OnServerChat(ServerChatEventArgs args)
        {
            if (args != null)
            {
                TSPlayer ply = TShock.Players[args.Who];
                if (ply != null)
                {
                    CommunicationManager.Instance.SendPacket(new ChatMessagePacket() {
                        PacketData = new ChatMessagePacket.Content
                        {
                            Message = args.Text,
                            PlayerInfo = CreatePlayerInfo(ply)
                        }
                    });
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
#endif
