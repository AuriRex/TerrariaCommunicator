using Communicator.Interfaces;
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
        public IDiscordInterface DiscordInterface { get; set; }
        public Client Client { get; set; }

        public void OnPacketReceived(IPacket packet)
        {
            switch(packet)
            {
                case BroadcastMessagePacket bmp:
                    var col = bmp.PacketData.Color;
                    DiscordInterface.SendEmbedMessage($"**{bmp.PacketData.Message}**", col.R, col.G, col.B);
                    break;
                case ChatMessagePacket cmp:
                    var playerName = FilterDiscordCodeblock(cmp.PacketData.PlayerInfo.Name);
                    var message = FilterDiscordCodeblock(cmp.PacketData.Message);
                    DiscordInterface.SendMessage($"**{playerName}** > `{message}`");
                    break;
            }
        }

        private string FilterDiscordCodeblock(string message)
        {
            return message.Replace("´", "");
        }

        public void OnDiscordMessageReceived(string message, string username, string discriminator)
        {
            Client.SendPacket(new DiscordMessagePacket() {
                PacketData = new DiscordMessagePacket.Content
                {
                    Discriminator = discriminator,
                    Message = message,
                    Username = username
                }
            });
        }

        public void Register(PacketSerializer packetSerializer)
        {
            packetSerializer.RegisterPacket<BroadcastMessagePacket>();
            packetSerializer.RegisterPacket<ChatMessagePacket>();
            packetSerializer.RegisterPacket<PlayerConnectionPacket>();
            packetSerializer.RegisterPacket<DiscordMessagePacket>();
        }
    }
}