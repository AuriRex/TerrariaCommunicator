using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaCommunicator_NetworkPlugin.Models
{
    public class PlayerInfo
    {
        public string Name { get; set; }
        public string UUID { get; set; }
        public bool IsDead { get; set; }
    }
}
