using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;

namespace FileDropper
{
    public class LocationFile
    {
        public LocationFile()
        {

        }
        public LocationFile(StorageFile file, Geoposition position)
        {
            this.File = file;
            this.Position = position;
        }
        public StorageFile File { get; private set; }
        public Geoposition Position { get; private set; }
    }
}
