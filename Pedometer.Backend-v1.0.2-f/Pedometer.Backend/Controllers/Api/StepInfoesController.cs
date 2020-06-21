using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Pedometer.Data;
using Pedometer.Models;

namespace Pedometer.Controllers.Api
{
    /// <summary>
    /// Pedometer information
    /// </summary>
    public class StepInfoesController : ApiController
    {
        private WebApiContext db = new WebApiContext();

        private static DateTime UnixStartTime = new DateTime(1970, 1, 1);
        /// <summary>
        /// Obtain all data after a given timestamp, pay attention. The actual production environment cannot do this. When the amount of data is huge, 
        /// this will read a large amount of data in the database, which will likely bring down the server.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        // GET: api/StepInfos/5
        public List<StepInfo> Get(long timestamp)
        {

            if (HttpContext.Current.Session?["UserId"] == null)
            {
                return new List<StepInfo>();
            }
            string userId = HttpContext.Current.Session["UserId"] as string;
            var stepInfo = db.StepInfos.Where(s => s.UserId == userId && s.TimeStamp > timestamp)
                .ToList()
                .GroupBy(s => UnixStartTime.AddMilliseconds(s.TimeStamp).ToString("M-d"))
                .Select(g => { return g.ToList().MaxItem(s => s.TodaySteps); })
                .ToList();
            return stepInfo;
        }

        /// <summary>
        /// Upload activity data
        /// </summary>
        /// <param name="stepInfo"></param>
        /// <returns></returns>
        // POST: api/StepInfos
        public IHttpActionResult PostStepInfo(StepInfo stepInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (stepInfo.TodaySteps == 0)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            if (HttpContext.Current.Session?["UserId"] == null)
            {
                return Unauthorized();
            }

            stepInfo.UserId = HttpContext.Current.Session?["UserId"].ToString();
            db.StepInfos.Add(stepInfo);
            db.SaveChanges();

            return Ok("Success");
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

    public class TestController : ApiController
    {
        private WebApiContext db = new WebApiContext();
        private static DateTime UnixStartTime = new DateTime(1970, 1, 1);
        public IHttpActionResult Get()
        {
            if (HttpContext.Current.Session?["UserId"] == null)
            {
                return Unauthorized();
            }
            string userId = HttpContext.Current.Session["UserId"] as string;
            var random = new Random();
            //Generate nearly 30 days of data, 10 items per day
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var steps = random.Next(100, 10000);
                    var stepInfo = new StepInfo
                    {
                        TimeStamp = (long)(DateTime.Now - TimeSpan.FromDays(i) - UnixStartTime).TotalMilliseconds,
                        TodaySteps = steps,
                        Calories = steps * 0.1f,
                        Distance = steps * 0.3f,
                        UserId = userId
                    };
                    db.StepInfos.Add(stepInfo);
                }
            }
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Notification(int threshold)
        {
            try
            {
                if (threshold > 0)
                {
                    BackgroundTask.NotifyThreshold = threshold;
                }
                return Ok(BackgroundTask.Work());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return InternalServerError(e);
            }
            ;
        }
    }
    static class LinqExt
    {
        public static T MaxItem<T>(this ICollection<T> source, Func<T, IComparable> keySelect)
        {
            if (source == null || source.Count == 0)
            {
                return default(T);
            }
            T maxT = source.First();
            foreach (var t in source)
            {
                maxT = keySelect(t).CompareTo(keySelect(maxT)) >= 0 ? t : maxT;
            }

            return maxT;
        }
    }
}