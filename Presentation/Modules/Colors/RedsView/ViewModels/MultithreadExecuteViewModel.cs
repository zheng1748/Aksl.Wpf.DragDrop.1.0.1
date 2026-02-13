using System.Windows.Media;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.ViewModels;

namespace Aksl.Modules.Functions.ViewModels
{
    public class MultithreadExecuteViewModel : BindableBase, INavigationAware
    {
        #region Members
        #endregion

        #region Constructors
        public MultithreadExecuteViewModel()
        {
            NodeViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<XNodeViewModel>();

            Initialize();
        }
        #endregion

        #region Initialize Method
        private void Initialize()
        {
            NodeViewModel.Content = "多线程执行";
            // NodeViewModel.ContentBackgroundColor = new SolidColorBrush(Colors.Red);
            //NodeViewModel.LineWidth = 5;
            //NodeViewModel.BorderVisible =  System.Windows.Visibility.Collapsed;
            //NodeViewModel.IsFocused =false;
        }
        #endregion

        #region Properties
        public XNodeViewModel NodeViewModel { get; set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
