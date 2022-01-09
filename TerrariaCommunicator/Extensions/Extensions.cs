using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaCommunicator_NetworkPlugin.Models;

namespace TerrariaCommunicator.Extensions
{
    public static class Extensions
    {

        public static ColorStruct ToComsColor(this Color color)
        {
            return new ColorStruct
            {
                R = color.R,
                G = color.G,
                B = color.B
            };
        }

        public static PlayerInfo ToComsPlayerInfo(this Player ply)
        {
            return new PlayerInfo
            {
                IsDead = ply.statLife <= 0,
                Name = ply.name,
                UUID = ply.name // shrug
            };
        }

    }
}
