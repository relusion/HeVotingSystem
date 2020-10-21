using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoteMachineWeb.Models
{
    public class VoteModel
    {
        public ulong IsBiden { get; set; }
        public ulong IsTrump { get; set; }

        public string DisplayName { get; set; }

    }
}
