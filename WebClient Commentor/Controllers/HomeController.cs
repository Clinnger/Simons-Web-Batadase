using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebClient_Commentor.Models;
using WebClient_Commentor.DB;
using Antlr.Runtime.Tree;
using System.Windows.Forms;

namespace WebClient_Commentor.Controllers
{
    public class HomeController : Controller
    {
        DBAccessVehicles dbVehicles = new DBAccessVehicles();
        
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    
        public ActionResult Index()
        {

            DBAccessVehicles dbvehicles = new DBAccessVehicles();
            List<Vehicle> vehiclesToDisplay = dbvehicles.getAllVehicles();
            IEnumerable<int> VehicleAmount = null;
            IEnumerable<string> HourStamp = null;
            string TypeName = null;
            string DateStamp = null;

            //List<Vehicle> vehiclesByDate = dbvehicles.GetAllVehiclesByLatestDate();
            List<Vehicle> vehiclesBy7Latest = dbvehicles.GetAllLatestVehiclesFromLatestHour();
            List<string> Hours = new List<string>();

            //måske til senere.
            //var VehicleAmount = vehicles.Select(x => x.VehicleAmount).OrderBy(x => x).ToArray()
            int Amount = 0;

            VehicleAmount = SelectVehicleAmount(vehiclesBy7Latest);
            HourStamp = SelectHourStampStrings(vehiclesBy7Latest, "Dato", false);
            DateStamp = vehiclesBy7Latest[vehiclesBy7Latest.Count - 1].DateTime;
            foreach (var item in HourStamp)
            {
                Hours.Add(item);
            }
            /*
            foreach (var item in DateStamp)
            {
                Amount++;
            }
            */
            
            ViewBag.CARCOUNT = VehicleAmount;
            ViewBag.CURRENTHOUR = Hours;
            ViewBag.DateStamp = DateTime.Now.ToString();
            ViewBag.AMOUNT = Amount;
            ViewBag.LOGGEDIN = false;
            ViewBag.VEHICLETYPE = TypeName;

            return View(vehiclesToDisplay);
        }

        public ActionResult Video_stream()
        {
            

            return View();
        }

        public ActionResult KameraOversigt()
        {
            DBAccessVehicles dbcars = new DBAccessVehicles();
            Vehicle LatestCar = dbcars.GetAllLatestVehiclesFromLatestHour()[0];
            ViewBag.HEYHEYSIMON = LatestCar.VehicleAmount;

            return View();
        }

        public ActionResult LiveStreamVideo()
        {
            return View();
        }

        // Bruges ikke lige nu
        /*
        public JsonResult SortBetweenHours(string startHour, string endHour)
        {
            List<Vehicles> vehicles = dbVehicles.getSortedVehiclesHour(startHour, endHour);
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicles);
            IEnumerable<string> selectHour = SelectHourStamp(vehicles);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour }, JsonRequestBehavior.AllowGet);
        }*/

        public JsonResult SortBetweenDays(string startDate, string endDate)
        {
            List<Vehicle> vehicle = dbVehicles.getSortedVehiclesDay(startDate, endDate);
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicle);
            IEnumerable<string> selectHour = SelectHourStampStrings(vehicle, "Dato", false);
            IEnumerable<string> selectDay = SelectCurrentDays(vehicle);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult SortBetweenWeeks(string startDate, string endDate)
        {
            List<Vehicle> vehicles = dbVehicles.SortByWeeks(startDate, endDate);
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicles);
            IEnumerable<string> selectHour = SelectHourStampStrings(vehicles, "Uge", false);
            IEnumerable<string> selectDay = SelectCurrentWeekNumber(vehicles);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }
        

