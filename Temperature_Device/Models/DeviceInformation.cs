using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Temperature_Device.Models
{
    internal class DeviceInformation
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Location { get; set; }
        public string Owner { get; set; }
        public string ConnectionString { get; set; }
    }
}
