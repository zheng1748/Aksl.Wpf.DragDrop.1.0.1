
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Modules.Blacks.ViewModels;
using Aksl.Modules.Blacks.Views;

namespace Aksl.Modules.Blacks
{
    public class BlacksModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public BlacksModule()
        {
            this._container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<BlackView>();
            containerRegistry.RegisterForNavigation<SilverView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(typeof(BlackView).ToString(),
                                               () => this._container.Resolve<BlackViewModel>());
            ViewModelLocationProvider.Register(typeof(SilverView).ToString(),
                                              () => this._container.Resolve<SilverViewModel>());
        }
        #endregion
    }
}
