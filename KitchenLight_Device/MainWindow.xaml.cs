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
using KitchenLight_Device.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace KitchenLight_Device
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DeviceClient _deviceClient;
        private readonly string _ApiUrl = "http://localhost:7177/api/devices/connect";
        private readonly string _connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\pelle\\source\\repos\\SmartApplication\\KitchenLight_Device\\Data\\LightDevices.Db.mdf;Integrated Security=True;Connect Timeout=30";
        private int _Intervall = 1000;
        private bool _lightstate = true;
        private bool _previouslightState = false;
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
                _deviceId = "IntelliLight";
                await connection.ExecuteAsync(
                    "INSERT INTO Device_Information (DeviceId, DeviceType, DeviceName, Location, Owner) " +
                    "VALUES (@DeviceId, @DeviceType, @DeviceName, @Location, @Owner)",
                    new
                    {
                        DeviceId = _deviceId,
                        DeviceType = "Light",
                        DeviceName = "Kitchen Lamp",
                        Location = "Kitchen",
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
            twinCollection["lightState"] = _lightstate;
            twinCollection["previousLightState"] = _previouslightState;

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
                    if (_lightstate != _previouslightState)
                    {

                        _lightstate = !_lightstate;
                        //d2c device 2 cloud
                        var json = JsonConvert.SerializeObject(new { lightState = _lightstate });
                        var message = new Message(Encoding.UTF8.GetBytes(json));
                        message.Properties.Add("deviceName", _deviceInformation.DeviceName);
                        message.Properties.Add("deviceType", _deviceInformation.DeviceType);
                        message.Properties.Add("owner", _deviceInformation.Owner);
                        message.Properties.Add("location", _deviceInformation.Location);

                        await _deviceClient.SendEventAsync(message);
                        tbConnectingMessage.Text = $"Message sent at {DateTime.Now}.";
                        //device twin reported
                        var twinCollection = new TwinCollection();
                        twinCollection["lightState"] = _lightstate;
                        twinCollection["previousLightState"] = null;
                        await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);
                    }
                }
                await Task.Delay(_Intervall);
            }

        }

        private void BtnOnOff_OnClick(object sender, RoutedEventArgs e)
        {
            
            if (!_lightstate)
            {
                btnOnOff.Content = "Turn On";
                LightOnOf.Text = "The light is on.";
            }
            else
            {
                btnOnOff.Content = "Turn Off";
                LightOnOf.Text = "The light is off.";
            }
            _lightstate = !_lightstate;
            _previouslightState = _lightstate;

        }
    }
}
