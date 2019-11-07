using MvvmCross.Base;
using MvvmCross.Plugin.Location;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Location.Core
{
    public class LocationService : ILocationService
    {
        private readonly object _lock = new object();

        private static readonly TimeSpan _timeOut = TimeSpan.FromSeconds(20);

        private TaskCompletionSource<MvxCoordinates> _taskSource;

        private readonly IMvxLocationWatcher _locationWatcher;
        private readonly IMvxMainThreadAsyncDispatcher _dispatcher;

        public Task<MvxCoordinates> GetCurrentLocation()
        {
            Task<MvxCoordinates> task;
            bool startWatcher = false;

            lock (_lock)
            {
                if (_taskSource == null)
                {
                    _taskSource = new TaskCompletionSource<MvxCoordinates>();
                    startWatcher = true;
                }
                task = _taskSource.Task;
            }

            if (startWatcher)
            {
                StartWatcher();
            }
            return task;
        }

        public MvxCoordinates GetLastSeenLocation()
        {
            var location = _locationWatcher.LastSeenLocation;
            if (location == null)
            {
                return new MvxCoordinates();
            }
            return new MvxCoordinates()
            {
                Latitude = location.Coordinates.Latitude,
                Longitude = location.Coordinates.Longitude
            };
        }

        public LocationService(IMvxLocationWatcher locationWatcher, IMvxMainThreadAsyncDispatcher dispatcher)
        {
            this._locationWatcher = locationWatcher;
            this._dispatcher = dispatcher;

            this._locationWatcher.OnPermissionChanged += PermissionChanged;
        }

        private void StartWatcher()
        {
            Console.WriteLine("StartWatcher");

            _dispatcher.ExecuteOnMainThreadAsync(() =>
            {
                if (_locationWatcher.Started)
                {
                    Console.WriteLine("Watcher already started!");
                    return;
                }

                _locationWatcher.Start(new MvxLocationOptions() { Accuracy = MvxLocationAccuracy.Coarse },
                    location =>
                    {
                        _locationWatcher.Stop();

                        OnSuccess(new MvxCoordinates()
                        {
                            Latitude = location.Coordinates.Latitude,
                            Longitude = location.Coordinates.Longitude
                        });
                    },
                    error =>
                    {
                        _locationWatcher.Stop();
                        Console.WriteLine(error.Code);
                        OnError(new Exception(error.Code.ToString()));
                    });

                Task.Delay(_timeOut).ContinueWith(_ => OnTimeout());
            });
        }

        private void OnTimeout()
        {
            Console.WriteLine("Timeout");
            TaskCompletionSource<MvxCoordinates> source;
            lock (_lock)
            {
                source = _taskSource;
                _taskSource = null;
            }
            source?.SetException(new Exception(MvxLocationErrorCode.Timeout.ToString()));

            if (_locationWatcher.Started)
            {
                _locationWatcher.Stop();
            }
        }

        private void OnSuccess(MvxCoordinates coordinates)
        {
            Console.WriteLine("Updated: {0}, {1}", coordinates.Longitude.ToString(), coordinates.Latitude.ToString());
            TaskCompletionSource<MvxCoordinates> source;
            lock (_lock)
            {
                source = _taskSource;
                _taskSource = null;
            }
            source?.SetResult(coordinates);
        }

        private void OnError(Exception error)
        {
            Console.WriteLine("Error: {0}", error.Message);
            TaskCompletionSource<MvxCoordinates> source;
            lock (_lock)
            {
                source = _taskSource;
                _taskSource = null;
            }
            source?.SetException(error);
        }

        private void PermissionChanged(object sender, MvxValueEventArgs<MvxLocationPermission> e)
        {
            Console.WriteLine("Permission changed: " + e.Value.ToString());
            if (e.Value != MvxLocationPermission.Denied)
            {
                return;
            }

            TaskCompletionSource<MvxCoordinates> source;
            lock (_lock)
            {
                source = _taskSource;
                _taskSource = null;
            }
            source?.SetException(new Exception(MvxLocationErrorCode.PermissionDenied.ToString()));
        }
    }
}
