using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Pedometer.Data;
using Pedometer.Models;

namespace Pedometer.Controllers.Api
{
    /// <summary>
    /// Background tasks, push messages to all users who lack exercise at a fixed time every day.
    /// </summary>
    public class BackgroundTask
    {
        static BackgroundTask()
        {
            Init();
        }
        public static DateTime NotifyTime = DateTime.Parse("18:00:00");
        public static int NotifyThreshold = 1000;
        private static Timer timer;
        public static void Init()
        {
            var now = DateTime.Now;
            var nowTime = now.TimeOfDay;
            var notifyTime = NotifyTime.TimeOfDay;

            var delayTime = nowTime < notifyTime ? notifyTime - nowTime : (notifyTime + TimeSpan.FromDays(1)) - nowTime;
            timer = new Timer(o => Work(), null, delayTime, TimeSpan.FromDays(1));
        }

        public static List<UserInfo> Work()
        {
            var notifiedUsers = new List<UserInfo>();
            WebApiContext db = new WebApiContext();
            var today = (DateTime.Today - new DateTime(1970, 1, 1)).Ticks / 10000;

            //Users who exercise but do not exercise enough
            var sql = $@"SELECT u.* FROM UserInfoes u , 
	(SELECT  UserId,MAX(TodaySteps) steps FROM StepInfoes s WHERE s.TimeStamp > {today} GROUP BY s.UserId) AS msteps 
WHERE 	u.Id=msteps.UserId	AND msteps.steps< {NotifyThreshold}";
            var devices = db.UserInfos.SqlQuery(sql);
            foreach (var deviceToken in devices)
            {
                if (deviceToken.DeviceToken.IsNullOrWhiteSpace())
                {
                    continue;
                }
                notifiedUsers.Add(deviceToken);
                Notify(deviceToken.DeviceToken);
            }
            //Today's list of users who have no exercise records
            sql = $@"select u.* from UserInfoes u ,	(select u.Id from UserInfoes u ,StepInfoes s where u.Id = s.UserId and s.TimeStamp>{today} group by u.Id,s.UserId) as st
where u.Id!=st.Id";
            devices = db.UserInfos.SqlQuery(sql);

            foreach (var deviceToken in devices)
            {
                if (deviceToken.DeviceToken.IsNullOrWhiteSpace())
                {
                    continue;
                }
                notifiedUsers.Add(deviceToken);
                Notify(deviceToken.DeviceToken);
            }
            return notifiedUsers;
        }

        private static void Notify(string deviceToken)
        {
            RequestFcmApi(deviceToken);
        }

        private void RequestFcmApiBySdk(string deviceToken)
        {

        }
        private static void RequestFcmApi(string deviceToken)
        {
            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";

            //serverKey - Key from Firebase cloud messaging server  
            tRequest.Headers.Add(string.Format("Authorization: key={0}", "AAAAAIZ_0sw:APA91bHz_58pQ0RMivL4_Ipur4wFYeI9EdIuPDSKXHySdrPMYTQhBfwqUYIerx3WDQAWjyrsfFBhhR3mI7sVEiOcTumY6Ft5lo74RQaEg-9eFA5HX2mz5bw95lTPQSFcvbhxhe26u8eP"));
            //Sender Id - From firebase project setting  
            tRequest.Headers.Add(string.Format("Sender: id={0}", "2256523980"));
            tRequest.ContentType = "application/json";
            var payload = new
            {
                to = deviceToken,
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = "You didn't exercise enough today, go for a walk!",
                    title = "Time to exercise",
                    badge = 1
                },
            };

            string postbody = JsonConvert.SerializeObject(payload).ToString();
            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
            tRequest.ContentLength = byteArray.Length;
            using (Stream dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                //result.Response = sResponseFromServer;
                            }
                    }
                }
            }
        }
    }
}