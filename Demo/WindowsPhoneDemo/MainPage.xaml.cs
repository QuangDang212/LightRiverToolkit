using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using App.Services;
using LightRiver.Net;
using LightRiver.Net.Sockets;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WindowsPhoneDemo.Resources;

namespace WindowsPhoneDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        private TelegramExchangeDispatcher _telegramExchangeDispatcher = new TelegramExchangeDispatcher();
        private ParserPool _parserPool = new ParserPool();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // 1. Create a service instance and give it a service address
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

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}