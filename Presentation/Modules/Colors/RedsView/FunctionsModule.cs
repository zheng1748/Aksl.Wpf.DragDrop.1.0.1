
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Modules.Functions.ViewModels;
using Aksl.Modules.Functions.Views;

namespace Aksl.Modules.Functions
{
    public class FunctionsModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public FunctionsModule()
        {
            this._container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<RelationOperationView>();
            containerRegistry.RegisterForNavigation<MultithreadExecuteView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(typeof(RelationOperationView).ToString(),
                                               () => this._container.Resolve<RelationOperationViewModel>());
            ViewModelLocationProvider.Register(typeof(MultithreadExecuteView).ToString(),
                                              () => this._container.Resolve<MultithreadExecuteViewModel>());
        }
        #endregion
    }
}
