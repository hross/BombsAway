using BombsAway.Common.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BombsAway.Web.Controllers
{
    public class HomeController : ContextControllerBase
    {
        private ProfileService _profileService;

        private GamePlayerService _playerService;

        public HomeController()
            : base()
        {
            _profileService = new ProfileService(this.Context);
            _playerService = new GamePlayerService(this.Context);
        }

        public ActionResult Index()
        {
            var profiles =_profileService.QueryAll().OrderByDescending(p => p.Wins).ThenByDescending(p => p.Trophies).ToList();
            var gamePlayers = _playerService.SelectProfiles(gp => gp.CreatedOnUTC > DateTime.UtcNow.AddDays(-4)).OrderByDescending(p => p.Wins).ThenByDescending(p => p.Trophies).ToList();

            return View(new OverviewStatsModel(profiles, gamePlayers));
        }
    }
}