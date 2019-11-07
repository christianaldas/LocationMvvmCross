using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Location.Core;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android.Core;

namespace Location.Droid
{
    public class Setup : MvxAndroidSetup<App>
    {
        protected override void InitializeFirstChance()
        {
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ILocationService, LocationService>();
            base.InitializeFirstChance();
        }

        public override IEnumerable<Assembly> GetPluginAssemblies()
        {
            var assemblies = base.GetPluginAssemblies().ToList();
            assemblies.Add(typeof(MvvmCross.Plugin.Visibility.Platforms.Android.Plugin).Assembly);
            return assemblies;
        }
    }
}