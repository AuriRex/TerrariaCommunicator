using Communicator.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaCommunicator_NetworkPlugin.Models;

namespace TerrariaCommunicator_NetworkPlugin.Packets
{
    public class PlayerConnectionPacket : BasePacket<PlayerConnectionPacket.Content>
    {
        public override Content PacketData { get; set; }

        public struct Content
        {
            public bool IsConnecting { get; set; }
            public PlayerInfo PlayerInfo { get; set; }
        }
    }
}
