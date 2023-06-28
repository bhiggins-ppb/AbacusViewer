using System;
using System.Net;
using System.Net.Http;
using AbacusViewer.Models;
using Newtonsoft.Json;

namespace KestrelTest.Core
{
    public class EmsService
    {
        private readonly string _baseUrl;

        public EmsService(EmsServiceConfiguration configuration)
        {
            _baseUrl = configuration.BaseUrl;
        }

        public EventHierarchy GetEventMarketSelections(long eventId)
        {
            try
            {
                var client = new HttpClient();

                var url = $"{_baseUrl}{eventId}";

                var response = client.GetAsync(url).Result;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"{eventId} not found in EMS");
                }

                var data = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<EventHierarchy>(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EMS request {_baseUrl}{eventId} exception {ex.Message}");
                return new EventHierarchy();
            }
        }
    }
}