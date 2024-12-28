using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durable.Records
{
    public record ImageTypeRecord(int Id, string Name)
    {
        public static ImageTypeRecord Photography { get; } = new(1, "Photography");
        public static ImageTypeRecord Drawing { get; } = new(2, "Drawing");
        public static ImageTypeRecord Painting { get; } = new(3, "Painting");
        public static ImageTypeRecord Sketch { get; } = new(4, "Sketch");
        public override string ToString() => Name;
    }
}
