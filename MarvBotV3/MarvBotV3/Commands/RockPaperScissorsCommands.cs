using Discord.Commands;
using MarvBotV3.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    public class RockPaperScissorsCommands : ModuleBase<ShardedCommandContext>
    {
        private NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = " " };
        DataAccess da;

        public RockPaperScissorsCommands()
        {
            da = new DataAccess(new DatabaseContext());
        }
    }
}
