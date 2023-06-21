using Microsoft.AspNetCore.Mvc;
using AbacusViewer.Models;
using Lightyear.Common.Agglomerator.Contracts.Proto.PesV3;
using Newtonsoft.Json;
using KestrelTest.Core;

namespace AbacusViewer.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly KafkaConsumer _kafkaConsumer;
        private readonly EmsService _emsService;

        public HomeController(KafkaConsumer kafkaConsumer, EmsService emsService)
        {
            _kafkaConsumer = kafkaConsumer;
            _emsService = emsService;
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
            return View(new SelectionFilter
            {
                EventId = eventId,
                Filter = filter
            });
        }

        [HttpPost]
        public ActionResult Selections(long eventId, int current, int rowCount, Dictionary<string, string> sort, string searchPhrase, string filter)
        {
            if (searchPhrase == null) searchPhrase = string.Empty;

            var raw = GetRawSelections(eventId)?.ToList();

            var newFiltered = new List<AbacusSelection>();

            var filtered = raw.Where(x => x.MarketTypeId.ToString().IndexOf(searchPhrase, StringComparison.OrdinalIgnoreCase) > -1
                                          || x.MarketName.ToString().IndexOf(searchPhrase, StringComparison.OrdinalIgnoreCase) > -1
                                          || x.SelectionId.ToString().IndexOf(searchPhrase, StringComparison.OrdinalIgnoreCase) > -1);

            if (sort != null)
            {
                if (sort.ContainsKey("market_type_id")) filtered = (sort["market_type_id"] != "desc") ? filtered.OrderBy(x => x.MarketTypeId) : filtered.OrderByDescending(x => x.MarketTypeId);
                else if (sort.ContainsKey("market_name")) filtered = (sort["market_name"] != "desc") ? filtered.OrderBy(x => x.MarketName) : filtered.OrderByDescending(x => x.MarketName);
                else if (sort.ContainsKey("selection_id")) filtered = (sort["selection_id"] != "desc") ? filtered.OrderBy(x => x.SelectionId) : filtered.OrderByDescending(x => x.SelectionId);
                else filtered = filtered.OrderBy(x => x.MarketName);
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var markets = filter.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var rec in filtered)
                {
                    var isInMarket = false;
                    foreach (var market in markets)
                    {
                        if (rec.MarketName.ToLower().Contains(market.ToLower()))
                        {
                            isInMarket = true;
                        }
                    }

                    if (isInMarket)
                    {
                        newFiltered.Add(rec);
                    }
                }
            }
            else
            {
                newFiltered = filtered.ToList();
            }

            var paged = newFiltered.Skip((current - 1) * rowCount).Take(rowCount > -1 ? rowCount : int.MaxValue);

            string json = "{\"current\": " + current + ", \"rowCount\": " + rowCount + ",\"rows\": " + JsonConvert.SerializeObject(paged.ToList()) + ", \"total\": " + filtered.Count() + "}";
            return new ContentResult { Content = json, ContentType = "application/json" };
        }

        private IEnumerable<AbacusSelection> GetRawSelections(long currentEventId)
        {
            PublisherMessage message = _kafkaConsumer.GetMostRecentMessageForEventId(currentEventId);

            AbacusEventFromPublisherMessage evt = new AbacusEventFromPublisherMessage(message);

            var emsSelectionLookup = message != null ? GetEmsSelectionLookup(message.EventId) : new Dictionary<long, string>();
            return evt?.GetSelections(emsSelectionLookup);
        }

        private Dictionary<long, string> GetEmsSelectionLookup(long eventId)
        {
            EventHierarchy eh = _emsService.GetEventMarketSelections(eventId);
            var emsSelectionLookup = _emsService.GetEventMarketSelections(eventId)?.Markets
                .SelectMany(m => m.Selections)
                .ToDictionary(s => s.PaddyPowerId, s => s.Name) ?? new Dictionary<long, string>();

            return emsSelectionLookup;
        }
    }
}