using Microsoft.Azure.Devices;
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
using SmartApplication.MVVM.Models;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace SmartApplication.Components
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        private static readonly string _iotHubConnectionString = "HostName=CoderPer-IotHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ma+LRFaad+UGhf/jh36X7aYMV2DlhsJ45OLbAnkzkrU=";
        private readonly RegistryManager _registryManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
        private readonly string _connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\pelle\\source\\repos\\SmartApplication\\KitchenLight_Device\\Data\\LightDevices.Db.mdf;Integrated Security=True;Connect Timeout=30";

        public static readonly DependencyProperty StateActiveProperty =
            DependencyProperty.Register("StateActive", typeof(string), typeof(Tile));
        public string StateActive
        {
            get { return (string)GetValue(StateActiveProperty); }
            set { SetValue(StateActiveProperty, value); }
        }

        public static readonly DependencyProperty StateInactiveProperty =
            DependencyProperty.Register("StateInactive", typeof(string), typeof(Tile));
        public string StateInactive
        {
            get { return (string)GetValue(StateInactiveProperty); }
            set { SetValue(StateInactiveProperty, value); }
        }



        public static readonly DependencyProperty DeviceNameProperty =
            DependencyProperty.Register("DeviceName", typeof(string), typeof(Tile));
        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }
        public static readonly DependencyProperty DeviceTypeProperty =
            DependencyProperty.Register("DeviceType", typeof(string), typeof(Tile));
        public string DeviceType
        {
            get { return (string)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }


        public static readonly DependencyProperty isCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(Tile));
        public bool IsChecked
        {
            get { return (bool)GetValue(isCheckedProperty); }
            set { SetValue(isCheckedProperty, value); }
        }

        public static readonly DependencyProperty ActiveIconProperty =
            DependencyProperty.Register("ActiveIcon", typeof(string), typeof(Tile));
        public string ActiveIcon
        {
            get { return (string)GetValue(ActiveIconProperty); }
            set { SetValue(ActiveIconProperty, value); }
        }

        public static readonly DependencyProperty InActiveIconProperty =
            DependencyProperty.Register("InActiveIcon", typeof(string), typeof(Tile));
        public string InActiveIcon
        {
            get { return (string)GetValue(InActiveIconProperty); }
            set { SetValue(InActiveIconProperty, value); }
        }

        public Tile()
        {
            InitializeComponent();
        }


        private async void BtnRemoveDevice_OnClick(object sender, RoutedEventArgs e)
        {
            
            var button = sender as Button;
            var deviceItem = (DeviceItem) button!.DataContext;
            btnRemoveDevice.IsEnabled = false;
            await _registryManager.RemoveDeviceAsync(deviceItem.DeviceId);
            using IDbConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync($"DELETE FROM Device_Information WHERE DeviceId = @DeviceId", new{DeviceId = deviceItem.DeviceId});

        }

        private async void CheckBoxOnOff_OnClick(object sender, RoutedEventArgs e)
        {
            CheckBoxOnOff.IsEnabled = false;
            var checkbox = sender as CheckBox;
            var deviceItem = (DeviceItem) checkbox!.DataContext;
            if (IsChecked != null)
            {
                using ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(_iotHubConnectionString);

                var directMethod = new CloudToDeviceMethod("ChangedLightState");
                //directMethod.SetPayloadJson(JsonConvert.SerializeObject(new { lightState = IsChecked }));
                var result = await serviceClient.InvokeDeviceMethodAsync(deviceItem.DeviceId, directMethod);
            }
            CheckBoxOnOff.IsEnabled = true;
        }
    }
}
