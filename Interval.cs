using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Manager;

public record Interval(string Name)
{
    public readonly Guid guid = Guid.NewGuid(); 
    public TimeSpan TimeSpan { get; set; } = TimeSpan.FromMinutes(0.05412321312);
}