using Communicator.Interfaces;
using Communicator.Net;
using System;
using System.Threading.Tasks;
using TerrariaCommunicator_NetworkPlugin.Packets;

namespace TerrariaCommunicator_NetworkPlugin.Managers
{
    public class CommunicationManager
    {
        public event Action<DiscordMessagePacket.Content> OnMessageReceivedEvent;
        public event Action OnDisconnectedEvent;

        public bool Connected
        {
            get
            {
                return _client != null && _client.Connected;
            }
        }

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
        private ComPlugin _comPluginInstance;

        public void Initialize()
        {
            // TODO: remove
            LogAction = (s) => { Console.WriteLine(s); };

            TryConnect();
        }

        public void TryConnect()
        {
            if (Connected) return;

            if(_comPluginInstance == null) _comPluginInstance = new ComPlugin();

            var conf = GameserverClient.GSConfig.LoadFromFile(kConfigFilePath);

            _ = Task.Run(() => {

                PacketSerializer packetSerializer = new PacketSerializer();

                _comPluginInstance.Register(packetSerializer);
                try
                {
                    _client = GameserverClient.Create(conf, _comPluginInstance.GameIdentification, packetSerializer, LogActionMethod);

                    if (_client != null)
                    {
                        _client.PacketReceivedEvent += _client_PacketReceivedEvent;
                        _client.DisconnectedEvent += _client_DisconnectedEvent;
                        LogAction?.Invoke("Connected!");
                    }
                }
                catch (Exception ex)
                {
                    LogAction?.Invoke($"An error occurred trying to connect: {ex.Message}");
                }
            });

            GameserverClient.GSConfig.SaveToFile(kConfigFilePath, conf);
        }

        private void _client_DisconnectedEvent(Communicator.Net.EventArgs.ClientDisconnectedEventArgs args)
        {
            _client.PacketReceivedEvent -= _client_PacketReceivedEvent;
            _client.DisconnectedEvent -= _client_DisconnectedEvent;

            OnDisconnectedEvent?.Invoke();

            _client = null;
        }

        public void TryDisconnect()
        {
            _client?.Disconnect();
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
