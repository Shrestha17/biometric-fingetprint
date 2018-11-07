using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioMetrixCore.ViewModel
{
    class AttendanceLog
    {
        public int MachineNumber { get; set; }
        public int PersonalId { get; set; }
        //public string DateTimeRecord { get; set; }
       // public string DateOnlyRecord { get; set; }
        //public string TimeOnlyRecord { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime AttendanceDateTime { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        //public DateTime DeviceDate { get; set; }
    }

   
}
