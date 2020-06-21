using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedometer.Models
{
    public class StepInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  //Set increment
        public int Id { get; set; }
        public String UserId { get; set; }
        public long TimeStamp { get; set; }
        public int TodaySteps { get; set; }
        public float Distance { get; set; }
        public float Calories { get; set; }
    }
}