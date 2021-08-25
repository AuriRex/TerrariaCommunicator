using Communicator.Packets;
using TerrariaCommunicator_NetworkPlugin.Models;

namespace TerrariaCommunicator_NetworkPlugin.Packets
{
    public class BroadcastMessagePacket : BasePacket<BroadcastMessagePacket.Content>
    {
        public override Content PacketData { get; set; }

        public struct Content
        {
            public string Message { get; set; }
            public ColorStruct Color { get; set; }
        }
    }
}
