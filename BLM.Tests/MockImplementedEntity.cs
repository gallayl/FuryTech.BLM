using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLM.Tests
{
    public class MockImplementedEntity : IMockEntity
    {
        public string Guid { get; set; }
        public bool IsVisible { get; set; }
        public bool IsValid { get; set; }
    }
}
