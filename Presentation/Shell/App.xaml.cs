using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Events;

using Aksl.Toolkit.Dialogs;
using Aksl.Toolkit.Services;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;

using Aksl.Modules.Shell;
using Aksl.Modules.Shell.ViewModels;
using Aksl.Modules.Shell.Views;

using Aksl.Modules.HamburgerMenuNavigationSideBar;

using Aksl.Modules.Account;

using Aksl.Modules.Functions;
using Aksl.Modules.FlowControls;

using Aksl.Views;
using Aksl.ViewModels;

namespace Aksl.Wpf.Unity
{
    public partial class App
    {
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.Register(typeof(ShellView).ToString(), () => Container.Resolve<ShellViewModel>());
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialogWindow<FixedSizeDialogWindow>(name:nameof(FixedSizeDialogWindow));

            containerRegistry.RegisterSingleton(typeof(IDialogService), typeof(DialogService));
            containerRegistry.RegisterSingleton(typeof(IDialogViewService), typeof(DialogViewService));

            containerRegistry.RegisterDialog<ConfirmView, ConfirmViewModel>();

            containerRegistry.RegisterDialog<EditXNodeView, EditXNodeViewModel>();
            ViewModelLocationProvider.Register(typeof(EditXNodeView).ToString(),() => Container.Resolve<EditXNodeViewModel>());

            RegisterMenuFactoryAsync(containerRegistry).Await();

            RegisterBuildWorkspaceViewEventAsync();
        }

        protected async Task RegisterMenuFactoryAsync(IContainerRegistry containerRegistry)
        {
            try
            {
                MenuService menuService = new(new List<string> {"pack://application:,,,/Aksl.Wpf.DragDrop;Component/Data/AllMenus.xml",
                                                                "pack://application:,,,/Aksl.Wpf.DragDrop;Component/Data/Blacks.xml",
                                                                "pack://application:,,,/Aksl.Wpf.DragDrop;Component/Data/Blues.xml",
                                                                "pack://application:,,,/Aksl.Wpf.DragDrop;Component/Data/FlowControls.xml",
                                                                "pack://application:,,,/Aksl.Wpf.DragDrop;Component/Data/Functions.xml",    
                                                                });
               
                  await menuService.CreateMenusAsync();

                containerRegistry.RegisterInstance<IMenuService>(menuService);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        protected Task RegisterBuildWorkspaceViewEventAsync()
        {
            try
            {
                var eventAggregator = Container.Resolve<IEventAggregator>();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            return Task.CompletedTask;
        }


        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            _ = moduleCatalog.AddModule(nameof(AccountModule), typeof(AccountModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);

            _ = moduleCatalog.AddModule(typeof(HamburgerMenuNavigationSideBarModule).Name, typeof(HamburgerMenuNavigationSideBarModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(typeof(ShellModule).Name, typeof(ShellModule).AssemblyQualifiedName, InitializationMode.WhenAvailable, dependsOn: typeof(HamburgerMenuNavigationSideBarModule).Name);

            _ = moduleCatalog.AddModule(nameof(FlowControlsModule), typeof(FlowControlsModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
            _ = moduleCatalog.AddModule(nameof(FunctionsModule), typeof(FunctionsModule).AssemblyQualifiedName, InitializationMode.WhenAvailable);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<ShellView>();
        }

        protected override  void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}
