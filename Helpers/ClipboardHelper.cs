using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;


namespace wingman.Helpers
{
    public static class ClipboardHelper
    {
        public static async Task SetTextAsync(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                await DispatcherQueue.GetForCurrentThread().EnqueueAsync(async () =>
                {
                    var data = new DataPackage
                    {
                        RequestedOperation = DataPackageOperation.Copy
                    };
                    data.SetText(value);
                    Clipboard.SetContent(data);
                    Clipboard.Flush();
                });
            }
        }

        public static async Task<string?> GetTextAsync()
        {
            string? result = null;

            await DispatcherQueue.GetForCurrentThread().EnqueueAsync(async () =>
            {
                var data = Clipboard.GetContent();
                if (data.Contains(StandardDataFormats.Text))
                {
                    result = await data.GetTextAsync();
                }
            });

            return result;
        }

    }
}
