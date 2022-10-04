﻿using SmartApplication.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace SmartApplication.MVVM.ViewModels
{
    internal class KitchenViewModel
    {
        private ObservableCollection<DeviceItem> _devices;
        private ObservableCollection<TemperatureDevice> _temperatureDevices;
        private List<DeviceItem> _removeList = new();
        private DispatcherTimer _timer;
        private readonly RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=CoderPer-IotHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ma+LRFaad+UGhf/jh36X7aYMV2DlhsJ45OLbAnkzkrU=");
        public string Title { get; set; } = "Kitchen";
        public IEnumerable<DeviceItem> DeviceItems => _devices;
        public IEnumerable<TemperatureDevice> TemperatureDevices => _temperatureDevices;
        public NotifyTaskCompletion<TemperatureDevice> tempSensor { get; set; }
        public KitchenViewModel()
        {
            _devices = new ObservableCollection<DeviceItem>();
            //tempSensor = new NotifyTaskCompletion<TemperatureDevice>(PopulateTemperatureSensor());
            PopulateDeviceListAsync().ConfigureAwait(false);
            setTimer(TimeSpan.FromSeconds(5));
        }

        private void setTimer(TimeSpan interval)
        {
            _timer = new DispatcherTimer()
            {
                Interval = interval
            };
            _timer.Tick += new EventHandler(timer_tick);
            _timer.Start();
        }

        private async void timer_tick(object sender, EventArgs e)
        {
            await PopulateDeviceListAsync();
            tempSensor = new NotifyTaskCompletion<TemperatureDevice>(PopulateTemperatureSensor());
            await UpdateDevicesAsync();
        }
        private async Task UpdateDevicesAsync()
        {
            _removeList.Clear();
            foreach (var item in _devices)
            {
                var device = await _registryManager.GetDeviceAsync(item.DeviceId);
                if (device == null)
                    _removeList.Add(item);
            }

            foreach (var item in _removeList)
            {
                _devices.Remove(item);
            }
        }

        private async Task<TemperatureDevice> PopulateTemperatureSensor()
        {
            var result =
                _registryManager.CreateQuery(
                    "SELECT * FROM Devices WHERE properties.reported.deviceType = 'Sensor'");
            var device = new TemperatureDevice();
            foreach (var twin in await result.GetNextAsTwinAsync())
            {
                try { device.TemperatureValue = twin.Properties.Reported["temperature"]; }
                catch { }
                try { device.HumidityValue = twin.Properties.Reported["humidity"]; }
                catch { }
            }

            return device;

        }

        private async Task PopulateDeviceListAsync()
        {
            var result = _registryManager.CreateQuery("SELECT * FROM devices WHERE properties.reported.location = 'Kitchen'");
            if (result.HasMoreResults)
                foreach (var twin in await result.GetNextAsTwinAsync())
                {
                    var device = _devices.FirstOrDefault(x => x.DeviceId == twin.DeviceId);
                    if (device == null)
                    {
                        device = new DeviceItem()
                        {
                            DeviceId = twin.DeviceId
                        };
                        try { device.DeviceName = twin.Properties.Reported["deviceName"]; }
                        catch { device.DeviceName = device.DeviceId; }
                        try { device.DeviceType = twin.Properties.Reported["deviceType"]; }
                        catch { }

                        switch (device.DeviceType.ToLower())
                        {
                            case "fan":
                                device.ActiveIcon = "\uf72e";
                                device.InactiveIcon = "\uf011";
                                device.StateActive = "ON";
                                device.StateInactive = "OFF";
                                break;
                            case "light":
                                device.ActiveIcon = "\uf0eb";
                                device.InactiveIcon = "\uf011";
                                device.StateActive = "ON";
                                device.StateInactive = "OFF";
                                break;
                            default:
                                device.ActiveIcon = "\uf011";
                                device.InactiveIcon = "\uf011";
                                device.StateActive = "ENABLE";
                                device.StateInactive = "DISABLE";
                                break;
                        }
                        _devices.Add(device);
                    }
                    else { }

                }
            else
            {
                _devices.Clear();
            }
        }
    }
}
