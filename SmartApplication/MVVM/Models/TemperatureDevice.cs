using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApplication.MVVM.Models
{
    public class TemperatureDevice
    {
        public string DeviceId { get; set; } = "";
        public string DeviceName { get; set; } = "";
        public string DeviceType { get; set; } = "";
        public string TemperatureValue { get; set; } = "";
        public string HumidityValue { get; set; } = "";
    }
}
