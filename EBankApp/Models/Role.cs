using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EBankApp.Models
{
    public class Role : BaseEntity
    {
        public string Name { get; set; }
    }
}