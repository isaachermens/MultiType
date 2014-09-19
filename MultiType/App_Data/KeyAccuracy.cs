using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace MultiType.AppData
{
    public class KeyAccuracy
    {
        [Key]
        public int KeyAccuracyId { get; set; }
        [Required]
        public int Key { get; set; }
        [Required]
        public int Occurances { get; set; }
        [Required]
        public int Errors { get; set; }

        public int RaceId { get; set; }
        [ForeignKey("RaceId")]
        public virtual Race Race { get; set; }

    }
}
