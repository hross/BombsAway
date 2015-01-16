
using System.Web.Mvc;
using BombsAway.Common.Statistics;

namespace BombsAway.Web.Controllers
{
    public class GameController : ContextControllerBase
    {
        private GameService _gameService;
        public GameController()
            : base()
        {
            _gameService = new GameService(this.Context);
        }

        public ActionResult Index()
        {

            return View(_gameService.QueryAll());
        }
    }
}