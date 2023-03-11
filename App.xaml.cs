using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using OpenAI.GPT3.Extensions;
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
            var test = "hello";
            var test2 = test;
            InitializeComponent();
            //UnhandledException += App_UnhandledException;
            _host = BuildHost();
            Ioc.Default.ConfigureServices(_host.Services);
            var settingsService = _host.Services.GetService<ISettingsService>();
            if (!settingsService.TryLoad<bool>("App", "FirstRun", out var FirstRun))
            {
                settingsService.Save("App", "FirstRun", false);

                settingsService.Save("Wingman", "Main_Hotkey", "`");
                settingsService.Save("Wingman", "Modal_Hotkey", "Ctrl+T");

                settingsService.Save("Wingman", "Trim_Whitespaces", false);
                settingsService.Save("Wingman", "Trim_Newlines", false);
                settingsService.Save("Wingman", "Append_Clipboard", false);
                settingsService.Save("Wingman", "Append_Clipboard_Modal", false);

                settingsService.Save("Wingman", "System_Preprompt", "You are a programming assistant.  You are only allowed to respond with the raw code.  Do not generate explanations.  Do not preface.  Do not follow-up after the code.");
            }

        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            IAppActivationService appWindowService = Ioc.Default.GetRequiredService<IAppActivationService>();
            appWindowService.Activate(args);

            Ioc.Default.GetRequiredService<EventsHandler>();
            //Ioc.Default.GetRequiredService<INamedPipesService>();
        }



        private static IHost BuildHost() => Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    _ = services
                        .AddSingleton<HookProvider>()
                        .AddSingleton<EventsHandler>()
                        .AddSingleton<INativeKeyboard, NativeKeyboard>()
                        .AddSingleton<IKeybindEvents, KeybindEvents>()
                        //.AddSingleton<INamedPipesService, NamedPipesService>()
                        .AddSingleton<IMicrophoneDeviceService, MicrophoneDeviceService>()
                        .AddSingleton<IEditorService, EditorService>()
                        .AddSingleton<IStdInService, StdInService>()
                        .AddSingleton<ISettingsService, LocalSettingsService>()
                        .AddSingleton<IAppThemeService, AppThemeService>()
                        .AddSingleton<ILocalizationService, LocalizationService>()
                        .AddSingleton<IAppTitleBarService, AppTitleBarService>()
                        .AddSingleton<IAppActivationService, AppActivationService>()
                        .AddOpenAIService(settings =>
                        {
                            var settingsService = services.BuildServiceProvider().GetService<ISettingsService>();
                            if (settingsService?.TryLoad<string>("OpenAI", "ApiKey", out var apiKey) ?? false)
                            {
                                settings.ApiKey = apiKey;
                            }
                            else
                            {
                                settings.ApiKey = "YOU MUST REPLACE THIS WITH YOUR KEY";
                                settingsService?.TrySave("OpenAI", "ApiKey", settings.ApiKey);
                            }
                            //settings.Organization = "Put your organization here";
                            //this is for people who are a part of multiple organizations only, not implementing for now
                        })
                        .AddSingleton<IOpenAIAPIService, OpenAIAPIService>()
                        .AddSingleton<IWindowingService, WindowingService>()
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