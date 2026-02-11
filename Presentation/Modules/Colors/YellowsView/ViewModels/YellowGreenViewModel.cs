using System.Threading.Tasks;

using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Aksl.Modules.Yellows.ViewModels
{
    public class YellowGreenViewModel : BindableBase, INavigationAware
    {
        #region Members
        #endregion

        #region Constructors
        public YellowGreenViewModel()
        {
        }
        #endregion

        #region Properties
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
