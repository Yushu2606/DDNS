using System.Collections.Generic;

namespace SNDD.Utils;

internal struct Config
{
    public string AccessKeyId { get; set; }
    public string AccessKeySecret { get; set; }
    public List<Domain> Domains { get; set; }
    public struct Domain
    {
        public string Name { get; set; }
        public List<string> SubDomains { get; set; }
    }
    public double Interval { get; set; }
}
