using MvvmCross.Plugin.Location;
using System.Threading.Tasks;

namespace Location.Core
{
    public interface ILocationService
    {
        Task<MvxCoordinates> GetCurrentLocation();
        MvxCoordinates GetLastSeenLocation();
    }
}