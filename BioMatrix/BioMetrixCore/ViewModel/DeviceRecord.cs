using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioMetrixCore.ViewModel
{
    public class DeviceRecord
    {
        public int PersonalId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime AttendanceDateTime { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
    }
}
