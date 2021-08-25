extern alias TShockAssemblies; 
using System;
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
            MessageManager.Init();

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

                //AsynchronousSocketSender.Send($"ServerBroadcast: {args.Message}\n{args.Color.ToString()}");
            }

        }

        private void OnGamePostInitialize(EventArgs args)
        {
            //AsynchronousSocketSender.Send("GamePostInitialize: true");
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
                if (TShock.Players[args.Who] != null)
                {
                    Console.WriteLine($"ServerJoin: {TShock.Players[args.Who].Name}");
                    //AsynchronousSocketSender.Send($"ServerJoin: {TShock.Players[args.Who].Name}\n{TShock.Players[args.Who].IP}");
                }
            }
        }

        void OnServerLeave(LeaveEventArgs args)
        {
            if (args != null)
            {
                if (TShock.Players[args.Who] != null)
                {
                    //AsynchronousSocketSender.Send($"ServerLeave: {TShock.Players[args.Who].Name}\n{TShock.Players[args.Who].IP}");
                }
            }
        }

        void OnServerChat(ServerChatEventArgs args)
        {
            if (args != null)
            {
                if (TShock.Players[args.Who] != null)
                {
                    //AsynchronousSocketSender.Send($"ServerChat: {TShock.Players[args.Who].Name}\n{args.Text}");
                }
            }
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

                MessageManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
