using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebClient_Commentor.Models;
using WebClient_Commentor.DB;
using Antlr.Runtime.Tree;

namespace WebClient_Commentor.Controllers
{
    public class HomeController : Controller
    {
        DBAccessCars dbCars = new DBAccessCars();
        
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

            DBAccessCars dbcars = new DBAccessCars();
            List<Cars> carsToDisplay = dbcars.getAllCars();
            Dashboard();
            
            return View(carsToDisplay);
        }

        public ActionResult Video_stream()
        {
            

            return View();
        }

        public ActionResult KameraOversigt()
        {
            DBAccessCars dbcars = new DBAccessCars();
            List<Cars> carsBy7Latest = dbcars.Get7LatestCars();
            ViewBag.HEYHEYSIMON = carsBy7Latest;

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
            List<Cars> cars = dbCars.getSortedCarsHour(startHour, endHour);
            IEnumerable<int> selectCount = SelectCarCount(cars);
            IEnumerable<string> selectHour = SelectCurrentHours(cars);
            return Json(new { countSelect = selectCount, hourSelect = selectHour }, JsonRequestBehavior.AllowGet);
        }*/

        public JsonResult SortBetweenDays(string startDate, string endDate)
        {
            List<Cars> cars = dbCars.getSortedCarsDay(startDate, endDate);
            IEnumerable<int> selectCount = SelectCarCount(cars);
            IEnumerable<string> selectHour = SelectCurrentHours(cars);
            IEnumerable<string> selectDay = SelectCurrentDays(cars);
            return Json(new { countSelect = selectCount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SortBetweenWeeks()
        {
            List<Cars> cars = dbCars.LoopThroughWeeks();
            IEnumerable<int> selectCount = SelectCarCount(cars);
            IEnumerable<string> selectHour = SelectCurrentHours(cars);
            IEnumerable<string> selectDay = SelectCurrentWeekNumber(cars);
            return Json(new { countSelect = selectCount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SortBetweenDaysAndHours(string startHour, string endHour, string startDate, string endDate)
        {
            List<Cars> cars = dbCars.getSortedCarsDayAndHours(startHour, endHour, startDate, endDate);
            IEnumerable<int> selectCount = SelectCarCount(cars);
            IEnumerable<string> selectHour = SelectCurrentHours(cars);
            IEnumerable<string> selectDay = SelectCurrentDays(cars);
            return Json(new { countSelect = selectCount, hourSelect = selectHour, daySelect = selectDay }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteFromDb(string deleteText = "")
        {
            int toParse = Int32.Parse(deleteText);
            DBAccessCars dbcars = new DBAccessCars();
            dbcars.DeleteFromDB(toParse);

            return RedirectToAction("Index");
        }

        public IEnumerable<int> SelectCarCount(List<Cars> cars)
        {
            IEnumerable<int> CarCount = cars.Select(x => x.CarCount);
            return CarCount;
        }

        public IEnumerable<string> SelectCurrentHours(List<Cars> cars)
        {
            List<string> CurrentHour = cars.Select(x => x.CurrentHour).ToList();
            List<string> CurrentDate = cars.Select(x => x.CurrentDate).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while(index != CurrentHour.Count())
            {
                string hour = CurrentHour[index];
                string date = CurrentDate[index];
                string finalResult = "Kl: " + hour + ":00" + " - " + "Dato: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                Console.WriteLine("");
                index++;
            }
            return HourAndDate;
        }

        public IEnumerable<string> SelectCurrentDays(List<Cars> cars)
        {
            List<string> CurrentHour = cars.Select(x => x.CurrentHour).ToList();
            List<string> CurrentDate = cars.Select(x => x.CurrentDate).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while (index != CurrentDate.Count())
            {
                string hour = CurrentHour[index];
                string date = CurrentDate[index];
                string finalResult = "Kl: " + hour + ":00" + " - " + "Dato: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                index++;
            }
            return HourAndDate;
        }

        public IEnumerable<string> SelectCurrentWeekNumber(List<Cars> cars)
        {
            List<string> CurrentHour = cars.Select(x => x.CurrentHour).ToList();
            List<string> CurrentDate = cars.Select(x => x.CurrentDate).ToList();
            IEnumerable<string> HourAndDate = new List<string>();

            int index = 0;
            while (index != CurrentDate.Count())
            {
                string hour = CurrentHour[index];
                string date = CurrentDate[index];
                string finalResult = "Uge: " + date;
                HourAndDate = HourAndDate.Concat(new[] { finalResult });
                index++;
            }
            return HourAndDate;
        }



        public ActionResult Dashboard()
        {
            IEnumerable<int> CarCount = null;
            IEnumerable<string> CurrentHour = null;
            string CurrentDate = null;
            DBAccessCars dbcars = new DBAccessCars();

            List<Cars> carsByDate = dbcars.GetAllCarsByLatestDate();
            List<Cars> carsBy7Latest = dbcars.Get7LatestCars();
            List<String> Hours = new List<String>();

            //måske til senere.
            //var CarCount = cars.Select(x => x.CarCount).OrderBy(x => x).ToArray()
            int Amount = 0;

            if (carsByDate != null)
            {
                CarCount = SelectCarCount(carsByDate);
                CurrentHour = SelectCurrentHours(carsByDate);
                CurrentDate = carsByDate[carsByDate.Count - 1].CurrentDate;
                foreach (var item in CurrentHour)
                {
                    Hours.Add(item);
                }
                foreach (var item in CurrentDate)
                {
                    Amount++;
                }
            }
            else
            {
                CarCount = SelectCarCount(carsBy7Latest);
                CurrentHour = SelectCurrentHours(carsBy7Latest);
                CurrentDate = carsBy7Latest[carsBy7Latest.Count - 1].CurrentDate;
                foreach (var item in CurrentHour)
                {
                    Hours.Add(item);
                }
                foreach (var item in CurrentDate)
                {
                    Amount++;
                }
            }
            
            ViewBag.CARCOUNT = CarCount;
            ViewBag.CURRENTHOUR = Hours;
            ViewBag.CURRENTDATE = CurrentDate;
            ViewBag.AMOUNT = Amount;

            return View();
        }
    }
}