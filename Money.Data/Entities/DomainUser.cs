﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities
{
    public class DomainUser
    {
        [Key]
        [Column]
        public int Id { get; set; }

        [Column]
        public Guid AuthUserId { get; set; }

        public required List<Category> Categories { get; set; }
    }
}