        public JsonResult SortBetweenDaysAndHours(string startHour, string endHour, string startDate, string endDate, string vehicleType)
        {
            List<Vehicle> vehicles = dbVehicles.getSortedVehiclesDayAndHours(startHour, endHour, startDate, endDate, vehicleType);
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicles);
            IEnumerable<string> selectHour = SelectHourStampStrings(vehicles, "Dato", true);
            IEnumerable<string> selectDay = SelectCurrentDays(vehicles);
            IEnumerable<string> selectVehiType = SelectVehicleType(vehicles);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay, vehicleSelect = selectVehiType }, JsonRequestBehavior.AllowGet);
            
        }
        public ActionResult DeleteFromDb(string deleteText = "")
        {
            int toParse = Int32.Parse(deleteText);
            DBAccessVehicles dbvehicles = new DBAccessVehicles();
            dbvehicles.DeleteFromDB(toParse);

            return RedirectToAction("Index");
        }

        public JsonResult GenerateTestDataVehicles()
        {
            DBAccessVehicles testData = new DBAccessVehicles();
            testData.GenerateTestDataVehicles();
            string startDate = "10 Dec 2020";
            string endDate = "11 Dec 2020";
            List<Vehicle> vehicle = dbVehicles.GetAllLatestVehiclesFromLatestHour();
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicle);
            IEnumerable<string> selectHour = SelectHourStamp(vehicle);
            IEnumerable<string> selectDay = SelectCurrentDays(vehicle);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FindVehicleType()
        {

            int vehicles = dbVehicles.FindVehicleType("Car");
            
            return Json(new { test = vehicles }, JsonRequestBehavior.AllowGet);

        }

        public IEnumerable<int> SelectVehicleAmount(List<Vehicle> vehicles)
        {
            IEnumerable<int> VehicleAmount = vehicles.Select(x => x.VehicleAmount);
            return VehicleAmount;
        }

        public IEnumerable<string> SelectHourStamp(List<Vehicle> vehicles)
        {
            //List<string> HourStamp = vehicles.Select(x => x.HourStamp).ToList();
            List<DateTime> DateStamp = vehicles.Select(x => x.DateTimeStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while(index != DateStamp.Count())
            {
                //string hour = HourStamp[index];
                DateTime date = DateStamp[index];
                string finalResult = "Dato: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                index++;
            }
            return HourAndDate;
        }

        public IEnumerable<string> SelectHourStampStrings(List<Vehicle> vehicles, string title, bool check)
        {
            List<string> HourStamp = vehicles.Select(x => x.HourToGet).ToList();
            List<string> DateStamp = vehicles.Select(x => x.DateTime).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while (index != DateStamp.Count())
            {
                string date = DateStamp[index];
                string hour = HourStamp[index];
                string finalResult = "";
                if(check)
                {
                    finalResult = title + ": " + date + " Kl:" + hour;
                    HourAndDate = HourAndDate.Concat(new[] { finalResult });
                    index++;
                }
                else
                {
                    finalResult = title + ": " + date;
                    HourAndDate = HourAndDate.Concat(new[] { finalResult });
                    index++;
                }
                
            }
            return HourAndDate;
        }

        public IEnumerable<string> SelectCurrentDays(List<Vehicle> vehicles)
        {
            //List<string> HourStamp = vehicles.Select(x => x.HourStamp).ToList();
            List<DateTime> DateStamp = vehicles.Select(x => x.DateTimeStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();
            int index = 0;
            while (index != DateStamp.Count())
            {
                //string hour = HourStamp[index];
                DateTime date = DateStamp[index];
                string finalResult = "Dato: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                index++;
            }
            return HourAndDate;
        }
        
        public IEnumerable<string> SelectVehicleType(List<Vehicle> vehicles)
        {
            IEnumerable<string> VehicleTypeName = vehicles.Select(x => x.TypeName);
            return VehicleTypeName;
        }

        public IEnumerable<string> SelectCurrentWeekNumber(List<Vehicle> vehicles)
        {
            //List<string> HourStamp = vehicles.Select(x => x.HourStamp).ToList();
            List<DateTime> DateStamp = vehicles.Select(x => x.DateTimeStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while (index != DateStamp.Count())
            {
                //string hour = HourStamp[index];
                DateTime date = DateStamp[index];
                string finalResult = "Uge: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                index++;
            }
            return HourAndDate;
        }
    }
}