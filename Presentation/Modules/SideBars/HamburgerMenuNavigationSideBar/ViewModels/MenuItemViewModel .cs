using System;
using System.Windows.Input;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Infrastructure;
using Aksl.Toolkit.Controls;
using Aksl.Toolkit.Services;
using Aksl.Toolkit.UI;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;
using Aksl.Views;
using System.Linq;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels
{
    public class MenuItemViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator; 
        private readonly IDialogViewService _dialogViewService;
        private readonly MenuItem _menuItem;
        #endregion

        #region Constructors
        public MenuItemViewModel(IEventAggregator eventAggregator, int groupIndex, int index, MenuItem menuItem)
        {
            _eventAggregator = eventAggregator;
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

            GroupIndex = groupIndex;
            Index = index;
            _menuItem = menuItem;
        }
        #endregion

        #region Properties
        public MenuItem MenuItem => _menuItem;
        public int GroupIndex { get; }
        public int Index { get; }
        public string Name => _menuItem.Name;
        public string Title => _menuItem.Title;
        public bool IsLeaf => _menuItem.SubMenus.Count <= 0;
        private bool IsNextNavigation => _menuItem.IsNextNavigation;
        private bool HasNavigationName => !string.IsNullOrEmpty(_menuItem.NavigationName);
        private bool IsNexOnNotLeaf => _menuItem.IsNexOnNotLeaf;

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty<bool>(ref _isSelected, value);
        }

        public PackIconKind IconKind
        {
            get
            {
                PackIconKind kind = PackIconKind.None;

                _ = Enum.TryParse(_menuItem.IconKind, out kind);

                return kind;
            }
        }

        private bool _isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => SetProperty<bool>(ref _isPaneOpen, value);
        }

        protected bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty<bool>(ref _isEnabled, value);
        }
        #endregion

        #region Loaded Event
        private AdornerLayer _adornerLayer = null;
        public async void Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.UserControl uc)
            {
                try
                {
                    VisualTreeFinder visualTreeFinder = new();
                    var listViewItem = visualTreeFinder.FindVisualParent<System.Windows.Controls.ListViewItem>(uc);

                    if (listViewItem is not null)
                    {
                        //listViewItem.PreviewMouseLeftButtonDown += (sender, e) =>
                        //{
                        //    if (sender is System.Windows.Controls.ListViewItem listViewItem)
                        //    {
                        //        System.Windows.DragDrop.DoDragDrop(dragSource: (System.Windows.DependencyObject)sender, data: _menuItem, allowedEffects: System.Windows.DragDropEffects.Copy);
                        //    }
                        //};

                        listViewItem.PreviewMouseMove += (sender, e) =>
                        {
                            if (sender is System.Windows.Controls.ListViewItem listViewItem)
                            {
                                var parents = visualTreeFinder.FindVisualParents<System.Windows.FrameworkElement>(listViewItem);
                                var grids = parents.Where(d => (d is System.Windows.Controls.Grid));
                                var rootGrid = grids.FirstOrDefault(d => d.Name== "HamburgerMenuLayoutGrid");

                                var childsInListViewItem = visualTreeFinder.FindLogicalChilds<System.Windows.Controls.Label>(uc);
                                var label = childsInListViewItem.FirstOrDefault(d => (d is System.Windows.Controls.Label)) as System.Windows.Controls.Label;

                                //var adorner = new SimpleCircleAdorner(listViewItem);
                                //_adornerLayer = AdornerLayer.GetAdornerLayer(rootGrid);
                                //_adornerLayer.Add(adorner);

                                DragDropAdorner adorner = new(listViewItem);
                                _adornerLayer = AdornerLayer.GetAdornerLayer(rootGrid);
                                _adornerLayer.Add(adorner);

                                System.Windows.DragDrop.DoDragDrop(dragSource: (System.Windows.DependencyObject)sender, data: _menuItem, allowedEffects: System.Windows.DragDropEffects.Copy);

                                _adornerLayer.Remove(adorner);
                                _adornerLayer = null;
                            }
                        };

                        listViewItem.QueryContinueDrag += (sender, e) =>
                        {
                            if (sender is System.Windows.Controls.ListViewItem listViewItem)
                            {
                                //Debug.Print($"QueryContinueDrag");
                                _adornerLayer.Update();
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Loaded Error: \"{ex.Message}\"", title: "Error");
                }
            }
        }
        #endregion

        #region Mouse Left Button Down Event
        public async void ExecuteDrag(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.UserControl uc)
                {
                    System.Windows.DragDrop.DoDragDrop(dragSource: (System.Windows.DependencyObject)sender, data: _menuItem, allowedEffects: System.Windows.DragDropEffects.Copy);
                }
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"ExecuteDrop Error.: \"{ex.Message}\"", title: "Error");
            }
        }
        #endregion
    }
}
