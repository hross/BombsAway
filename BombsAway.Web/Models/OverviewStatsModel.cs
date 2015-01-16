using BombsAway.Common.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BombsAway.Web
{
    public class OverviewStatsModel
    {
        public List<Profile> Profiles { get; private set; }

        public List<GamePlayerProfile> GamePlayers { get; private set; }

        public OverviewStatsModel(List<Profile> profiles, List<GamePlayerProfile> gamePlayers)
        {
            this.Profiles = profiles;
            this.GamePlayers = gamePlayers;
        }
    }
}