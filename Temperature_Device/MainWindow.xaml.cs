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
using Temperature_Device.Models;

namespace Temperature_Device
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DeviceClient _deviceClient;
        private readonly string _ApiUrl = "http://localhost:7177/api/devices/connect";
        private readonly string _sqlconnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\pelle\\source\\repos\\SmartApplication\\Temperature_Device\\Data\\Temp_Db.mdf;Integrated Security=True;Connect Timeout=30";
        private bool _connected = false;
        private string _deviceId;
        private DeviceInformation _deviceInformation;
        private int _previousTemp;
        private int _previousHumidity;
        private  static Random _random = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (_random) // synchronize
            {
                return _random.Next(min, max);
            }
        }

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
            tbConnectingMessage.Text = "Initializing Device, Please wait....";

            await Task.Delay(5000);

            using IDbConnection connection = new SqlConnection(_sqlconnectionString);
            _deviceId = await connection.QueryFirstOrDefaultAsync<string>("SELECT DeviceId FROM Device_Information");
            if (string.IsNullOrEmpty(_deviceId))
            {
                tbConnectingMessage.Text = "Generating new DeviceId";
                _deviceId = "Kitchen-Thermometer";
                await connection.ExecuteAsync(
                    "INSERT INTO Device_Information (DeviceId, DeviceType, DeviceName, Location, Owner) " +
                    "VALUES (@DeviceId, @DeviceType, @DeviceName, @Location, @Owner)",
                    new
                    {
                        DeviceId = _deviceId, DeviceType = "Sensor", DeviceName = "KitchenThermometer",
                        Location = "KitchenEntrance", Owner = "Per" 
                    });
            }

            var deviceConnectionString =
                await connection.QueryFirstOrDefaultAsync<string>("SELECT ConnectionString FROM Device_Information WHERE DeviceId = @DeviceId",
                    new { DeviceId = _deviceId });
            if (string.IsNullOrEmpty(deviceConnectionString))
            {
                tbConnectingMessage.Text = "Initializing ConnectionString, Please Wait.....";
                using var http = new HttpClient();
                var result = await http.PostAsJsonAsync(_ApiUrl, new {DeviceId = _deviceId});
                deviceConnectionString = await result.Content.ReadAsStringAsync();
                await connection.ExecuteAsync(
                    "UPDATE Device_Information SET ConnectionString = @ConnectionString WHERE DeviceId = @DeviceId",
                    new {DeviceId = _deviceId, ConnectionString = deviceConnectionString});
            }
            _deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);

            tbConnectingMessage.Text = "Updating Device Twin Properties, Please Wait.....";

            _deviceInformation = await connection.QueryFirstOrDefaultAsync<DeviceInformation>(
                "SELECT * FROM Device_Information where DeviceId = @DeviceId",
                new {DeviceId = _deviceId});
            var twinCollection = new TwinCollection();
            twinCollection["owner"] = _deviceInformation.Owner;
            twinCollection["deviceName"] = _deviceInformation.DeviceName;
            twinCollection["deviceType"] = _deviceInformation.DeviceType;
            twinCollection["location"] = _deviceInformation.Location;

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
                    loopLog.Text = ("I'm Looping \n");
                    int currentTemp = GetRandomNumber(18, 21);
                    int currentHumidity = GetRandomNumber(40, 60);
                    var twinCollection = new TwinCollection();
                    if (_previousTemp != currentTemp)
                    {
                        twinCollection["temperature"] = currentTemp + "°C";
                       loopLog.Text += ($"{DateTime.Now}: Current temperature is {currentTemp}, Previously it was {_previousTemp}\n");
                        _previousTemp = currentTemp;
                    }

                    if (_previousHumidity != currentHumidity)
                    {
                        twinCollection["humidity"] = currentHumidity + "%"; 
                        loopLog.Text += ($"{DateTime.Now}: Current Humidity is {currentHumidity}, Previously it was {_previousHumidity}\n");
                        _previousHumidity = currentHumidity;
                    }
                    await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);
                }
                await Task.Delay(20000);
            }

        }

        private void BtnConnect_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
