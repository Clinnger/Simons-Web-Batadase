using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebClient_Commentor.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string TypeName { get; set; }
        public int VehicleAmount { get; set; }
        public int Feed { get; set; }
        public string DateStamp { get; set; }
        public int WeekNumber { get; set; }
        public string HourStamp { get; set; }

        public Vehicle(int vehicleId, string typeName, int vehicleAmount, int feed, string dateStamp, int weekNumber, string hourStamp)
        {
            VehicleId = vehicleId;
            TypeName = typeName;
            VehicleAmount = vehicleAmount;
            Feed = feed;
            DateStamp = dateStamp;
            WeekNumber = weekNumber;
            HourStamp = hourStamp;
        }

        public Vehicle(int vehicleId, int vehicleAmount, string dateStamp, string hourStamp)
        {
            VehicleId = vehicleId;
            VehicleAmount = vehicleAmount;
            DateStamp = dateStamp;
            HourStamp = hourStamp;

        }

        public Vehicle(int vehicleAmount, string dateStamp, string hourStamp)
        {
            VehicleAmount = vehicleAmount;
            DateStamp = dateStamp;
            HourStamp = hourStamp;
        }

        public Vehicle()
        {

        }

    }
}