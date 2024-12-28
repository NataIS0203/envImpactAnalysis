using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durable.Records
{
    public record ReportTypeReport(int Id, string Name)
    {
        public static ImageTypeRecord Species { get; } = new(1, "Species");
        public static ImageTypeRecord Resources { get; } = new(2, "Resources");
        public static ImageTypeRecord Images { get; } = new(3, "Images");
        public override string ToString() => Name;
    }
}
