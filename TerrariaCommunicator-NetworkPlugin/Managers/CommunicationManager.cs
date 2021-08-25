using Communicator.Interfaces;
using Communicator.Net;
using System;
using TerrariaCommunicator_NetworkPlugin.Packets;

namespace TerrariaCommunicator_NetworkPlugin.Managers
{
    public class CommunicationManager
    {
        private static CommunicationManager _instance;
        public static CommunicationManager Instance
        {
            get
            {
                if (_instance == null) _instance = new CommunicationManager();
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

            GameserverClient.GSConfig.SaveToFile(kConfigFilePath, conf);

            PacketSerializer packetSerializer = new PacketSerializer();

            comPlugin.Register(packetSerializer);

            _client = GameserverClient.Create(conf, comPlugin.GameIdentification, packetSerializer, LogActionMethod);
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
            _client.SendPacket(packet);
        }
    }
}
