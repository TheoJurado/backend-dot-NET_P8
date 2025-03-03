using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPricer.Helpers;

namespace TripPricer;

public class TripPricer
{
    public List<Provider> GetPrice(string apiKey, Guid attractionId, int adults, int children, int nightsStay, int rewardsPoints)
    {
        List<Provider> providers = new List<Provider>();
        //HashSet<string> providersUsed = new HashSet<string>();

        // Sleep to simulate some latency
        Thread.Sleep(ThreadLocalRandom.Current.Next(1, 50));

        //get all provider and randomize
        var providerNames = GetAllProviderNames().OrderBy(_ => ThreadLocalRandom.Current.Next()).Take(10).ToList();

        for (int i = 0; i < providerNames.Count; i++)//i < 10 //i < 5
        {
            int multiple = ThreadLocalRandom.Current.Next(100, 700);
            double childrenDiscount = children / 3.0;
            double price = multiple * adults + multiple * childrenDiscount * nightsStay + 0.99 - rewardsPoints;

            if (price < 0.0)
            {
                price = 0.0;
            }

            //string provider;
            /*do
            {
                provider = GetProviderName(apiKey, adults);
            } while (providersUsed.Contains(provider));*/

            //providersUsed.Add(provider);
            providers.Add(new Provider(attractionId, providerNames[i], price));//(attractionId, provider, price)
        }
        return providers;
    }

    public string GetProviderName(string apiKey, int adults)
    {
        int multiple = ThreadLocalRandom.Current.Next(1, 11);//Change from Next(1, 10);

        return multiple switch
        {
            1 => "Holiday Travels",
            2 => "Enterprize Ventures Limited",
            3 => "Sunny Days",
            4 => "FlyAway Trips",
            5 => "United Partners Vacations",
            6 => "Dream Trips",
            7 => "Live Free",
            8 => "Dancing Waves Cruselines and Partners",
            9 => "AdventureCo",
            _ => "Cure-Your-Blues",
        };        
    }

    private static List<string> GetAllProviderNames()
    {
        return new List<string>
        {
            "Holiday Travels",
            "Enterprize Ventures Limited",
            "Sunny Days",
            "FlyAway Trips",
            "United Partners Vacations",
            "Dream Trips",
            "Live Free",
            "Dancing Waves Cruselines and Partners",
            "AdventureCo",
            "Cure-Your-Blues"
        };
    }
}
