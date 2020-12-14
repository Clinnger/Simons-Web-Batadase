using System;

namespace WebClient_Commentor.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string TypeName { get; set; }
        public int VehicleAmount { get; set; }
        public int Feed { get; set; }
        public DateTime DateTimeStamp { get; set; }
        public string DateTime { get; set; }

        public Vehicle(int vehicleId, string typeName, int vehicleAmount, int feed, DateTime dateStamp)
        {
            VehicleId = vehicleId;
            TypeName = typeName;
            VehicleAmount = vehicleAmount;
            Feed = feed;
            DateTimeStamp = dateStamp;
        }

        public Vehicle(int vehicleId, int vehicleAmount, DateTime dateStamp)
        {
            VehicleId = vehicleId;
            VehicleAmount = vehicleAmount;
            DateTimeStamp = dateStamp;
        }

        public Vehicle(int vehicleAmount, DateTime dateStamp)
        {
            VehicleAmount = vehicleAmount;
            DateTimeStamp = dateStamp;
        }

        public Vehicle(int vehicleAmount, string dateTime)
        {
            VehicleAmount = vehicleAmount;
            DateTime = dateTime;
        }

        public Vehicle()
        {

        }
    }
}