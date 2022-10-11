using LivingRoomFan_Device.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
using Dapper;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace LivingRoomFan_Device
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DeviceClient _deviceClient;
        private readonly string _ApiUrl = "http://localhost:7177/api/devices/connect";
        private readonly string _connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\pelle\\source\\repos\\SmartApplication\\LivingRoomFan_Device\\Data\\FanDevices.mdf;Integrated Security=True;Connect Timeout=30";
        private int _Intervall = 1000;
        private bool _fanState = true;
        private bool _previousFanState = false;
        private bool _connected = false;
        private string _deviceId = "";
        private DeviceInformation _deviceInformation;





        public MainWindow()
        {
            InitializeComponent();
            RunSensor().ConfigureAwait(false);
        }
        private async Task RunSensor()
        {
            await Setup();
            await Loop();
        }
        private async Task Setup()
        {
            tbConnectingMessage.Text = "Initializing Device, Please wait.....";

            await Task.Delay(5000);

            using IDbConnection connection = new SqlConnection(_connectionString);
            _deviceId = await connection.QueryFirstOrDefaultAsync<string>("SELECT DeviceId FROM Device_Information");
            if (string.IsNullOrEmpty(_deviceId))
            {
                tbConnectingMessage.Text = "Generating new DeviceId";
                _deviceId = "SmartFan";
                await connection.ExecuteAsync(
                    "INSERT INTO Device_Information (DeviceId, DeviceType, DeviceName, Location, Owner) " +
                    "VALUES (@DeviceId, @DeviceType, @DeviceName, @Location, @Owner)",
                    new
                    {
                        DeviceId = _deviceId,
                        DeviceType = "Fan",
                        DeviceName = "AirCondition",
                        Location = "LivingRoom",
                        Owner = "Per"
                    });
            }

            var deviceConnectionString =
                await connection.QueryFirstOrDefaultAsync<string>("SELECT ConnectionString FROM Device_Information WHERE DeviceId = @DeviceId",
                    new { DeviceId = _deviceId });
            if (string.IsNullOrEmpty(deviceConnectionString))
            {
                tbConnectingMessage.Text = "Initializing ConnectionString, Please Wait.....";
                using var http = new HttpClient();
                var result = await http.PostAsJsonAsync(_ApiUrl, new { DeviceId = _deviceId });
                deviceConnectionString = await result.Content.ReadAsStringAsync();
                await connection.ExecuteAsync(
                    "UPDATE Device_Information SET ConnectionString = @ConnectionString WHERE DeviceId = @DeviceId",
                    new { DeviceId = _deviceId, ConnectionString = deviceConnectionString });
            }
            _deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);

            tbConnectingMessage.Text = "Updating Device Twin Properties, Please Wait.....";

            _deviceInformation = await connection.QueryFirstOrDefaultAsync<DeviceInformation>(
                "SELECT * FROM Device_Information where DeviceId = @DeviceId",
                new { DeviceId = _deviceId });
            var twinCollection = new TwinCollection();
            twinCollection["deviceName"] = _deviceInformation.DeviceName;
            twinCollection["deviceType"] = _deviceInformation.DeviceType;
            twinCollection["owner"] = _deviceInformation.Owner;
            twinCollection["location"] = _deviceInformation.Location;
            twinCollection["fanState"] = _fanState;
            twinCollection["previousFanState"] = _previousFanState;

            await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);

            _connected = true;
            tbConnectingMessage.Text = "Device Connected.";

        }
        private async Task Loop()
        {
            while (true)
            {
                if (_connected)
                {
                    if (_fanState != _previousFanState)
                    {

                        _fanState = !_fanState;
                        //d2c device 2 cloud
                        var json = JsonConvert.SerializeObject(new { fanState = _fanState });
                        var message = new Message(Encoding.UTF8.GetBytes(json));
                        message.Properties.Add("deviceName", _deviceInformation.DeviceName);
                        message.Properties.Add("deviceType", _deviceInformation.DeviceType);
                        message.Properties.Add("owner", _deviceInformation.Owner);
                        message.Properties.Add("location", _deviceInformation.Location);

                        await _deviceClient.SendEventAsync(message);
                        tbConnectingMessage.Text = $"Message sent at {DateTime.Now}.";
                        //device twin reported
                        var twinCollection = new TwinCollection();
                        twinCollection["fanState"] = _fanState;
                        twinCollection["previousFanState"] = null;
                        await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);
                    }
                }
                await Task.Delay(_Intervall);
            }

        }


        private void FanBtnOnOff_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_fanState)
            {
                FanBtnOnOff.Content = "Turn On";
                FanOnOf.Text = "The Fan is on.";
            }
            else
            {
                FanBtnOnOff.Content = "Turn Off";
                FanOnOf.Text = "The Fan is off.";
            }
            _fanState = !_fanState;
            _previousFanState = _fanState;
        }
    }
}

