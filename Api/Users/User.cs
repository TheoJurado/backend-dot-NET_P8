using GpsUtil.Location;
using System.Collections.Concurrent;
using TripPricer;

namespace TourGuide.Users;

public class User
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public DateTime LatestLocationTimestamp { get; set; }
    public List<VisitedLocation> VisitedLocations { get; } = new List<VisitedLocation>();
    public ConcurrentDictionary<string, UserReward> UserRewards { get; } = new ConcurrentDictionary<string, UserReward>();
    //public List<UserReward> UserRewards { get; } = new List<UserReward>();
    public UserPreferences UserPreferences { get; set; } = new UserPreferences();
    public List<Provider> TripDeals { get; set; } = new List<Provider>();

    public User(Guid userId, string userName, string phoneNumber, string emailAddress)
    {
        UserId = userId;
        UserName = userName;
        PhoneNumber = phoneNumber;
        EmailAddress = emailAddress;
    }

    public void AddToVisitedLocations(VisitedLocation visitedLocation)
    {
        VisitedLocations.Add(visitedLocation);
    }

    public void ClearVisitedLocations()
    {
        VisitedLocations.Clear();
    }

    public void AddUserReward(UserReward userReward)
    {
        //if (!UserRewards.Exists(r => r.Attraction.AttractionName == userReward.Attraction.AttractionName))
        {
            //UserRewards.Add(userReward);
            UserRewards.TryAdd(userReward.Attraction.AttractionName, userReward);
        }
    }

    public VisitedLocation GetLastVisitedLocation()
    {
        return VisitedLocations[^1];
    }
}
