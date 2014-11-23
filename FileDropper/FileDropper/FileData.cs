using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Geolocation;

namespace FileDropper
{
    public class FileList
    {
        public FileList(JsonArray jsonArray, Geopoint point)
        {
            DataList = new List<FileData>();
            Debug.WriteLine(jsonArray.Count);
            foreach (JsonValue value in jsonArray)
            {
                try
                {
                    Debug.WriteLine("Foreach FileValue: " + value.Stringify());
                    JsonObject jsonObject = value.GetObject();
                    Debug.WriteLine("Foreach FileList: " + jsonObject.Stringify());
                    DataList.Add(new FileData(jsonObject));
                }
                catch (ArgumentException e1)
                {
                    Debug.WriteLine("ArgumentException");
                }
            }
            Debug.WriteLine("Start Refresh");
            refreshNearestFile(point);
        }

        public FileList()
            : this(new JsonArray(), new Geopoint(new BasicGeoposition
                {
                    Latitude = 0,
                    Longitude = 0

                }))
        {
            // TODO: Complete member initialization
        }
        public void refreshNearestFile(Geopoint point)
        {
            if (DataList != null)
            {
                Debug.WriteLine("Start Initiate");
                FileData nearest = new FileData();
                Debug.WriteLine("After Initiate");
                double distance_temp = Double.PositiveInfinity;
                foreach (FileData file in DataList)
                {
                    Debug.WriteLine("Loop FileData");
                    double file_distance = CompassPage.getdistancebtw(file.Position, point);
                    if (file_distance < distance_temp)
                    {
                        distance_temp = file_distance;
                        nearest = file;
                    }
                }
                Debug.WriteLine("Before Assign Nearest");
                NearestFile = nearest;
                Debug.WriteLine("After Assign Nearest");
            }
        }
        public List<FileData> DataList { get; private set; }
        public FileData NearestFile { get; private set; }
        
    }
    public class FileData
    {

        public static readonly string PHOTO = "image";
        public static readonly string VIDEO = "video";
        public static readonly string FILE = "file";
        public FileData():this("File Not Found","0","0","Annonymous","Error")
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
                string type = FileData.FILE;
                string typeName = FileName.Substring(FileName.IndexOf('.') + 1);
                switch (typeName)
                {
                    case "gif":
                    case "raw":
                    case "tiff":
                    case "png": 
                    case "jpg":
                    case "jpeg":
                        type = FileData.PHOTO; break;
                    case "mp4":
                    case "wav":
                    case "avi":
                    case "mkv":
                    case "ogg":
                    case "wmv":
                    case "mp3":
                        type = FileData.VIDEO; break;
                    case "docx": 
                    default: 
                        type = FileData.FILE; break;
                }

                return type;
            }
        }
    }
}
