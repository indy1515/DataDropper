using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Geolocation;

namespace FileDropper
{
    public class FileData
    {

        private readonly string PHOTO = "image";
        private readonly string VIDEO = "video";
        private readonly string FILE = "file";
        public FileData()
        {

        }
        public FileData(String name, String lat, String lng, String dropBy, String fileName)
        {
            this.Name = name;
            this.Position = new Geopoint(new BasicGeoposition()
            {
                Latitude = Convert.ToDouble(lat),
                Longitude = Convert.ToDouble(lng)
            });
            this.DropBy = dropBy;
            this.FileName = fileName;
        }

        public FileData(JsonObject jsonObject)
            : this(jsonObject["name"].GetString(), jsonObject["x"].GetString(), jsonObject["y"].GetString(),
            jsonObject["dropby"].GetString(), jsonObject["file"].GetString())
        {

        }

        public String Name { get; private set; }
        public Geopoint Position { get; private set; }
        public String DropBy { get; private set; }
        public String FileName { get; private set; }
        public String FileType
        {
            get
            {
                string type = this.FILE;
                string typeName = FileName.Substring(FileName.IndexOf('.') + 1);
                switch (typeName)
                {
                    case "gif":
                    case "raw":
                    case "tiff":
                    case "png": 
                    case "jpg":
                    case "jpeg":
                        type = this.PHOTO; break;
                    case "mp4":
                    case "wav":
                    case "avi":
                    case "mkv":
                    case "ogg":
                    case "wmv":
                    case "mp3":
                        type = this.VIDEO; break;
                    case "docx": 
                    default: 
                        type = this.FILE; break;
                }

                return type;
            }
        }
    }
}
