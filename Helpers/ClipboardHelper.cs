using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace wingman.Helpers
{
    public static class ClipboardHelper
    {
        private static readonly SemaphoreSlim ClipboardSemaphore = new SemaphoreSlim(1, 1);

        public static async Task SetTextAsync(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                await ClipboardSemaphore.WaitAsync();
                try
                {
                    var data = new DataPackage
                    {
                        RequestedOperation = DataPackageOperation.Copy
                    };
                    data.SetText(value);
                    Clipboard.SetContent(data);
                    Clipboard.Flush();
                }
                finally
                {
                    ClipboardSemaphore.Release();
                }
            }
        }

        public static async Task SetBitmapAsync(IRandomAccessStream stream)
        {
            await ClipboardSemaphore.WaitAsync();
            try
            {
                var value = RandomAccessStreamReference.CreateFromStream(stream);
                var data = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                data.SetBitmap(value);
                Clipboard.SetContent(data);
                Clipboard.Flush();
            }
            finally
            {
                ClipboardSemaphore.Release();
            }
        }

        public static async Task SetBitmapAsync(IStorageFile file)
        {
            await ClipboardSemaphore.WaitAsync();
            try
            {
                var value = RandomAccessStreamReference.CreateFromFile(file);
                var data = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                data.SetBitmap(value);
                Clipboard.SetContent(data);
                Clipboard.Flush();
            }
            finally
            {
                ClipboardSemaphore.Release();
            }
        }

        public static async Task SetBitmapAsync(Uri uri)
        {
            await ClipboardSemaphore.WaitAsync();
            try
            {
                var value = RandomAccessStreamReference.CreateFromUri(uri);
                var data = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                data.SetBitmap(value);
                Clipboard.SetContent(data);
                Clipboard.Flush();
            }
            finally
            {
                ClipboardSemaphore.Release();
            }
        }

        public static async Task SetStorageItemsAsync(DataPackageOperation operation, params IStorageItem[] items)
        {
            await ClipboardSemaphore.WaitAsync();
            try
            {
                var data = new DataPackage
                {
                    RequestedOperation = operation,
                };
                data.SetStorageItems(items);
                Clipboard.SetContent(data);
                Clipboard.Flush();
            }
            finally
            {
                ClipboardSemaphore.Release();
            }
        }

        public static async Task<string?> GetTextAsync()
        {
            await ClipboardSemaphore.WaitAsync();
            try
            {
                var data = Clipboard.GetContent();
                if (data.Contains(StandardDataFormats.Text))
                {
                    return await data.GetTextAsync();
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                ClipboardSemaphore.Release();
            }
        }
    }

}
