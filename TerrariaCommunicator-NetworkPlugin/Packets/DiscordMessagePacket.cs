using Communicator.Packets;

namespace TerrariaCommunicator_NetworkPlugin.Packets
{
    public class DiscordMessagePacket : BasePacket<DiscordMessagePacket.Content>
    {
        public override Content PacketData { get; set; }

        public struct Content
        {
            public string Message { get; set; }
            public string Username { get; set; }
            public string Discriminator { get; set; }
        }
    }
}
