using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WorldCities.Data.Models
{
    public class Country
    {
        public Country()
        {

        }

        [Key]
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }

        public string ISO2 { get; set; }

        public string ISO3 { get; set; }

        #region Navigation Properties
        /// <summary>
        /// A list containing all the cities related to this country.
        /// </summary>
        public virtual List<City> Cities { get; set; }
        #endregion
    }
}
