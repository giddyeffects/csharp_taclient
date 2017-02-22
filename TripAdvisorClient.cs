using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Public.Models
{
    /// <summary>
    /// C# class to access the TripAdvisor API and get hotel information
    /// by Gideon Nyaga, 2016
    /// An API key is required
    /// </summary>
    public class TripAdvisorClient
    {
        private const string URL = "http://api.tripadvisor.com/api/partner/2.0/";
        private string taKey;
        private string geocodeURL;
        
        public TripAdvisorClient(string key, string geocodeurl)
        {
            taKey = key;
            geocodeURL = geocodeurl;
        }

        protected async Task<string> callMethod(string method, Dictionary<string, string> taParams=null, string mapper="") 
        {
            var jsonString = "{}";
            HttpClient client = new HttpClient();
            var newParams = new Dictionary<string, string>();
            newParams.Add("key", taKey+mapper);
            if (taParams != null)
            {
                foreach (var par in taParams)
                {
                    if (par.Value != null) newParams.Add(par.Key, par.Value);
                }
            }
            client.BaseAddress = new Uri(QueryHelpers.AddQueryString(URL + method, newParams));
            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync("");

            if (response.IsSuccessStatusCode) { jsonString = await response.Content.ReadAsStringAsync(); }
            return jsonString;
        }

        public async Task<string> getHotelInfo(int locationID = 0, string[] fields = null,string hotelName=null,string address=null)
        {
            if (locationID == 0)
            {
                locationID = await getLocationId(hotelName, address);
            }
            var pars = new Dictionary<string, string>();//parameters
            pars.Add("lang", "en");
            var fullHotelInfo = await callMethod("location/" + locationID, pars);
            if (fullHotelInfo != null)
            {
                if (fields != null)
                {
                    return getFields(fullHotelInfo, fields);
                }
                else
                {
                    return fullHotelInfo;
                }
            }
            else
            {
                return null;
            }
        }

        //@TODO test this
        public async Task<dynamic> getLocationId(string hotelName=null, string address =null)
        {
            var coords = await getCoords(hotelName, address);
            if (coords == null)
            {
                throw new Exception("Cannot get the coordinates");
            }
            var pars = new Dictionary<string, string>();
            pars.Add("category", "hotels"); pars.Add("q", hotelName);
            var shortHotelInfo = await callMethod("location_mapper/" + Uri.EscapeDataString(coords), pars, "-mapper");
            dynamic obj = (JObject)JsonConvert.DeserializeObject(shortHotelInfo);

            return obj.data[0].location_id;
        }

        public static string getFields(string fullHotelInfo, string[] fields)
        {
            var hotelinfo = new Dictionary<string, dynamic>();

            dynamic obj = (JObject)JsonConvert.DeserializeObject(fullHotelInfo);
            foreach (var field in fields)
            {
                hotelinfo.Add(field,obj[field]);
            }

            return JsonConvert.SerializeObject(hotelinfo);
        }
        //@TODO test
        public async Task<string> getCoords(string hotelName = null, string address = null)
        {
            string lat=""; string longi=""; string jsonString = "";
            if (hotelName == null || address == null)
            {
                throw new ArgumentException("Hotel name or address cannot be null");
            }
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(geocodeURL+Uri.EscapeDataString(hotelName)+","+Uri.EscapeDataString(address)+"&sensor=false");
            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync("");

            if (response.IsSuccessStatusCode)
            {
                jsonString = await response.Content.ReadAsStringAsync();
            }
            dynamic obj = (JObject)JsonConvert.DeserializeObject(jsonString);
            lat = obj.results[0].geometry.location.lat;
            longi = obj.results[0].geometry.location.lng;
            return (lat == null && longi == null)?null:lat + "," + longi;
        }
    }
}
