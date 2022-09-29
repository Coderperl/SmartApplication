using SmartApplication.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApplication.MVVM.ViewModels
{
    internal class BedRoomViewModel
    {
        public string Title { get; set; } = "Bed Room";
        public string Temperature { get; set; } = "18";
        public string TemperatureScale { get; set; } = "°C";
        public string Humidity { get; set; } = "29";
        public string HumidityScale { get; set; } = "%";
        public List<DeviceItem> Devices { get; set; } = new List<DeviceItem>();
    }
}
