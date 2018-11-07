using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace BioMetrixCore.ViewModel
{
    class AttendanceLogDA
    {
        public static bool Insert(FinalEntryData attendanceLog)
        {

            bool ExecutionState = false;
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string sql = "Insert into ATT_Attendance (PersonalId, [Date], InTime, OutTime) values" +
                           "( @PersonalId, @AttendanceDate, @InTime, @OutTime)";

            if (attendanceLog.PersonalId != 0)
                oDatabaseHelper.AddParameter("@PersonalId", attendanceLog.PersonalId);
            else
                oDatabaseHelper.AddParameter("@PersonalId", DBNull.Value);

            if (attendanceLog.AttendanceDate != null)
                oDatabaseHelper.AddParameter("@AttendanceDate", attendanceLog.AttendanceDate.Date);
            else
                oDatabaseHelper.AddParameter("@AttendanceDate", DBNull.Value);

            if (attendanceLog.InTime != null)
                oDatabaseHelper.AddParameter("@InTime", attendanceLog.InTime);
            else
                oDatabaseHelper.AddParameter("@InTime", DBNull.Value);

            if (attendanceLog.OutTime != null)
                oDatabaseHelper.AddParameter("@OutTime", attendanceLog.OutTime);
            else
                oDatabaseHelper.AddParameter("@OutTime", DBNull.Value);


            oDatabaseHelper.ExecuteNonQuery(sql, CommandType.Text);
            oDatabaseHelper.Dispose();
            return ExecutionState;
        }
        public static bool InsertAll(AttendanceLog attendanceLog)
        {

            bool ExecutionState = false;
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string sql = "Insert into ATT_Attendance (PersonalId, [Date], InTime, OutTime) values" +
                           "( @PersonalId, @AttendanceDate, @InTime, @OutTime)";

            if (attendanceLog.PersonalId != 0)
                oDatabaseHelper.AddParameter("@PersonalId", attendanceLog.PersonalId);
            else
                oDatabaseHelper.AddParameter("@PersonalId", DBNull.Value);

            if (attendanceLog.AttendanceDate != null)
                oDatabaseHelper.AddParameter("@AttendanceDate", attendanceLog.AttendanceDate.Date);
            else
                oDatabaseHelper.AddParameter("@AttendanceDate", DBNull.Value);

            if (attendanceLog.InTime != null)
                oDatabaseHelper.AddParameter("@InTime", attendanceLog.InTime);
            else
                oDatabaseHelper.AddParameter("@InTime", DBNull.Value);

            if (attendanceLog.OutTime != null)
                oDatabaseHelper.AddParameter("@OutTime", attendanceLog.OutTime);
            else
                oDatabaseHelper.AddParameter("@OutTime", DBNull.Value);


            oDatabaseHelper.ExecuteNonQuery(sql, CommandType.Text);
            oDatabaseHelper.Dispose();
            return ExecutionState;
        }

        public static bool Update(FinalLeaveingData leavingData)
        {
            bool ExecutionState = false;
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string sql = "Update ATT_Attendance  set  OutTime=@OutTime where PersonalId=@PersonalId and [Date]=@OutDate";

            if (leavingData.PersonalId != 0)
                oDatabaseHelper.AddParameter("@PersonalId", leavingData.PersonalId);
            else
                oDatabaseHelper.AddParameter("@PersonalId", DBNull.Value);

            if (leavingData.OutTime != null)
                oDatabaseHelper.AddParameter("@OutTime", leavingData.OutTime);
            else
                oDatabaseHelper.AddParameter("@OutTime", DBNull.Value);

            if (leavingData.AttendanceDateTime != null)
                oDatabaseHelper.AddParameter("@OutDate", leavingData.AttendanceDateTime.Date);
            else
                oDatabaseHelper.AddParameter("@OutDate", DBNull.Value);


            try
            {
                oDatabaseHelper.ExecuteNonQuery(sql, CommandType.Text);
                oDatabaseHelper.Dispose();
            }
            catch { }
            return ExecutionState;
        }
        public static bool InsertTimeRecord(DateTime currentDate, DateTime currentTime,DateTime currentDateTime)
        {

            bool ExecutionState = false;
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string sql = "Insert into DV_AttendanceLogPull (LastUpdateDate,LastUpdateTime,LastUpdateDateTime) values" +
                           "(@currentDate,@currentTime,@currentDateTime)";

            if (currentDate != null)
                oDatabaseHelper.AddParameter("@currentDate", currentDate);
            else
                oDatabaseHelper.AddParameter("@currentDate", DBNull.Value);

            if (currentTime != null)
                oDatabaseHelper.AddParameter("@currentTime", currentTime);
            else
                oDatabaseHelper.AddParameter("@currentTime", DBNull.Value);

            if (currentDateTime != null)
                oDatabaseHelper.AddParameter("@currentDateTime", currentDateTime);
            else
                oDatabaseHelper.AddParameter("@currentDateTime", DBNull.Value);


            oDatabaseHelper.ExecuteNonQuery(sql, CommandType.Text);
            oDatabaseHelper.Dispose();
            return ExecutionState;
        }

        public static DataTable GetLastAttendanceLogPull()
        {
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string qry = "Select Top 1 LastUpdateDate,LastUpdateTime,LastUpdateDateTime from DV_AttendanceLogPull order by Id DESC";

            DataSet ds = null;
            try
            {
                ds = oDatabaseHelper.ExecuteDataSet(qry, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds.Tables[0];
        }
      
        public static DataTable GetPersonalIdFromPersonalDetail()
        {
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string qry = "Select PersonalId from EMP_PersonalDetail";

            DataSet ds = null;
            try
            {
                ds = oDatabaseHelper.ExecuteDataSet(qry, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds.Tables[0];
        }

        //For Multiple Device
        public static DataTable GetListOfDevice_From_Device()
        {
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string qry = "Select DeviceName,IPAddress,Port,Status from DV_Device";

            DataSet ds = null;
            try
            {
                ds = oDatabaseHelper.ExecuteDataSet(qry, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds.Tables[0];
        }
        public static bool Update_Device_Status(bool status,string IpAddress)
        {
            bool ExecutionState = false;
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string sql = "Update DV_Device  set  Status=@status where IPAddress=@IpAddress";

            //status will be either true or false
           oDatabaseHelper.AddParameter("@status", status);

           if (IpAddress != null)
               oDatabaseHelper.AddParameter("@IpAddress", IpAddress);
           else
               oDatabaseHelper.AddParameter("@IpAddress", DBNull.Value);

            oDatabaseHelper.ExecuteNonQuery(sql, CommandType.Text);
            oDatabaseHelper.Dispose();
            return ExecutionState;
        }

        public static DataTable GetListOfDevice_Where_Status_True()
        {
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string qry = "Select DeviceName,IpAddress,Port,Status from DV_Device where Status='true'";

            DataSet ds = null;
            try
            {
                ds = oDatabaseHelper.ExecuteDataSet(qry, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds.Tables[0];
        }




        public static DataTable GetAllAttendance(int personalId, DateTime LastUpdatedDate)
        {
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string qry = "Select Top 1 * From ATT_Attendance where PersonalId=@personalId and [Date]=@LastUpdatedDate order by AttendanceId DESC";

            if (personalId != 0)
                oDatabaseHelper.AddParameter("@personalId", personalId);
            else
                oDatabaseHelper.AddParameter("@personalId", DBNull.Value);
            if (LastUpdatedDate != null)
                oDatabaseHelper.AddParameter("@LastUpdatedDate", LastUpdatedDate.Date);
            else
                oDatabaseHelper.AddParameter("@LastUpdatedDate", DBNull.Value);

            DataSet ds = null;
            try
            {
                ds = oDatabaseHelper.ExecuteDataSet(qry, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds.Tables[0];
        }


        public static int GetAllTeamCount()
        {
            DatabaseHelper oDatabaseHelper = new DatabaseHelper();
            string qry = "Select count(TeamID) From Team";
            object num = null;
            try
            {
                num = oDatabaseHelper.ExecuteScalar(qry, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            int Count = 0;
            if (num.ToString() == "")
                Count = 0;
            else
                Count = Convert.ToInt32(num);

            return Count;

        }

    }
}
