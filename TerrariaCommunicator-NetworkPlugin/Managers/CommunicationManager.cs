using Communicator.Interfaces;
using Communicator.Net;
using System;
using TerrariaCommunicator_NetworkPlugin.Packets;

namespace TerrariaCommunicator_NetworkPlugin.Managers
{
    public class CommunicationManager
    {
        public event Action<DiscordMessagePacket.Content> OnMessageReceivedEvent;

        private static CommunicationManager _instance = new CommunicationManager();
        public static CommunicationManager Instance
        {
            get
            {
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        public const string kConfigFilePath = "./tshock/TerrariaCommunicatorConfig.json";
        
        public Action<string> LogAction { get; set; }

        private GameserverClient _client;

        public void Initialize()
        {
            // TODO: remove
            LogAction = (s) => { Console.WriteLine(s); };

            var comPlugin = new ComPlugin();

            var conf = GameserverClient.GSConfig.LoadFromFile(kConfigFilePath);

            PacketSerializer packetSerializer = new PacketSerializer();

            comPlugin.Register(packetSerializer);

            _client = GameserverClient.Create(conf, comPlugin.GameIdentification, packetSerializer, LogActionMethod);

            if(_client != null)
            {
                _client.PacketReceivedEvent += _client_PacketReceivedEvent;
            }
            
            GameserverClient.GSConfig.SaveToFile(kConfigFilePath, conf);
        }

        private void _client_PacketReceivedEvent(object sender, IPacket e)
        {
            switch (e)
            {
                case DiscordMessagePacket dmp:
                    OnMessageReceivedEvent?.Invoke(dmp.PacketData);
                    break;
            }
        }

        private void LogActionMethod(string msg)
        {
            LogAction?.Invoke(msg);
        }

        public void Dispose()
        {
            _client?.Disconnect();
            Instance = null;
        }

        public void SendPacket(IPacket packet)
        {
            _client?.SendPacket(packet);
        }
    }
}
