using Communicator.Interfaces;
using Communicator.Net;
using System;
using System.IO;
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

        public const string kTShockConfigFilePath = "./tshock/TerrariaCommunicatorConfig.json";
        public const string kTModLoaderConfigFilePath = "./userdata/TerrariaCommunicatorConfig.json";
        public string ConfigFilePath { get; set; } = kTShockConfigFilePath;

        public Action<string> LogAction { get; set; }

        private GameserverClient _client;
        private ComPlugin _comPluginInstance;

        public void Initialize(bool tModLoader = false)
        {
            if (tModLoader)
                ConfigFilePath = kTModLoaderConfigFilePath;

            TryConnect();
        }

        public void TryConnect()
        {
            if (Connected) return;

            if(_comPluginInstance == null) _comPluginInstance = new ComPlugin();

            GameserverClient.GSConfig config = null;
            try
            {
                if(!Directory.Exists(Path.GetDirectoryName(ConfigFilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath));
                }
                config = GameserverClient.GSConfig.LoadFromFile(ConfigFilePath);

                _ = Task.Run(() => {

                    PacketSerializer packetSerializer = new PacketSerializer();

                    _comPluginInstance.Register(packetSerializer);
                    try
                    {
                        _client = GameserverClient.Create(config, _comPluginInstance.GameIdentification, packetSerializer, LogActionMethod);

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
            }
            catch(ArgumentException)
            {

            }
            finally
            {
                GameserverClient.GSConfig.SaveToFile(ConfigFilePath, config ?? new GameserverClient.GSConfig());
            }
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
