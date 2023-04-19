using AbacusViewer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
//using AbacusViewer.Utility;

namespace AbacusViewer.Controllers
{
    public partial class HomeController
    {
        /*[HttpPost]
        //[GZipOrDeflate]
        public ActionResult Selections(int current, int rowCount, Dictionary<string, string> sort, string searchPhrase, string filter)
        {
            var raw = GetRawSelections().ToList();

            //if (Session["AbacusJointSelection"] != null)
            //{
            //    var previousSelections = (AbacusJointSelection)Session["AbacusJointSelection"];
            //    for (var i = 0; i < raw.Count(); i++)
            //    {
            //        if (previousSelections.Selections.Select(s => s.SelectionId).ToList().Contains(raw[i].SelectionId))
            //        {
            //            raw[i].Commands = true;
            //        }
            //    }
            //}

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
                var markets = filter.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

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
        }*/

        [HttpGet]
        public ActionResult UpdateJointSelection(int forMarketTypeId, int forSelectionId, bool include)
        {
            long eventId = 12345;// (long)Session["CurrentEventId"];
            var jointSelection = //(Session["AbacusJointSelection"] != null) ? (AbacusJointSelection)Session["AbacusJointSelection"] : 
                new AbacusJointSelection();

            // add / remove selection


            //Latest Selections and their Outcomes
            var latestSelections = _eventCollector.AbacusEvents[eventId].Selections.SingleOrDefault(x => x.MarketTypeId == forMarketTypeId && x.SelectionId == forSelectionId);

            //Current Joint Selections
            var abacusJointSelections = jointSelection.Selections.SingleOrDefault(x => x.MarketTypeId == forMarketTypeId && x.SelectionId == forSelectionId);

            //If unticked, remove
            if (abacusJointSelections != null && include == false)
                jointSelection.Selections.Remove(abacusJointSelections);

            //if ticked, add
            if (abacusJointSelections == null & include == true)
            {
                jointSelection.Selections.Add(latestSelections);
            }

            for (int y = 0; y < jointSelection.Selections.Count; y++ )
            {
                //Checks if selctions no longer exists in latest message consumed and removed
                if (_eventCollector.AbacusEvents[eventId].Selections.All(x => x.SelectionId != jointSelection.Selections[y].SelectionId))
                {
                    jointSelection.Selections.Remove(jointSelection.Selections[y]);
                }

                //Updates each Joint Selection to take their latest state from the latest message consumed
                else
                {
                    jointSelection.Selections[y] = _eventCollector.AbacusEvents[eventId].Selections.SingleOrDefault(x =>
                        x.MarketTypeId == jointSelection.Selections[y].MarketTypeId
                        && x.SelectionId == jointSelection.Selections[y].SelectionId);
                }
            }

            //Session["AbacusJointSelection"] = jointSelection;

            //return Json(jointSelection, JsonRequestBehavior.AllowGet);
            return null;
        }

        private IEnumerable<AbacusSelection> GetRawSelections()
        {
            long currentEventId = 12345l;
            //long.TryParse(Session["CurrentEventId"]?.ToString() ?? "0", out var currentEventId);

            //if (!_eventCollector.AbacusEvents.ContainsKey(currentEventId)) return new List<AbacusSelection>();

            //var url = ConfigurationManager.AppSettings["APIEMSBaseUrl"];


            //var ems = System.Web.HttpRuntime.Cache[$"ems_{currentEventId}"] as EventHierarchy;

            //if (ems == null)
            //{
                //var ems = _emsProvider.GetEventMarketSelections(currentEventId, url);
                /*if (ems != null)
                {
                    System.Web.HttpRuntime.Cache[$"ems_{currentEventId}"] = ems;
                }*/
            //}

            var selections = _eventCollector.AbacusEvents[currentEventId].Selections.ToList();


            /*if (ems?.Markets == null)
                return selections;

            var emsFlatSelections = ems.Markets.SelectMany(p => p.Selections);*/

            Dictionary<long, Selection> emsSelectionDictionary = null;// emsFlatSelections.ToDictionary(key => key.PaddyPowerId, val => val);

            foreach (var sel in selections.AsParallel())
            {
                emsSelectionDictionary.TryGetValue(sel.SelectionId, out var abacusSelection);

                if (abacusSelection != null)
                {
                    sel.SelectionIdentifier = abacusSelection.Name;
                }

            }

            return selections;
        }
    }
}