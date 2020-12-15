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
            IEnumerable<int> VehicleAmount = null;
            IEnumerable<string> HourStamp = null;
            string TypeName = null;
            string DateStamp = null;

            List<Vehicle> latestVehicles = dbvehicles.GetAllLatestVehiclesFromLatestDate();
            List<string> Hours = new List<string>();

            //måske til senere.
            //var VehicleAmount = vehicles.Select(x => x.VehicleAmount).OrderBy(x => x).ToArray()
            int Amount = 0;

            VehicleAmount = SelectVehicleAmount(latestVehicles);
            HourStamp = SelectHourStampStrings(latestVehicles, "Dato", true);
            DateStamp = latestVehicles[latestVehicles.Count - 1].DateTime;
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

            return View(latestVehicles);
        }

        public ActionResult Video_stream()
        {
            

            return View();
        }

        public ActionResult KameraOversigt()
        {
            DBAccessVehicles dbcars = new DBAccessVehicles();
            Vehicle LatestCar = dbcars.GetAllLatestVehiclesFromLatestDate()[0];
            ViewBag.HEYHEYSIMON = dbcars.GetVehiclesForMapOverview();

            return View();
        }

        public ActionResult LiveStreamVideo()
        {
            return View();
        }


        public JsonResult SortBetweenDays(string startDate, string endDate, string VehicleType)
        {
            List<Vehicle> vehicle = dbVehicles.getSortedVehiclesDay(startDate, endDate, VehicleType);
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicle);
            IEnumerable<string> selectHour = SelectHourStampStrings(vehicle, "Dato", false);
            IEnumerable<string> selectDay = SelectCurrentDays(vehicle);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult SortBetweenWeeks(string startDate, string endDate, string VehicleType)
        {
            List<Vehicle> vehicles = dbVehicles.SortByWeeks(startDate, endDate, VehicleType);
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
            List<Vehicle> vehicle = dbVehicles.GetAllLatestVehiclesFromLatestDate();
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicle);
            IEnumerable<string> selectHour = SelectHourStampStrings(vehicle, "Dato", true);
            IEnumerable<string> selectDay = SelectCurrentDays(vehicle);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<int> SelectVehicleAmount(List<Vehicle> vehicles)
        {
            IEnumerable<int> VehicleAmount = vehicles.Select(x => x.VehicleAmount);
            return VehicleAmount;
        }

        public IEnumerable<string> SelectHourStamp(List<Vehicle> vehicles)
        {
            List<DateTime> DateStamp = vehicles.Select(x => x.DateTimeStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while(index != DateStamp.Count())
            {
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
            List<DateTime> DateStamp = vehicles.Select(x => x.DateTimeStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();
            int index = 0;
            while (index != DateStamp.Count())
            {
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
            List<DateTime> DateStamp = vehicles.Select(x => x.DateTimeStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while (index != DateStamp.Count())
            {
                DateTime date = DateStamp[index];
                string finalResult = "Uge: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                index++;
            }
            return HourAndDate;
        }
    }
}