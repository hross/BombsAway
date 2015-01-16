using BombsAway.Common.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BombsAway.Web.Controllers
{
    public abstract class ContextControllerBase : Controller
    {
        private DatabaseContext _context;

        protected DatabaseContext Context
        {
            get
            {
                if (null == _context)
                {
                    _context = AppConfig.DbContext(conn => conn); //new ProfiledDbConnection(conn, MiniProfiler.Current)
                }

                return _context;
            }
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (null != _context)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }
    }
}