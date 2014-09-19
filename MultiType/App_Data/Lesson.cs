using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiType.AppData;

namespace MultiType.App_Data
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }
        [Required]
        [MaxLength]
        [MinLength(1)]
        public string Content { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        [Required]
        public int TimesCompleted { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
