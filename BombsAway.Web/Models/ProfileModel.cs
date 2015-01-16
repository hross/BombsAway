using BombsAway.Common.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BombsAway.Common;

namespace BombsAway.Web
{
    public class ProfileModel
    {
        public ProfileModel()
        {
        }

        public ProfileModel(Profile profile, List<PlayerColor> colors)
        {
            this.Profile = profile;
            this.Colors = colors;
        }

        public Profile Profile { get; set; }
        public List<PlayerColor> Colors { get; set; }

        public List<SelectListItem> ColorList
        {
            get
            {
                var list = new List<SelectListItem>();
                foreach (var color in Colors)
                {
                    var name = color.Name;

                    if (string.IsNullOrEmpty(name))
                    {
                        name = string.Format("(R:{0} G:{1} B:{2})", color.Red, color.Green, color.Blue);
                    }

                    list.Add(new SelectListItem { Text = name, Value = color.Id.ToString() });
                }

                return list;
            }
        }

        public Dictionary<long, string> HexValues()
        {
            var dict = new Dictionary<long, string>();

            foreach (var color in Colors)
            {
                dict.Add(color.Id, color.FindColor().HexString());
            }

            return dict;
        }
    }
}