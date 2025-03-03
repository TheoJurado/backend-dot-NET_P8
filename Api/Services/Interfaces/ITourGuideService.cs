using GpsUtil.Location;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services.Interfaces
{
    public interface ITourGuideService
    {
        Tracker Tracker { get; }

        void AddUser(User user);
        List<User> GetAllUsers();
        Task<List<Attraction>> GetNearByAttractions(VisitedLocation visitedLocation, int maxAttraction);
        List<Provider> GetTripDeals(User user);
        User GetUser(string userName);
        VisitedLocation GetUserLocation(User user);
        List<UserReward> GetUserRewards(User user);
        VisitedLocation TrackUserLocation(User user);
    }
}