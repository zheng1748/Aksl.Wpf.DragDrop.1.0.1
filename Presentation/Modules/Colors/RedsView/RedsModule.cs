
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Modules.Reds.ViewModels;
using Aksl.Modules.Reds.Views;

namespace Aksl.Modules.Reds
{
    public class RedsModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public RedsModule()
        {
            this._container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<OperationNodeView>();
            containerRegistry.RegisterForNavigation<DarkRedView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(typeof(OperationNodeView).ToString(),
                                               () => this._container.Resolve<OperationNodeViewModel>());
            ViewModelLocationProvider.Register(typeof(DarkRedView).ToString(),
                                              () => this._container.Resolve<DarkRedViewModel>());
        }
        #endregion
    }
}
