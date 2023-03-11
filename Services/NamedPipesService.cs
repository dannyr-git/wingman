using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wingman.Interfaces;

namespace wingman.Services
{
    public class NamedPipesService : INamedPipesService, IDisposable
    {
        private bool disposed = false;
        private CancellationTokenSource cts;
        private Task mouseServer;

        public NamedPipesService()
        {
            cts = new CancellationTokenSource();
            mouseServer = Task.Run(MouseServer, cts.Token);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                cts.Cancel();
                mouseServer.Wait();
            }
        }

        private async Task MouseServer()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var cmd = await Listen("MousePipe");
                    switch (cmd)
                    {
                        case "MouseArrow":
                            ToggleMouseArrow();
                            break;
                        case "MouseWait":
                            ToggleMouseWait();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or do something else with it
                    Console.WriteLine(ex.ToString());
                }
                await Task.Delay(1000);
            }
        }

        public async Task SendMessageAsync(string message)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "MousePipe", PipeDirection.Out, PipeOptions.Asynchronous))
            {
                var connectTask = pipeClient.ConnectAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

                // Wait for either the connection to succeed or the timeout to elapse
                await Task.WhenAny(connectTask, timeoutTask);

                if (!pipeClient.IsConnected)
                {
                    throw new TimeoutException("Timed out while waiting for named pipe connection.");
                }

                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await pipeClient.WriteAsync(buffer, 0, buffer.Length);
                await pipeClient.FlushAsync();

            }
        }


        private async Task<string> Listen(string PipeName)
        {
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.ReadWrite, AccessControlType.Allow));
            // Create NamedPipeServerStream
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                //pipeServer.SetAccessControl(pipeSecurity);
                // Wait for a Connection from a client
                await Task.Factory.FromAsync(pipeServer.BeginWaitForConnection, pipeServer.EndWaitForConnection, null);

                string msg = string.Empty;
                using (StreamReader reader = new StreamReader(pipeServer))
                {
                    // Wait for object to be placed on stream by client then read it   
                    msg = await reader.ReadToEndAsync();
                }

                return msg;
            }
        }


        private async void ToggleMouseArrow()
        {
            /*
             * Many bugs in Win UI right now prevent any workarounds i tried (including this namedpipes solution) from working.
             * You can read more about similar issues here :
             * 
             * https://github.com/microsoft/microsoft-ui-xaml/issues/7947
             *
             * https://github.com/microsoft/microsoft-ui-xaml/issues/7062
             *
             * 
             */

        }

        private async void ToggleMouseWait()
        {

        }


    }

}