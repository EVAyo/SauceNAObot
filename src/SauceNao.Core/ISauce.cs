using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SauceNAO.Core
{
    public interface ISauce
    {
        string? Title { get; }
        string? Characters { get; }
        string? Material { get; }
        string? Part { get; }
        string? Year { get; }
        string? EstTime { get; }
        string? By { get; }
    }
}
