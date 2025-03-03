using GpsUtil.Location;
using System.Collections.Concurrent;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;

namespace TourGuide.Services;

public class RewardsService : IRewardsService
{
    private const double StatuteMilesPerNauticalMile = 1.15077945;
    private readonly int _defaultProximityBuffer = 10;
    private int _proximityBuffer;
    private readonly int _attractionProximityRange = 200;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardCentral _rewardsCentral;
    private static int count = 0;

    private readonly ConcurrentDictionary<(Locations, Locations), double> _distanceCache = new();


    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral =rewardCentral;
        _proximityBuffer = _defaultProximityBuffer;
    }

    public void SetProximityBuffer(int proximityBuffer)
    {
        _proximityBuffer = proximityBuffer;
    }

    public void SetDefaultProximityBuffer()
    {
        _proximityBuffer = _defaultProximityBuffer;
    }

    public async Task CalculateRewards(User user, List<Attraction> attractions = null)
    {
        count++;
        List<VisitedLocation> userLocations = user.VisitedLocations.ToList();
        if (attractions == null)//new
            attractions = await _gpsUtil.GetAttractionsAsync();

        var existingRewards = new HashSet<string>(user.UserRewards.Select(r => r.Attraction.AttractionName));//new

        var tasks = new List<Task>();

        foreach (var visitedLocation in userLocations)
        {
            var nearbyAttractions = attractions//new
                .Where(attraction => NearAttraction(visitedLocation, attraction)) // Première passe sur les distances
                .ToList();

            foreach (var attraction in nearbyAttractions)
            {
                //if (!user.UserRewards.Any(r => r.Attraction.AttractionName == attraction.AttractionName)) old
                if (!existingRewards.Contains(attraction.AttractionName))
                {
                    //if (NearAttraction(visitedLocation, attraction)) old
                    tasks.Add(Task.Run(() =>
                    {
                        user.AddUserReward(new UserReward(visitedLocation, attraction, GetRewardPoints(attraction, user)));
                        existingRewards.Add(attraction.AttractionName);//new
                    }));
                }
            }
        }

        await Task.WhenAll(tasks);
    }

    public bool IsWithinAttractionProximity(Attraction attraction, Locations location, int extendeRange = 0)
    {
        Console.WriteLine(GetDistance(attraction, location));
        return GetDistance(attraction, location) <= (_attractionProximityRange + extendeRange);
    }

    private bool NearAttraction(VisitedLocation visitedLocation, Attraction attraction)
    {
        return GetDistance(attraction, visitedLocation.Location) <= _proximityBuffer;
    }

    private int GetRewardPoints(Attraction attraction, User user)
    {
        return _rewardsCentral.GetAttractionRewardPoints(attraction.AttractionId, user.UserId);
    }

    public double GetDistance(Locations loc1, Locations loc2)
    {
        double lat1 = Math.PI * loc1.Latitude / 180.0;
        double lon1 = Math.PI * loc1.Longitude / 180.0;
        double lat2 = Math.PI * loc2.Latitude / 180.0;
        double lon2 = Math.PI * loc2.Longitude / 180.0;

        double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2)
                                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));

        double nauticalMiles = 60.0 * angle * 180.0 / Math.PI;
        return StatuteMilesPerNauticalMile * nauticalMiles;
    }
}
