using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using jailOtomationSystem.Models;
using jailOtomationSystem.viewModels;

namespace jailOtomationSystem.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        JailEntities db = new JailEntities();
        // GET: Public
        public ActionResult Index()
      {
            int totalOfficers = (from item in db.officers
                                 select item).Count();

            int totalPrisoners = (from item in db.Prisoners
                                  select item).Count();

            int prisonCapacity = (from item in db.prisonerCells
                                  select item.capacity).Sum();

            int availableOfficerJobs = db.Jobs.Where(a => a.workerType == "prisoner")
                     .Sum(a => a.availablePositionsCount);

            statisticsViewModel viewModel = new statisticsViewModel();
            viewModel.availableOfficersJobs = availableOfficerJobs;
            viewModel.prisonCapacity = prisonCapacity;
            viewModel.totalOfficers = totalOfficers;
            viewModel.totalPrisoners = totalPrisoners;


            return View(viewModel);
        }

        public ActionResult Contact()
        { 
            return View();
        }

        public ActionResult Search(statisticsViewModel statisticsView) {

            int prisonerid = statisticsView.prisonerid;

            var prisoner = (from item in db.Prisoners
                            where item.prisonerID == prisonerid
                            select item).FirstOrDefault();

            Session["prisonerID"] = prisonerid;

            if (prisoner != null)
                return Redirect("~/public/ApplyForAvisit");
            else
                return Redirect("~/public/index");
        }

        [HttpGet]
        public ActionResult ApplyForAvisit()
        {

            int numberOfVisitsAtTheSameTime = 2;

            /*            select* from times
                                  where time not in(
                                  select visitTime from visitTimes
                                  where num >= 2)

             */

            /*        visit time view:
                 
                           SELECT        dateOfVisit, VisitTime, COUNT(*) AS num
                           FROM            dbo.visit
                           GROUP BY dateOfVisit, VisitTime
            */
           var  availableAppointments = db.GetAvailableAppointments(numberOfVisitsAtTheSameTime).ToList();

            VisitsViewModel visitView = new VisitsViewModel();
            visitView.availableAppointments = availableAppointments;


            return View(visitView);
        }

        [HttpPost]
        public ActionResult ApplyForAvisit(VisitsViewModel visitViewModel)
        {
            int prisonerid = Convert.ToInt32(Session["prisonerid"]);

            var prisoner = (from item in db.Prisoners
                            where item.prisonerID == prisonerid
                            select item).FirstOrDefault();

            visitViewModel.visit.VisitTime = visitViewModel.selectedAppointment;
            visit visit = new visit();
            visit = visitViewModel.visit;
            visit.Prisoner = prisoner;
            visit.prisonerid = prisonerid;

            db.Entry(visit).State = EntityState.Added;
            db.SaveChanges();
             
            return Redirect("~/public/index");
        }
    }
}