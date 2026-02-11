using System.Threading.Tasks;

using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Aksl.Modules.Reds.ViewModels
{
    public class DarkRedViewModel : BindableBase, INavigationAware
    {
        #region Members
        #endregion

        #region Constructors
        public DarkRedViewModel()
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
