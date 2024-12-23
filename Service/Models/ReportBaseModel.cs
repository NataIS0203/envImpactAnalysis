using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durable.Service.Models
{
    public class ReportBaseModel
    {
        public required string Name { get; set; }

        public string? Region { get; set; }

        public int Percentage { get; set; }

        public string ReportName { get; set; }
    }
}
