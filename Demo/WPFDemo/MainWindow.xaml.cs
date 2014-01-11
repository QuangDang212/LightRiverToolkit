using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using App.Services;
using LightRiver.Net;
using LightRiver.Net.Sockets;

namespace WPFDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TelegramExchangeDispatcher _telegramExchangeDispatcher = new TelegramExchangeDispatcher();
        private ParserPool _parserPool = new ParserPool();

        public MainWindow()
        {
            InitializeComponent();
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
