using BombsAway.Common.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BombsAway.Web.Controllers
{
    public class ProfileController : ContextControllerBase
    {
        private ProfileService _profileService;
        private PlayerColorService _playerColorService;

        public ProfileController()
        {
            _profileService = new ProfileService(this.Context);
            _playerColorService = new PlayerColorService(this.Context);
        }

        public ActionResult Index()
        {
            return View(_profileService.QueryAll());
        }

        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id.HasValue && id != 0)
            {
                return View(WrapProfile(_profileService.First(p => p.Id == id.Value)));
            }
            else
            {
                return View(WrapProfile(new Profile()));
            }
        }

        [HttpPost]
        public ActionResult Edit(ProfileModel model)
        {
            if (model.Profile.Id == 0)
            {
                var p = _profileService.Add(model.Profile);
            }
            else
            {
                var p = _profileService.Update(model.Profile);
            }

            return RedirectToAction("Index");
        }

        private ProfileModel WrapProfile(Profile profile)
        {
            return new ProfileModel(profile, _playerColorService.QueryAll());
        }
    }
}