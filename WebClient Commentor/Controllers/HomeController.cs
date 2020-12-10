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
            Dashboard();
            
            return View(vehiclesToDisplay);
        }

        public ActionResult Video_stream()
        {
            return View();
        }

        public ActionResult KameraOversigt()
        {
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
            IEnumerable<string> selectHour = SelectHourStamp(vehicle);
            IEnumerable<string> selectDay = SelectCurrentDays(vehicle);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SortBetweenWeeks()
        {
            List<Vehicle> vehicles = dbVehicles.LoopThroughWeeks();
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicles);
            IEnumerable<string> selectHour = SelectHourStamp(vehicles);
            IEnumerable<string> selectDay = SelectCurrentWeekNumber(vehicles);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SortBetweenDaysAndHours(string startHour, string endHour, string startDate, string endDate)
        {
            List<Vehicle> vehicles = dbVehicles.getSortedVehiclesDayAndHours(startHour, endHour, startDate, endDate);
            IEnumerable<int> selectAmount = SelectVehicleAmount(vehicles);
            IEnumerable<string> selectHour = SelectHourStamp(vehicles);
            IEnumerable<string> selectDay = SelectCurrentDays(vehicles);
            return Json(new { countSelect = selectAmount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
            
        }
        [Authorize]
        public ActionResult DeleteFromDb(string deleteText = "")
        {
            int toParse = Int32.Parse(deleteText);
            DBAccessVehicles dbvehicles = new DBAccessVehicles();
            dbvehicles.DeleteFromDB(toParse);

            return RedirectToAction("Index");
        }

        public IEnumerable<int> SelectVehicleAmount(List<Vehicle> vehicles)
        {
            IEnumerable<int> VehicleAmount = vehicles.Select(x => x.VehicleAmount);
            return VehicleAmount;
        }

        public IEnumerable<string> SelectHourStamp(List<Vehicle> vehicles)
        {
            List<string> HourStamp = vehicles.Select(x => x.HourStamp).ToList();
            List<string> DateStamp = vehicles.Select(x => x.DateStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while(index != HourStamp.Count())
            {
                string hour = HourStamp[index];
                string date = DateStamp[index];
                string finalResult = "Kl: " + hour + ":00" + " - " + "Dato: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                Console.WriteLine("");
                index++;
            }
            return HourAndDate;
        }

        public IEnumerable<string> SelectCurrentDays(List<Vehicle> vehicles)
        {
            List<string> HourStamp = vehicles.Select(x => x.HourStamp).ToList();
            List<string> DateStamp = vehicles.Select(x => x.DateStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while (index != DateStamp.Count())
            {
                string hour = HourStamp[index];
                string date = DateStamp[index];
                string finalResult = "Kl: " + hour + ":00" + " - " + "Dato: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                index++;
            }
            return HourAndDate;
        }

        public IEnumerable<string> SelectCurrentWeekNumber(List<Vehicle> vehicles)
        {
            List<string> HourStamp = vehicles.Select(x => x.HourStamp).ToList();
            List<string> DateStamp = vehicles.Select(x => x.DateStamp).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while (index != DateStamp.Count())
            {
                string hour = HourStamp[index];
                string date = DateStamp[index];
                string finalResult = "Uge: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                index++;
            }
            return HourAndDate;
        }



        public ActionResult Dashboard()
        {
            IEnumerable<int> VehicleAmount = null;
            IEnumerable<string> HourStamp = null;
            string DateStamp = null;
            DBAccessVehicles dbvehicles = new DBAccessVehicles();

            List<Vehicle> vehiclesByDate = dbvehicles.GetAllVehiclesByLatestDate();
            List<Vehicle> vehiclesBy7Latest = dbvehicles.Get7LatestVehicles();
            List<String> Hours = new List<String>();

            //måske til senere.
            //var VehicleAmount = vehicles.Select(x => x.VehicleAmount).OrderBy(x => x).ToArray()
            int Amount = 0;

            if (vehiclesByDate != null)
            {
                VehicleAmount = SelectVehicleAmount(vehiclesByDate);
                HourStamp = SelectHourStamp(vehiclesByDate);
                DateStamp = vehiclesByDate[vehiclesByDate.Count - 1].DateStamp;
                foreach (var item in HourStamp)
                {
                    Hours.Add(item);
                }
                foreach (var item in DateStamp)
                {
                    Amount++;
                }
            }
            else
            {
                VehicleAmount = SelectVehicleAmount(vehiclesBy7Latest);
                HourStamp = SelectHourStamp(vehiclesBy7Latest);
                DateStamp = vehiclesBy7Latest[vehiclesBy7Latest.Count - 1].DateStamp;
                foreach (var item in HourStamp)
                {
                    Hours.Add(item);
                }
                foreach (var item in DateStamp)
                {
                    Amount++;
                }
            }
            ViewBag.CARCOUNT = VehicleAmount;
            ViewBag.CURRENTHOUR = Hours;
            ViewBag.DateStamp = DateStamp;
            ViewBag.AMOUNT = Amount;
            ViewBag.LOGGEDIN = false;

            return View();
        }
    }
}