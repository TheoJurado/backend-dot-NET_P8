using GpsUtil.Location;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TourGuide.Utilities;
using Xunit.Abstractions;

namespace TourGuideTest
{
    public class PerformanceTest : IClassFixture<DependencyFixture>
    {
        /*
         * Note on performance improvements:
         * 
         * The number of generated users for high-volume tests can be easily adjusted using this method:
         * 
         *_fixture.Initialize(100000); (for example)
         * 
         * 
         * These tests can be modified to fit new solutions, as long as the performance metrics at the end of the tests remain consistent.
         * 
         * These are the performance metrics we aim to achieve:
         * 
         * highVolumeTrackLocation: 100,000 users within 15 minutes:
         * Assert.True(TimeSpan.FromMinutes(15).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
         *
         * highVolumeGetRewards: 100,000 users within 20 minutes:
         * Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        */

        private readonly DependencyFixture _fixture;

        private readonly ITestOutputHelper _output;

        public PerformanceTest(DependencyFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]//(Skip = ("Delete Skip when you want to pass the test"))
        public void HighVolumeTrackLocation()
        {
            //On peut ici augmenter le nombre d'utilisateurs pour tester les performances
            _fixture.Initialize(1000);

            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            /*
            foreach (var user in allUsers)
            {
                _fixture.TourGuideService.TrackUserLocation(user);
            }/**/
            /*
            Parallel.ForEach(allUsers, user =>
            {
                _fixture.TourGuideService.TrackUserLocation(user);
            });/**/
            var partitioner = Partitioner.Create(allUsers, true);
            Parallel.ForEach(partitioner, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, user =>
            {
                _fixture.TourGuideService.TrackUserLocation(user);
            });
            stopWatch.Stop();
            _fixture.TourGuideService.Tracker.StopTracking();

            _output.WriteLine($"highVolumeTrackLocation: Time Elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");

            Assert.True(TimeSpan.FromMinutes(15).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }

        [Fact]//(Skip = ("Delete Skip when you want to pass the test"))
        public async Task HighVolumeGetRewards()
        {
            //On peut ici augmenter le nombre d'utilisateurs pour tester les performances
            _fixture.Initialize(100);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            List<Attraction> attractionsCache = await _fixture.GpsUtil.GetAttractionsAsync();//new
            //Attraction attraction = _fixture.GpsUtil.GetAttractions()[0];
            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();

            allUsers.ForEach(u => u.AddToVisitedLocations(new VisitedLocation(u.UserId, attractionsCache[0], DateTime.Now)));
            /*var tasks = allUsers.Select(user =>
                Task.Run(() =>
                {
                    user.AddToVisitedLocations(new VisitedLocation(user.UserId, attractionsCache[0], DateTime.Now));
                })
            );
            await Task.WhenAll(tasks);/**/

            var rewardTasks = allUsers.Select(user => Task.Run(() => _fixture.RewardsService.CalculateRewards(user, attractionsCache)));
            await Task.WhenAll(rewardTasks);

            /*foreach (var user in allUsers) old
            {
                Assert.True(user.UserRewards.Count > 0);
            }/**/
            Assert.All(allUsers, u => Assert.True(u.UserRewards.Count > 0));

            stopWatch.Stop();
            _fixture.TourGuideService.Tracker.StopTracking();

            _output.WriteLine($"highVolumeGetRewards: Time Elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");
            Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }
        /*
        [Fact]
        public void TestFullCharge()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var task1 = Task.Run(HighVolumeTrackLocation);
            var task2 = Task.Run(HighVolumeGetRewards);
            Task.WaitAll(task1, task2);
            stopWatch.Stop();
            Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }/**/
    }
}
