using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Pedometer.Data;
using Pedometer.Models;

namespace Pedometer.Controllers
{
    [Authorize]
    public class UserInfosController : Controller
    {
        private WebApiContext db = new WebApiContext();

        // GET: UserInfos
        public ActionResult Index()
        {
            return View(db.UserInfos.ToList());
        }

        // GET: UserInfos/Details/5
        public ActionResult Details(string uid)
        {
            if (uid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserInfo userInfo = db.UserInfos.Find(uid);
            if (userInfo == null)
            {
                return HttpNotFound();
            }
            return View(userInfo);
        }

        // GET: UserInfos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserInfos/Create
        // To prevent "too many releases" attacks, please enable specific attributes to bind to,
        // For more information, see https://go.microsoft.com/fwlink/?LinkId=317598
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserName,Password,Phone,Height,Weight,DeviceToken")] UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                db.UserInfos.Add(userInfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(userInfo);
        }

        // GET: UserInfos/Edit/5
        public ActionResult Edit(string uid,string t)
        {
            if (uid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserInfo userInfo = db.UserInfos.Find(uid);
            if (userInfo == null)
            {
                return HttpNotFound();
            }
            return View(userInfo);
        }

        // POST: UserInfos/Edit/5
        // To prevent "too many releases" attacks, please enable specific attributes to bind to,
        // For more information, see https://go.microsoft.com/fwlink/?LinkId=317598
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,Password,Phone,Height,Weight,DeviceToken")] UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userInfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userInfo);
        }

        // GET: UserInfos/Delete/5
        public ActionResult Delete(string uid, string t)
        {
            if (uid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserInfo userInfo = db.UserInfos.Find(uid);
            if (userInfo == null)
            {
                return HttpNotFound();
            }
            return View(userInfo);
        }

        // POST: UserInfos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string uid, string t)
        {
            UserInfo userInfo = db.UserInfos.Find(uid);
            db.UserInfos.Remove(userInfo);
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
