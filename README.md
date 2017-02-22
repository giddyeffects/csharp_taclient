#
#	A simple C# class to connect to and consume the TripAdvisor API by Gideon Nyaga
#	TripAdvisor content API documentation: https://developer-tripadvisor.com/content-api/documentation/
#
1. Get a TripAdvisor API Key from contentapi@tripadvisor.com
2. Download the class
3. Sample usage, when you have a tripadvisor location id
	NOTE: add the necessary usings, add your error handlers :-)
	
        public async static Task<dynamic> getReviews(int tripadvisor_location_id)
        {
            var client = new TripAdvisorClient("<your API Key>", "http://maps.googleapis.com/maps/api/geocode/json?address=");
            var summary_fields = new string[] { "ranking_data", "num_reviews", "rating_image_url" };
            //TripAdvisor location documentation https://developer-tripadvisor.com/content-api/documentation/location/
            var reviews = await client.getHotelInfo(tripadvisor_location_id,summary_fields);

            if (reviews != null)
            {
                dynamic jObj = (JObject)JsonConvert.DeserializeObject(reviews);
                return jObj;
            }
            else
            {
                return null;
            }
        }
		
Feel free to download and use in your projects.
Note: Test before use. No guarantees given :-)

Thanks,
Gideon Nyaga
