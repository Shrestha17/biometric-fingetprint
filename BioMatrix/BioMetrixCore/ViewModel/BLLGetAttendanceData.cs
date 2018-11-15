using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioMetrixCore.ViewModel
{
    public class BLLGetAttendanceData
    {
        public int Insert(DeviceRecord deviceRecord)
        {
            string sql = "insert into LogData values(@a,@b,@c,@d,@e)";
            SqlParameter[] param = new SqlParameter[]
            {
               new SqlParameter("@a",deviceRecord.PersonalId),
               new SqlParameter("@b",deviceRecord.AttendanceDate),
               new SqlParameter("@c",deviceRecord.AttendanceDateTime),
               new SqlParameter("@d",deviceRecord.InTime),
               new SqlParameter("@e",deviceRecord.OutTime)
            };

            int i = DAO.IUD(sql, param);
            return i;
        }

        public int InsertLatestPullRecord()
        {
            string sql = "insert into LatestPullRecord values(@a)";
            SqlParameter[] param = new SqlParameter[]
            {
               new SqlParameter("@a",DateTime.Now)              
            };

            int i = DAO.IUD(sql, param);
            return i;
        }

        public DataTable GetLastAttendanceLogPull()
        {
            DataTable dt = DAO.GetTable("Select Top 1 * from LatestPullRecord Order By LatestPullRecordID DESC", null);
            return dt;
        }

        public DataTable GetAllUser()
        {
            DataTable dt = DAO.GetTable("Select * from [User]", null);
            return dt;
        }
    }
}