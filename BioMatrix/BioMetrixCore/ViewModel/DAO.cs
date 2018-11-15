using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioMetrixCore.ViewModel
{
    public static class DAO
    {
        public static AppSettingsReader aps = new AppSettingsReader();
        public static SqlConnection GetConnection()
        {
            SqlConnection con = new SqlConnection(aps.GetValue("myconnection", typeof(string)).ToString());
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            return con;
        }

        public static int IUD(string sql, SqlParameter[] param)
        {
            using (SqlConnection con = GetConnection())
            {

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {

                    if (param != null)
                    {
                        cmd.Parameters.AddRange(param);
                    }


                    int i = cmd.ExecuteNonQuery();//insert delete update

                    return i;
                }
            }
        }
        public static DataTable GetTable(string sql, SqlParameter[] param)
        {

            using (SqlConnection con = GetConnection())
            {

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {

                    if (param != null)
                    {
                        cmd.Parameters.AddRange(param);
                    }
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {

                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }


                }
            }
        }

    }
}