using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.UI.Core;
using wingman.Interfaces;

namespace wingman.Updates
{
    public class AppUpdater
    {
        private StoreContext context;
        private ILoggingService Logger;

        public AppUpdater()
        {
            context = StoreContext.GetDefault();

            Logger = Ioc.Default.GetRequiredService<ILoggingService>();
        }

        public async Task CheckForUpdatesAsync(CoreDispatcher dispatcher)
        {
            Logger.LogInfo("Checking for updates...");
            IReadOnlyList<StorePackageUpdate> storePackageUpdates = await context.GetAppAndOptionalStorePackageUpdatesAsync();

            if (storePackageUpdates.Count > 0)
            {
                Logger.LogInfo("Updates available.");
                if (!context.CanSilentlyDownloadStorePackageUpdates)
                {
                    Logger.LogError("Unable to silently download store packages");
                    return;
                }

                Logger.LogInfo("Downloading updates.");
                StorePackageUpdateResult downloadResult = await context.TrySilentDownloadStorePackageUpdatesAsync(storePackageUpdates);

                switch (downloadResult.OverallState)
                {
                    case StorePackageUpdateState.Completed:
                        Logger.LogInfo("Updates downloaded.");
                        if (IsNowAGoodTimeToRestartApp())
                        {
                            Logger.LogInfo("Installing updates.");
                            await InstallUpdate(storePackageUpdates);
                        }
                        else
                        {
                            // Retry/reschedule the installation later.
                            Logger.LogInfo("Retrying later.");
                            RetryInstallLater(dispatcher);
                        }
                        break;

                    case StorePackageUpdateState.Canceled:
                    case StorePackageUpdateState.ErrorLowBattery:
                    case StorePackageUpdateState.ErrorWiFiRecommended:
                    case StorePackageUpdateState.ErrorWiFiRequired:
                    case StorePackageUpdateState.OtherError:
                        RetryDownloadAndInstallLater(dispatcher);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                Logger.LogInfo("No Updates available.");
            }
        }

        private async Task InstallUpdate(IReadOnlyList<StorePackageUpdate> storePackageUpdates)
        {
            StorePackageUpdateResult installResult = await context.TrySilentDownloadAndInstallStorePackageUpdatesAsync(storePackageUpdates);

            switch (installResult.OverallState)
            {
                case StorePackageUpdateState.Canceled:
                case StorePackageUpdateState.ErrorLowBattery:
                case StorePackageUpdateState.OtherError:
                    Logger.LogError("Installation Failed: " + installResult.OverallState.ToString());
                    Logger.LogError("This could be broken.  Try uninstalling/reinstalling from store");
                    //RetryInstallLater();
                    break;

                default:
                    break;
            }
        }

        private bool IsNowAGoodTimeToRestartApp()
        {
            // Implement logic to determine if now is a good time to restart the app.
            // For example, you can check if the app has been idle for a certain period of time.
            return true;
        }

        private async void RetryDownloadAndInstallLater(CoreDispatcher dispatcher)
        {
            // Implement logic for retrying the download and installation later.
            // You can use a timer or another scheduling mechanism to retry after a certain period of time.
        }

        private async void RetryInstallLater(CoreDispatcher dispatcher)
        {
            // Implement logic for retrying the installation later.
            // You can use a timer or another scheduling mechanism to retry after a certain period of time.
        }
    }
}