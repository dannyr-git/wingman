using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Linq;
using wingman.Handlers;
using wingman.Interfaces;
using wingman.Services;
using wingman.ViewModels;
using wingman.Views;

namespace wingman
{
    public partial class App : Application, IDisposable
    {
        private readonly IHost _host;

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            _host = BuildHost();
            Ioc.Default.ConfigureServices(_host.Services);
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

            Ioc.Default.GetRequiredService<EventsHandler>(); // make sure events are initialized

            IAppActivationService appWindowService = Ioc.Default.GetRequiredService<IAppActivationService>();
            appWindowService.Activate(args);
            MainWindow mw = Ioc.Default.GetRequiredService<MainWindow>();
            mw.SetApp(this);
        }

        private static IHost BuildHost() => Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    _ = services
                        .AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>()
                        .AddSingleton<ILoggingService, LoggingService>()
                        // Handlers
                        .AddSingleton<EventsHandler>()
                        //.AddSingleton<HookProvider>()
                        // Other
                        //.AddSingleton<INativeKeyboard, NativeKeyboard>()
                        //.AddSingleton<IKeybindEvents, KeybindEvents>()
                        // Services 
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
                        .AddSingleton<MainWindow>();

                })
                .Build();
    }
}