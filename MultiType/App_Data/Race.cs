using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiType.App_Data;

// ReSharper disable once CheckNamespace
namespace MultiType.AppData
{
    public class Race
    {
        [Key]
        public int RaceId { get; set; }
        [Required]
        public int ContentLength { get; set; }
        [Required]
        public int CharactersTyped { get; set; }
        [Required]
        public bool Multiplayer { get; set; }
        [Required]
        public bool Won { get; set; }
        [Required]
        public int Wpm { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual IList<KeyAccuracy> KeyAccuracies { get; set; }

        [NotMapped]
        public double Accuracy { get; set; }
    }
}
