using MvvmCross.Plugin.Location;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Location.Core
{
    public class MainActivityViewModel:MvxViewModel
    {
        private readonly ILocationService locationService;

        public MainActivityViewModel(ILocationService locationService)
        {
            this.locationService = locationService;
        }

        public async override void ViewAppeared()
        {
            base.ViewAppeared();
            var a = await CurrentPosition();
        }

        private async Task<MvxCoordinates> CurrentPosition()
        {
            return await this.locationService.GetCurrentLocation();

        }
    }
}
