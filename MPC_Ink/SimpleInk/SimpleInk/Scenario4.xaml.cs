//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace SimpleInk
{
    /// <summary>
    /// This page displays code for remote ink identification
    /// </summary>
    public sealed partial class Scenario4 : Page
    {
        private MainPage rootPage;

        //socket definition
        bool isConnect = false;
        string ipAddress = string.Empty;
        string ServiceName = "22112";
        StreamSocketListener listener;
        StreamSocket socketSend;
        StreamSocket socketReceive;
        DataWriter writerSend;
        DataWriter writerReceive;
        //brush thickness definition
        const int minPenSize = 2;
        const int penSizeIncrement = 2;
        int penSize;
        int selectThickness = 1;

        public Scenario4()
        {
            this.InitializeComponent();
            //read language related resource file .strings/en or zh-cn/resources.resw
            Run run1 = new Run();
            run1.Text = ResourceManagerHelper.ReadValue("Description4_p1");
            Run run2 = new Run();
            run2.Text = ResourceManagerHelper.ReadValue("Description4_p2");
            this.textDes.Inlines.Add(run1);
            this.textDes.Inlines.Add(new LineBreak());
            this.textDes.Inlines.Add(run2);
            this.txtBoxAddress.PlaceholderText = ResourceManagerHelper.ReadValue("IP");

            penSize = minPenSize + penSizeIncrement * selectThickness;
            // Initialize drawing attributes. These are used in inking mode.
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Red;
            drawingAttributes.Size = new Size(penSize, penSize);
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;

            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;
            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
            this.SizeChanged += Scenario4_SizeChanged;
            this.radioBtnClient.Checked += radioBtnClient_Checked;
            this.radioBtnServer.Checked += radioBtnServer_Checked;
        }

        private void Scenario4_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetCanvasSize();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //set inkCanvas size
            rootPage = MainPage.Current;
            this.SetCanvasSize();
        }

        private void SetCanvasSize()
        {
            //set inkCanvas size
            outputGrid.Height = Window.Current.Bounds.Height / 2;
            inkCanvas.Height = Window.Current.Bounds.Height / 2;
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            SendInk();
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            SendInk();
        }

        void OnPenColorChanged(object sender, RoutedEventArgs e)
        {
            this.FlyoutColor.Hide();
            if (inkCanvas != null)
            {
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();

                // Use button's background to set new pen's color
                var borderSender = sender as Border;
                var brush = borderSender.Background as Windows.UI.Xaml.Media.SolidColorBrush;

                drawingAttributes.Color = brush.Color;
                inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            }
        }

        private void AppBarThickness_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.FlyoutPenThickness.ShowAt((FrameworkElement)sender);
        }

        private void AppBarColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.FlyoutColor.ShowAt((FrameworkElement)sender);
        }

        private void TextBlockThickness_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //set brush thickness
            this.FlyoutPenThickness.Hide();
            if (inkCanvas != null)
            {
                this.selectThickness = int.Parse(((TextBlock)sender).Tag.ToString());
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
                penSize = minPenSize + penSizeIncrement * this.selectThickness;
                drawingAttributes.Size = new Size(penSize, penSize);
                inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            }
        }

        private void btnClear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            SendInk();
        }

        async void OnSaveAsync(object sender, RoutedEventArgs e)
        {
            // We don't want to save an empty file
            if (inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add("Gif with embedded ISF", new System.Collections.Generic.List<string> { ".gif" });

                Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
                if (null != file)
                {
                    try
                    {
                        using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(stream);
                        }
                        rootPage.ShowMessage(inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count + " stroke(s) saved!");
                    }
                    catch (Exception ex)
                    {
                        rootPage.ShowMessage(ex.Message);
                    }
                }
            }
            else
            {
                rootPage.ShowMessage("There is no ink to save.");
            }
        }

        private async void btnConnect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // By default 'HostNameForConnect' is disabled and host name validation is not required. When enabling the
            // text box validating the host name is required since it was received from an untrusted source
            // (user input). The host name is validated by catching ArgumentExceptions thrown by the HostName
            // constructor for invalid input.
            HostName hostName;
            try
            {
                hostName = new HostName(this.txtBoxAddress.Text);
            }
            catch (ArgumentException)
            {
                rootPage.ShowMessage("Error: Invalid host name.");
                return;
            }
            this.socketSend = new StreamSocket();

            // If necessary, tweak the socket's control options before carrying out the connect operation.
            // Refer to the StreamSocketControl class' MSDN documentation for the full list of control options.
            this.socketSend.Control.KeepAlive = false;

            try
            {
                rootPage.ShowMessage("Connecting to: " + hostName);

                // Connect to the server (by default, the listener we created in the previous step).
                await this.socketSend.ConnectAsync(hostName, ServiceName);
                isConnect = true;

                rootPage.ShowMessage("Connected");

                await Task.Run(async () =>
                {
                    DataReader reader = new DataReader(this.socketSend.InputStream);

                    try
                    {
                        while (true)
                        {
                            // Read first 4 bytes (length of the subsequent string).
                            uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                            if (sizeFieldCount != sizeof(uint))
                            {
                                // The underlying socket was closed before we were able to read the whole data.
                                return;
                            }

                            // Read the string.
                            uint stringLength = reader.ReadUInt32();
                            uint actualStringLength = await reader.LoadAsync(stringLength);
                            if (stringLength != actualStringLength)
                            {
                                // The underlying socket was closed before we were able to read the whole data.
                                return;
                            }
                            byte[] dataArray = new byte[actualStringLength];
                            reader.ReadBytes(dataArray);


                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                inkCanvas.InkPresenter.StrokeContainer.Clear();

                                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                                await randomAccessStream.WriteAsync(dataArray.AsBuffer());
                                randomAccessStream.Seek(0);
                                await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(randomAccessStream);

                            }).AsTask();

                            // Display the string on the screen. The event is invoked on a non-UI thread, so we need to marshal
                            // the text back to the UI thread.
                            //Debug.WriteLine(String.Format("Received data: \"{0}\"", reader.ReadString(actualStringLength)));
                            //string str = reader.ReadString(actualStringLength);
                        }
                    }
                    catch (Exception exception)
                    {
                        // If this is an unknown status it means that the error is fatal and retry will likely fail.
                        if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                        {
                            rootPage.ShowMessage("Connect failed with error: " + exception.Message);
                        }
                    }

                });
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    rootPage.ShowMessage("Connect failed with error: " + exception.Message);
                }
            }
        }

        private async void SendInk()
        {
            if (!this.isConnect) return;
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();

            await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(randomAccessStream);

            Stream stream = randomAccessStream.CloneStream().AsStream();

            byte[] data = new byte[stream.Length];

            stream.Read(data, 0, (int)stream.Length);

            SendData(data);
        }

        private async void SendData(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            // Write the locally buffered data to the network.
            try
            {
                if (this.radioBtnClient.IsChecked == true)
                {
                    if (writerSend == null)
                        writerSend = new DataWriter(this.socketSend.OutputStream);

                    writerSend.WriteInt32(data.Length);
                    writerSend.WriteBytes(data);
                    await writerSend.StoreAsync();
                }
                else if (this.radioBtnServer.IsChecked == true)
                {
                    if (writerReceive == null)
                        writerReceive = new DataWriter(this.socketReceive.OutputStream);

                    writerReceive.WriteInt32(data.Length);
                    writerReceive.WriteBytes(data);
                    await writerReceive.StoreAsync();
                }

            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error if fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    rootPage.ShowMessage("Send failed with error:" + exception.Message);
                }
                this.isConnect = false;
                if (this.radioBtnServer.IsChecked == true)
                    this.txtBoxAddress.Text = this.ipAddress;
            }
        }

        private void radioBtnClient_Checked(object sender, RoutedEventArgs e)
        {
            this.listener.Dispose();
            this.btnConnect.Visibility = Visibility.Visible;
            this.txtBoxAddress.Text = string.Empty;
            this.txtBoxAddress.IsHitTestVisible = true;
        }

        private void radioBtnServer_Checked(object sender, RoutedEventArgs e)
        {
            this.btnConnect.Visibility = Visibility.Collapsed;
            this.StartServer();
            this.txtBoxAddress.IsHitTestVisible = false;
        }

        public async void StartServer()
        {
            try
            {
                //get IP
                var icp = NetworkInformation.GetInternetConnectionProfile();
                if (icp != null && icp.NetworkAdapter != null)
                {
                    var hostname =
                        NetworkInformation.GetHostNames()
                            .SingleOrDefault(
                                hn =>
                                hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                                && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                == icp.NetworkAdapter.NetworkAdapterId);

                    if (hostname != null)
                    {
                        // the ip address
                        this.ipAddress = hostname.CanonicalName;
                        this.txtBoxAddress.Text = hostname.CanonicalName;
                    }
                }
                listener = new StreamSocketListener();
                listener.ConnectionReceived += OnConnection;

                // If necessary, tweak the listener's control options before carrying out the bind operation.
                // These options will be automatically applied to the connected StreamSockets resulting from
                // incoming connections (i.e., those passed as arguments to the ConnectionReceived event handler).
                // Refer to the StreamSocketListenerControl class' MSDN documentation for the full list of control options.
                listener.Control.KeepAlive = false;

                // Try to bind to a specific address.
                await listener.BindEndpointAsync(new HostName(txtBoxAddress.Text), ServiceName);

                rootPage.ShowMessage("Listening");
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    rootPage.ShowMessage(
                   "Start listening failed with error: " + exception.Message);
                }
            }
        }

        private async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            socketReceive = args.Socket;
            this.isConnect = true;
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.txtBoxAddress.Text = "Connected";
            });

            try
            {
                while (true)
                {
                    // Read first 4 bytes (length of the subsequent string).
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        // The underlying socket was closed before we were able to read the whole data.
                        return;
                    }

                    // Read the string.
                    uint stringLength = reader.ReadUInt32();
                    uint actualStringLength = await reader.LoadAsync(stringLength);
                    if (stringLength != actualStringLength)
                    {
                        // The underlying socket was closed before we were able to read the whole data.
                        return;
                    }
                    byte[] dataArray = new byte[actualStringLength];
                    reader.ReadBytes(dataArray);

                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        //set new ink
                        inkCanvas.InkPresenter.StrokeContainer.Clear();

                        InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                        await randomAccessStream.WriteAsync(dataArray.AsBuffer());
                        randomAccessStream.Seek(0);
                        await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(randomAccessStream);

                    }).AsTask();
                }
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    rootPage.ShowMessage(
                  "Start Receive failed with error: " + exception.Message);
                }
            }
        }


    }
}
