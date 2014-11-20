using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace FileDropper
{
    public static class DeviceInfo
    {
        private static EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();

        public static bool IsRunningOnEmulator
        {
            get
            {
                return (deviceInfo.SystemProductName == "Virtual");
            }
        }
    }
}
