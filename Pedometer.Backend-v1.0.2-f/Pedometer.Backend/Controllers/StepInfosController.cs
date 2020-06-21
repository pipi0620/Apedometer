using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Pedometer.Data;
using Pedometer.Models;

namespace Pedometer.Controllers
{
    [Authorize]
    public class StepInfosController : Controller
    {
        private WebApiContext db = new WebApiContext();

        // GET: StepInfos
        public ActionResult Index()
        {
            return View(db.StepInfos.ToList());
        }

        // GET: StepInfos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StepInfo stepInfo = db.StepInfos.Find(id);
            if (stepInfo == null)
            {
                return HttpNotFound();
            }
            return View(stepInfo);
        }

        // GET: StepInfos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StepInfos/Create
        // To prevent "too many releases" attacks, please enable specific attributes to bind to,
        // For more information, see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,TimeStamp,TodaySteps,Distance,Calories")] StepInfo stepInfo)
        {
            if (ModelState.IsValid)
            {
                db.StepInfos.Add(stepInfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stepInfo);
        }

        // GET: StepInfos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StepInfo stepInfo = db.StepInfos.Find(id);
            if (stepInfo == null)
            {
                return HttpNotFound();
            }
            return View(stepInfo);
        }

        // POST: StepInfos/Edit/5
        // To prevent "too many releases" attacks, please enable specific attributes to bind to,
        // For more information, see https://go.microsoft.com/fwlink/?LinkId=317598
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,TimeStamp,TodaySteps,Distance,Calories")] StepInfo stepInfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stepInfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stepInfo);
        }

        // GET: StepInfos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StepInfo stepInfo = db.StepInfos.Find(id);
            if (stepInfo == null)
            {
                return HttpNotFound();
            }
            return View(stepInfo);
        }

        // POST: StepInfos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StepInfo stepInfo = db.StepInfos.Find(id);
            db.StepInfos.Remove(stepInfo);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
