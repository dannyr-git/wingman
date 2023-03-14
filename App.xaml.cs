using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using wingman.Handlers;
using wingman.Interfaces;
using wingman.Natives;
using wingman.Services;
using wingman.ViewModels;
using wingman.Views;

namespace wingman
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            _host = BuildHost();
            Ioc.Default.ConfigureServices(_host.Services);
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // complete this function
            Console.WriteLine(e.Exception.ToString());
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            Ioc.Default.GetRequiredService<EventsHandler>(); // make sure events are initialized

            IAppActivationService appWindowService = Ioc.Default.GetRequiredService<IAppActivationService>();
            appWindowService.Activate(args);
        }

        private static IHost BuildHost() => Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    _ = services
                        .AddSingleton<EventsHandler>()
                        .AddSingleton<HookProvider>()
                        .AddSingleton<INativeKeyboard, NativeKeyboard>()
                        .AddSingleton<IKeybindEvents, KeybindEvents>()
                        .AddSingleton<IMicrophoneDeviceService, MicrophoneDeviceService>()
                        .AddSingleton<IEditorService, EditorService>()
                        .AddSingleton<IStdInService, StdInService>()
                        .AddSingleton<ILocalSettings, LocalSettingsService>()
                        .AddSingleton<IAppActivationService, AppActivationService>()
                        .AddTransient<IOpenAIAPIService, OpenAIAPIService>()
                        .AddSingleton<AudioInputControlViewModel>()
                        .AddSingleton<OpenAIControlViewModel>()
                        .AddSingleton<MainWindowViewModel>()
                        .AddTransient<ModalControlViewModel>()
                        .AddSingleton<MainPageViewModel>()
                        .AddSingleton<MainWindow>();

                })
                .Build();
    }
}