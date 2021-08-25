using Communicator.Net;
using DiscordBotPluginBase.Interfaces;
using System;
using TerrariaCommunicator_NetworkPlugin.Packets;

namespace TerrariaCommunicator_NetworkPlugin
{
    public class ComPlugin : ICommunicationPlugin
    {
        public string GameIdentification { get; set; } = "terraria";

        public string Description { get; set; } = "Used to communicate with a terraria game server";

        public void Register(PacketSerializer packetSerializer)
        {
            packetSerializer.RegisterPacket<BroadcastMessagePacket>();
        }
    }
}