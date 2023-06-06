using Microsoft.AspNetCore.Mvc;
using AbacusViewer.Services;
using AbacusViewer.Models;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace AbacusViewer.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly EventCollector _eventCollector;
        //private string UserName { get { return User != null ? User.Identity.Name : "N/A"; } }
        //private readonly IEMSServiceProvider _emsProvider;

        public HomeController(EventCollector eventCollector)
                              //  IEMSServiceProvider emsProvider)
        {
            _eventCollector = eventCollector;
            //_emsProvider = emsProvider;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Markets()
        {
            return View();
        }

        public ActionResult Selections(long eventId, string filter)
        {
            /*var currentEventId = HttpContext.Request.Query["eventid"];

            if (HttpContext.Session.Get("CurrentEventId") != null)
            {
                if (currentEventId != HttpContext.Session.GetString("CurrentEventId"))
                {
                    HttpContext.Session.Set("AbacusJointSelection", null);
                }
            }

            HttpContext.Session.SetString("CurrentEventId", currentEventId);*/

            return View(new SelectionFilter
            {
                EventId = eventId,
                Filter = filter
            });
        }

        /*public ActionResult Selections(long eventId, string filter)
        {
            //Int64.TryParse(Request.QueryString["eventid"], out var currentEventId);
            int.TryParse(HttpUtility.ParseQueryString(Request.QueryString.Value).Get("eventid"), out var currentEventId);

            if (HttpContext.Session.Get("CurrentEventId") != null)
            {
                if (currentEventId != BitConverter.ToInt64(HttpContext.Session.Get("CurrentEventId")))
                {
                    HttpContext.Session.Set("AbacusJointSelection", null);
                }
            }

            HttpContext.Session.Set("CurrentEventId", BitConverter.GetBytes(currentEventId));

            return View(new SelectionFilter
            {
                EventId = eventId,
                Filter = filter
            });
        }
    }*/
        /*    private readonly ILogger<HomeController> _logger;

            public HomeController(ILogger<HomeController> logger)
            {
                _logger = logger;
            }

            public IActionResult Index()
            {
                return View();
            }

            public IActionResult Privacy()
            {
                return View();
            }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }*/
        }
    }