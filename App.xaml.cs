using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Linq;
using wingman.Interfaces;
using wingman.Services;
using wingman.Updates;
using wingman.ViewModels;
using wingman.Views;

namespace wingman
{
    public partial class App : Application, IDisposable
    {
        private readonly IHost _host;
        private readonly AppUpdater _appUpdater;

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            _host = BuildHost();
            Ioc.Default.ConfigureServices(_host.Services);

            _appUpdater = new AppUpdater();
        }

        public void Dispose()
        {
            var serviceTypes = _host.Services.GetType().Assembly.GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IDisposable)) && !t.IsInterface);

            foreach (var serviceType in serviceTypes)
            {
                var serviceInstance = _host.Services.GetService(serviceType);
                if (serviceInstance is IDisposable disposableService)
                {
                    disposableService.Dispose();
                }
            }
            Debug.WriteLine("Services disposed.");
            _host.Dispose();
            Debug.WriteLine("Host disposed.");
        }


        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var Logger = Ioc.Default.GetRequiredService<ILoggingService>();
            Logger.LogException(e.Exception.ToString());
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            Ioc.Default.GetRequiredService<StatusWindow>(); // make sure events are initialized
            Ioc.Default.GetRequiredService<IEventHandlerService>(); // make sure events are initialized

            IAppActivationService appWindowService = Ioc.Default.GetRequiredService<IAppActivationService>();
            appWindowService.Activate(args);
            MainWindow mw = Ioc.Default.GetRequiredService<MainWindow>();
            mw.SetApp(this);

            _ = _appUpdater.CheckForUpdatesAsync(mw.Dispatcher);
        }

        private static IHost BuildHost() => Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    _ = services
                        // Services
                        .AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>()
                        .AddSingleton<ILoggingService, LoggingService>()
                        .AddSingleton<IEventHandlerService, EventHandlerService>()
                        .AddSingleton<IWindowingService, WindowingService>()
                        .AddSingleton<IMicrophoneDeviceService, MicrophoneDeviceService>()
                        .AddSingleton<IEditorService, EditorService>()
                        .AddSingleton<IStdInService, StdInService>()
                        .AddSingleton<ILocalSettings, LocalSettingsService>()
                        .AddSingleton<IAppActivationService, AppActivationService>()
                        .AddTransient<IOpenAIAPIService, OpenAIAPIService>()
                        // ViewModels   
                        .AddSingleton<AudioInputControlViewModel>()
                        .AddSingleton<OpenAIControlViewModel>()
                        .AddSingleton<MainWindowViewModel>()
                        .AddTransient<ModalControlViewModel>()
                        .AddSingleton<MainPageViewModel>()
                        .AddSingleton<FooterViewModel>()
                        // Views
                        .AddSingleton<StatusWindow>()
                        .AddSingleton<MainWindow>();

                })
                .Build();
    }
}