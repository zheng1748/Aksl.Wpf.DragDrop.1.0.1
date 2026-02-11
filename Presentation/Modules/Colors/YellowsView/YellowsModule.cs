
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Modules.Yellows.ViewModels;
using Aksl.Modules.Yellows.Views;

namespace Aksl.Modules.Yellows
{
    public class YellowsModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public YellowsModule()
        {
            this._container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<YellowView>();
            containerRegistry.RegisterForNavigation<YellowGreenView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(typeof(YellowView).ToString(),
                                              () => this._container.Resolve<YellowViewModel>());
            ViewModelLocationProvider.Register(typeof(YellowGreenView).ToString(),
                                             () => this._container.Resolve<YellowGreenViewModel>());
        }
        #endregion
    }
}
