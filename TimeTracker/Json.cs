using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace TimeTracker
{
    static class Json
    {
        private static string filePath = @"C:\Temp\TimeTrackerData.json";

        ///  ###################### JSON ################################################
        public static void writeToJson(List<Task> allTasks)
        {
            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, allTasks);
            }
        }

        public static List<Task> readFromJson()
        {
            if (File.Exists(filePath))
            {
                using (StreamReader file = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    List<Task> allData = (List<Task>)serializer.Deserialize(file, typeof(List<Task>));
                    if (allData != null)
                        return allData;
                }
            }
            return null;
        }
    }
}
