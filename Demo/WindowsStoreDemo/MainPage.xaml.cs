using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Services;
using LightRiver.Net;
using LightRiver.Net.Sockets;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsStoreDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TelegramExchangeDispatcher _telegramExchangeDispatcher = new TelegramExchangeDispatcher();
        private ParserPool _parserPool = new ParserPool();

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // 1. Create a service instance
            var getPhotoService = new GetPhotoHttpService("http://testgetPhoto.photo.net/photo/get.aspx");
            // 2. Create parameter that service need which will become in this case GET style url
            var getPhotoParam = new GetPhotoParameter() {
                PhotoId = "1",
                IsThumbnail = true,
            };
            // 3. wait for the result
            var photoData = await getPhotoService.InvokeAsync(getPhotoParam);
        }

        private void ConnectSocketButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. create a platform dependent socket
            var socket = new TelegramSocket();
            // 2. create socket connector/sender/receiver
            var socketConnector = new TelegramSocketConnector(socket);
            var socketSender = new TelegramSocketSender(socket);
            var socketReceiver = new TelegramSocketReceiver(socket);
            // 3. setup connector setting(adding hosts)
            socketConnector.HostProvider.Hosts.Add(new TelegramExchangerHost("127.0.0.1", "1234"));
            socketConnector.ConnectionStateChanged += socketConnector_ConnectionStateChanged;
            // 4. start
            _telegramExchangeDispatcher.Start(socketConnector, socketSender, socketReceiver);
            _parserPool.Start();
        }

        private void socketConnector_ConnectionStateChanged(object sender, TelegramSocketConnectionStateChangeEventArgs e)
        {
        }

        private async void FetchDataSocket_Click(object sender, RoutedEventArgs e)
        {
            // 1. Create a service instance
            var getPhotoService = new GetPhotoSocketService(_telegramExchangeDispatcher, _parserPool);
            // 2. Create parameter that service need which will become in this case GET style url
            var getPhotoParam = new GetPhotoParameter() {
                PhotoId = "1",
                IsThumbnail = true,
            };
            // 3. wait for the result
            var photoData = await getPhotoService.InvokeAsync(getPhotoParam);
        }
    }
}
