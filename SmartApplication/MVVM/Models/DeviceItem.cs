using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApplication.MVVM.Models
{
    internal class DeviceItem
    {
        public string DeviceId { get; set; } = "";
        public string DeviceName { get; set; } = "";
        public string DeviceType { get; set; } = "";

        public string ActiveIcon { get; set; } = "";
        public string InactiveIcon { get; set; } = "";
        public string StateActive { get; set; } = "";
        public string StateInactive { get; set; } = "";
    }
}
