﻿
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Model.Vm
{
    public class FreelancerVm
    {
        public int Id { get; set; }
        [Required]
        public string Lenguaje { get; set; }
        [Required]
        public decimal PriceHour { get; set; }

        public string Biography { get; set; }
        [Required]
        public string Interest { get; set; }
        public int Level { get; set; }
        
        public string Historial { get; set; }
        public int Rating { get; set; }
        public string Profesion { get; set; }

        [Required]
        public decimal Long { get; set; }
        [Required]
        public decimal Lat { get; set; }

        //account
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Name { get; set; }
        public string Avatar { get; set; }
        [Required]
        public string Email { get; set; }
        public string ApplicationUserId { get; set; }

        public List<Hability> Habilities { get; set; }


    }
}
