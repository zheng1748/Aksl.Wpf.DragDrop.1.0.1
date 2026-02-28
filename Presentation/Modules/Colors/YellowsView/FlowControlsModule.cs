
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Modules.FlowControls.ViewModels;
using Aksl.Modules.FlowControls.Views;

namespace Aksl.Modules.FlowControls
{
    public class FlowControlsModule : IModule
    {
        #region Members
        private readonly IUnityContainer _container;
        #endregion

        #region Constructors
        public FlowControlsModule()
        {
            this._container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
        }
        #endregion

        #region IModule
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CountCycleView>();
            containerRegistry.RegisterForNavigation<JudgmentView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(typeof(CountCycleView).ToString(),
                                              () => this._container.Resolve<CountCycleViewModel>());
            ViewModelLocationProvider.Register(typeof(JudgmentView).ToString(),
                                             () => this._container.Resolve<JudgmentViewModel>());
        }
        #endregion
    }
}
