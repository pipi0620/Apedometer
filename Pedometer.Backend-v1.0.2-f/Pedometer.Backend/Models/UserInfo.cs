using System;

namespace Pedometer.Models
{
    public class UserInfo
    {
        public String Id { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }
        public String Phone { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string DeviceToken { get; set; }
    }
}