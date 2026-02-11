
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Modules.Blues.ViewModels;
using Aksl.Modules.Blues.Views;

namespace Aksl.Modules.Blues
{
    public class BluesModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public BluesModule()
        {
            this._container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<BlueView>();
            containerRegistry.RegisterForNavigation<LightBlueView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(typeof(BlueView).ToString(),
                                               () => this._container.Resolve<BlueViewModel>());
            ViewModelLocationProvider.Register(typeof(LightBlueView).ToString(),
                                            () => this._container.Resolve<LightBlueViewModel>());
        }
        #endregion
    }
}
