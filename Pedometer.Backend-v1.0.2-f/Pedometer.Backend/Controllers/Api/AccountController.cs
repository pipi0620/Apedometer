using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Pedometer.Data;
using Pedometer.Models;

namespace Pedometer.Controllers.Api
{



    /// <summary>
    /// account information.
    /// </summary>
    public class AccountController : ApiController
    {
        private WebApiContext db = new WebApiContext();


        // GET: api/Account
        [ResponseType(typeof(UserInfo))]
        public IHttpActionResult Get()
        {
            if (HttpContext.Current.Session?["UserId"] == null)
            {
                return Unauthorized();
            }

            UserInfo userInfo = db.UserInfos.Find(HttpContext.Current.Session?["UserId"]);
            if (userInfo == null)
            {
                return NotFound();
            }

            return Ok(userInfo);
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        // PUT: api/Account/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUserInfo(string id, UserInfo userInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != userInfo.Id)
            {
                return BadRequest();
            }
            if (HttpContext.Current.Session?["UserId"] == null)
            {
                return Unauthorized();
            }
            if (Equals(HttpContext.Current.Session?["UserId"], id) == false)
            {
                return Unauthorized();
            }

            var entity = db.UserInfos.Find(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.UserName = userInfo.UserName ?? entity.UserName;
            entity.Password = userInfo.Password ?? entity.Password;
            entity.Phone = userInfo.Phone ?? entity.Phone;
            entity.DeviceToken = userInfo.DeviceToken ?? entity.DeviceToken;
            entity.Weight = userInfo.Weight > 0 ? userInfo.Weight : entity.Weight;
            entity.Height = userInfo.Height > 0 ? userInfo.Height : entity.Height;

            db.Entry(entity).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Log-in or registered
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        // POST: api/Account
        [ResponseType(typeof(UserInfo))]
        public IHttpActionResult Post(UserInfo userInfo)
        {
            if (string.IsNullOrWhiteSpace(userInfo.Id) || string.IsNullOrWhiteSpace(userInfo.Password))
            {
                return BadRequest(ModelState);
            }

            var entity = db.UserInfos.Find(userInfo.Id);
            if (entity == null)
            {
                return Register(userInfo);
            }

            if (entity.Password.Equals(userInfo.Password))
            {
                HttpContext.Current.Session["UserId"] = entity.Id;
                entity.DeviceToken = userInfo.DeviceToken;
                db.SaveChanges();

                userInfo.Id = entity.Id;
                userInfo.UserName = entity.UserName;
                userInfo.Weight = entity.Weight;
                userInfo.Height = entity.Height;
                userInfo.DeviceToken = entity.DeviceToken;
                userInfo.Phone = entity.Phone;
                userInfo.Password = null;

                return Ok(userInfo);
            }

            return NotFound();

        }

        private IHttpActionResult Register(UserInfo userInfo)
        {
            userInfo.UserName = userInfo.UserName ?? userInfo.Id;
            userInfo.Weight = userInfo.Weight > 0 ? userInfo.Weight : 65;
            userInfo.Height = userInfo.Height > 0 ? userInfo.Height : 170;

            db.UserInfos.Add(userInfo);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (UserInfoExists(userInfo.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            userInfo.Password = null;

            return Ok(userInfo);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserInfoExists(string id)
        {
            return db.UserInfos.Count(e => e.Id == id) > 0;
        }
    }
}