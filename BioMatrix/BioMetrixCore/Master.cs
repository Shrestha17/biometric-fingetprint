///
///    Experimented By : Ozesh Thapa
///    Email: dablackscarlet@gmail.com
///
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using BioMetrixCore.ViewModel;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace BioMetrixCore
{
    public partial class Master : Form
    {
        DeviceManipulator manipulator = new DeviceManipulator();
        public ZkemClient objZkeeper;
        private bool isDeviceConnected = false;

        public bool IsDeviceConnected
        {
            get { return isDeviceConnected; }
            set
            {
                isDeviceConnected = value;
                if (isDeviceConnected)
                {
                    ShowStatusBar("The device is connected !!", true);
                    btnConnect.Text = "Disconnect";
                    ToggleControls(true);
                }
                else
                {
                    ShowStatusBar("The device is diconnected !!", true);
                    objZkeeper.Disconnect();
                    btnConnect.Text = "Connect";
                    ToggleControls(false);
                }
            }
        }


        private void ToggleControls(bool value)
        {
            btnBeep.Enabled = value;
            btnDownloadFingerPrint.Enabled = value;
            btnPullData.Enabled = value;
            btnPowerOff.Enabled = value;
            btnRestartDevice.Enabled = value;
            btnGetDeviceTime.Enabled = value;
            btnEnableDevice.Enabled = value;
            btnDisableDevice.Enabled = value;
            btnGetAllUserID.Enabled = value;
            btnUploadUserInfo.Enabled = value;
            tbxMachineNumber.Enabled = !value;
            tbxPort.Enabled = !value;
            tbxDeviceIP.Enabled = !value;

        }

        public Master()
        {
            InitializeComponent();
            ToggleControls(false);
            ShowStatusBar(string.Empty, true);
            DisplayEmpty();
        }


        /// <summary>
        /// Your Events will reach here if implemented
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="actionType"></param>
        private void RaiseDeviceEvent(object sender, string actionType)
        {
            switch (actionType)
            {
                case UniversalStatic.acx_Disconnect:
                    {
                        ShowStatusBar("The device is switched off", true);
                        DisplayEmpty();
                        btnConnect.Text = "Connect";
                        ToggleControls(false);
                        break;
                    }

                default:
                    break;
            }

        }


        private void btnConnect_Click(object sender, EventArgs e)
        {
            Connect();
        }

        public void Connect()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                ShowStatusBar(string.Empty, true);

                if (IsDeviceConnected)
                {
                    IsDeviceConnected = false;
                    this.Cursor = Cursors.Default;

                    return;
                }

                string ipAddress = tbxDeviceIP.Text.Trim();
                string port = tbxPort.Text.Trim();
                if (ipAddress == string.Empty || port == string.Empty)
                    throw new Exception("The Device IP Address and Port is mandotory !!");

                int portNumber = 4370;
                if (!int.TryParse(port, out portNumber))
                    throw new Exception("Not a valid port number");

                bool isValidIpA = UniversalStatic.ValidateIP(ipAddress);
                if (!isValidIpA)
                    throw new Exception("The Device IP is invalid !!");

                isValidIpA = UniversalStatic.PingTheDevice(ipAddress);
                if (!isValidIpA)
                    throw new Exception("The device at " + ipAddress + ":" + port + " did not respond!!");

                objZkeeper = new ZkemClient(RaiseDeviceEvent);
                IsDeviceConnected = objZkeeper.Connect_Net(ipAddress, portNumber);

                if (IsDeviceConnected)
                {
                    string deviceInfo = manipulator.FetchDeviceInfo(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                    lblDeviceInfo.Text = deviceInfo;
                }

            }
            catch (Exception ex)
            {
                ShowStatusBar(ex.Message, false);
            }
            this.Cursor = Cursors.Default;
        }
        public void ShowStatusBar(string message, bool type)
        {
            if (message.Trim() == string.Empty)
            {
                lblStatus.Visible = false;
                return;
            }

            lblStatus.Visible = true;
            lblStatus.Text = message;
            lblStatus.ForeColor = Color.White;

            if (type)
                lblStatus.BackColor = Color.FromArgb(79, 208, 154);
            else
                lblStatus.BackColor = Color.FromArgb(230, 112, 134);
        }


        private void btnPingDevice_Click(object sender, EventArgs e)
        {
            ShowStatusBar(string.Empty, true);

            string ipAddress = tbxDeviceIP.Text.Trim();

            bool isValidIpA = UniversalStatic.ValidateIP(ipAddress);
            if (!isValidIpA)
                throw new Exception("The Device IP is invalid !!");

            isValidIpA = UniversalStatic.PingTheDevice(ipAddress);
            if (isValidIpA)
                ShowStatusBar("The device is active", true);
            else
                ShowStatusBar("Could not read any response", false);
        }

        private void btnGetAllUserID_Click(object sender, EventArgs e)
        {
            try
            {
                ICollection<UserIDInfo> lstUserIDInfo = manipulator.GetAllUserID(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));

                if (lstUserIDInfo != null && lstUserIDInfo.Count > 0)
                {
                    BindToGridView(lstUserIDInfo);
                    ShowStatusBar(lstUserIDInfo.Count + " records found !!", true);
                }
                else
                {
                    DisplayEmpty();
                    DisplayListOutput("No records found");
                }

            }
            catch (Exception ex)
            {
                DisplayListOutput(ex.Message);
            }

        }

        private void btnBeep_Click(object sender, EventArgs e)
        {
            objZkeeper.Beep(100);
        }

        private void btnDownloadFingerPrint_Click(object sender, EventArgs e)
        {
            try
            {
                ShowStatusBar(string.Empty, true);

                ICollection<UserInfo> lstFingerPrintTemplates = manipulator.GetAllUserInfo(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                if (lstFingerPrintTemplates != null && lstFingerPrintTemplates.Count > 0)
                {
                    BindToGridView(lstFingerPrintTemplates);
                    ShowStatusBar(lstFingerPrintTemplates.Count + " records found !!", true);
                }
                else
                    DisplayListOutput("No records found");
            }
            catch (Exception ex)
            {
                DisplayListOutput(ex.Message);
            }

        }


        private void btnPullData_Click(object sender, EventArgs e)
        {
            try
            {
                ShowStatusBar(string.Empty, true);

                ICollection<MachineInfo> lstMachineInfo = manipulator.GetLogData(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));

                if (lstMachineInfo != null && lstMachineInfo.Count > 0)
                {
                    BindToGridView(lstMachineInfo);
                    ShowStatusBar(lstMachineInfo.Count + " records found !!", true);
                }
                else
                    DisplayListOutput("No records found");
            }
            catch (Exception ex)
            {
                DisplayListOutput(ex.Message);
            }

        }


        private void ClearGrid()
        {
            if (dgvRecords.Controls.Count > 2)
            { dgvRecords.Controls.RemoveAt(2); }


            dgvRecords.DataSource = null;
            dgvRecords.Controls.Clear();
            dgvRecords.Rows.Clear();
            dgvRecords.Columns.Clear();
        }
        private void BindToGridView(object list)
        {
            ClearGrid();
            dgvRecords.DataSource = list;
            dgvRecords.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            UniversalStatic.ChangeGridProperties(dgvRecords);
        }



        private void DisplayListOutput(string message)
        {
            if (dgvRecords.Controls.Count > 2)
            { dgvRecords.Controls.RemoveAt(2); }

            ShowStatusBar(message, false);
        }

        private void DisplayEmpty()
        {
            ClearGrid();
            dgvRecords.Controls.Add(new DataEmpty());
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        { UniversalStatic.DrawLineInFooter(pnlHeader, Color.FromArgb(204, 204, 204), 2); }



        private void btnPowerOff_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            var resultDia = DialogResult.None;
            resultDia = MessageBox.Show("Do you wish to Power Off the Device ??", "Power Off Device", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (resultDia == DialogResult.Yes)
            {
                bool deviceOff = objZkeeper.PowerOffDevice(int.Parse(tbxMachineNumber.Text.Trim()));

            }

            this.Cursor = Cursors.Default;
        }

        private void btnRestartDevice_Click(object sender, EventArgs e)
        {

            DialogResult rslt = MessageBox.Show("Do you wish to restart the device now ??", "Restart Device", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rslt == DialogResult.Yes)
            {
                if (objZkeeper.RestartDevice(int.Parse(tbxMachineNumber.Text.Trim())))
                    ShowStatusBar("The device is being restarted, Please wait...", true);
                else
                    ShowStatusBar("Operation failed,please try again", false);
            }

        }

        private void btnGetDeviceTime_Click(object sender, EventArgs e)
        {
            int machineNumber = int.Parse(tbxMachineNumber.Text.Trim());
            int dwYear = 0;
            int dwMonth = 0;
            int dwDay = 0;
            int dwHour = 0;
            int dwMinute = 0;
            int dwSecond = 0;

            bool result = objZkeeper.GetDeviceTime(machineNumber, ref dwYear, ref dwMonth, ref dwDay, ref dwHour, ref dwMinute, ref dwSecond);

            string deviceTime = new DateTime(dwYear, dwMonth, dwDay, dwHour, dwMinute, dwSecond).ToString();
            List<DeviceTimeInfo> lstDeviceInfo = new List<DeviceTimeInfo>();
            lstDeviceInfo.Add(new DeviceTimeInfo() { DeviceTime = deviceTime });
            BindToGridView(lstDeviceInfo);
        }


        private void btnEnableDevice_Click(object sender, EventArgs e)
        {
            // This is of no use since i implemented zkemKeeper the other way
            bool deviceEnabled = objZkeeper.EnableDevice(int.Parse(tbxMachineNumber.Text.Trim()), true);

        }



        private void btnDisableDevice_Click(object sender, EventArgs e)
        {
            // This is of no use since i implemented zkemKeeper the other way
            bool deviceDisabled = objZkeeper.DisableDeviceWithTimeOut(int.Parse(tbxMachineNumber.Text.Trim()), 3000);
        }

        private void tbxPort_TextChanged(object sender, EventArgs e)
        { UniversalStatic.ValidateInteger(tbxPort); }

        private void tbxMachineNumber_TextChanged(object sender, EventArgs e)
        { UniversalStatic.ValidateInteger(tbxMachineNumber); }

        private void btnUploadUserInfo_Click(object sender, EventArgs e)
        {
            // Add you new UserInfo Here and  uncomment the below code
            //List<UserInfo> lstUserInfo = new List<UserInfo>();
            //manipulator.UploadFTPTemplate(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()), lstUserInfo);
        }
        public static AppSettingsReader aps = new AppSettingsReader();
        private void button1_Click(object sender, EventArgs e)
        {
            //transaction
            //using (SqlConnection connection1 = new SqlConnection(aps.GetValue("myconnection", typeof(string)).ToString())
            //{
            //    connection1.Open();

            //    // Start a local transaction.
            //    SqlTransaction sqlTran = connection1.BeginTransaction();

            //    // Enlist a command in the current transaction.
            //    SqlCommand command = connection1.CreateCommand();
            //    command.Transaction = sqlTran;                
            //    {
                try
            {
                Connect();

                ShowStatusBar(string.Empty, true);

                ICollection<MachineInfo> machineInfoCollection = manipulator.GetLogData(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));

                List<MachineInfo> lstMachineInfo = machineInfoCollection.ToList();

                BLLGetAttendanceData bllGetAttendanceData = new BLLGetAttendanceData();
                DataTable dt = bllGetAttendanceData.GetLastAttendanceLogPull();

                DateTime LastPulledDateTime;
                DateTime LastPulledDateOnly;
                var todaysDateTime = DateTime.Now;
                if (dt.Rows.Count > 0)
                {
                    LastPulledDateTime = Convert.ToDateTime(dt.Rows[0]["LastPulledDateTime"]);
                    LastPulledDateOnly = Convert.ToDateTime(Convert.ToDateTime(dt.Rows[0]["LastPulledDateTime"]).ToShortDateString());
                }
                else
                {
                    LastPulledDateTime = Convert.ToDateTime(lstMachineInfo.Where(x => Convert.ToDateTime(x.DateTimeRecord) < DateTime.Now).OrderBy(x => x.DateTimeRecord).FirstOrDefault().DateTimeRecord);
                    LastPulledDateOnly = Convert.ToDateTime(LastPulledDateTime.ToShortDateString());
                }

                var newAttendanceRecords = lstMachineInfo.Where(x => Convert.ToDateTime(x.DateTimeRecord) >= LastPulledDateTime).ToList();



                int daysGap = Convert.ToInt32((todaysDateTime - LastPulledDateTime).TotalDays);

                DataTable allUsers = bllGetAttendanceData.GetAllUser();

                for (int i = 0; i <= daysGap; i++)
                {
                    var DaywiseAttendanceRecords = newAttendanceRecords.Where(x => Convert.ToDateTime(x.DateOnlyRecord) == LastPulledDateOnly.AddDays(i));

                    if (DaywiseAttendanceRecords.Count() > 0 && allUsers.Rows.Count > 0)
                    {
                        for (int j = 0; j < allUsers.Rows.Count; j++)
                        {
                            int personId = Convert.ToInt32(allUsers.Rows[j]["PersonID"]);
                            if (i == 0)
                            {
                                DataTable logData = bllGetAttendanceData.GetPersonLogDataByDate(personId, LastPulledDateOnly);
                                if (logData.Rows.Count > 0)
                                {
                                    var UserwiseLastAttendancerecords = DaywiseAttendanceRecords.Where(x => x.IndRegID == personId).LastOrDefault();
                                    if (UserwiseLastAttendancerecords != null)
                                    {
                                        int val = bllGetAttendanceData.UpdateLogData(personId, UserwiseLastAttendancerecords.TimeOnlyRecord);
                                    }
                                }
                                else
                                {
                                    //insert
                                    var UserwiseFirstAttendancerecords = DaywiseAttendanceRecords.Where(x => x.IndRegID == personId).FirstOrDefault();
                                    var UserwiseLastAttendancerecords = DaywiseAttendanceRecords.Where(x => x.IndRegID == personId).LastOrDefault();
                                    if (UserwiseFirstAttendancerecords != null && UserwiseLastAttendancerecords != null)
                                    {
                                        DeviceRecord dr = new DeviceRecord();
                                        dr.PersonalId = personId;
                                        dr.AttendanceDate = UserwiseFirstAttendancerecords.DateOnlyRecord;
                                        dr.AttendanceDateTime = Convert.ToDateTime(UserwiseFirstAttendancerecords.DateTimeRecord);
                                        dr.InTime = UserwiseFirstAttendancerecords.TimeOnlyRecord;
                                        dr.OutTime = UserwiseLastAttendancerecords.TimeOnlyRecord;

                                        int a = bllGetAttendanceData.Insert(dr);
                                    }
                                }
                            }
                            else
                            {
                                var UserwiseFirstAttendancerecords = DaywiseAttendanceRecords.Where(x => x.IndRegID == personId).FirstOrDefault();
                                var UserwiseLastAttendancerecords = DaywiseAttendanceRecords.Where(x => x.IndRegID == personId).LastOrDefault();
                                if (UserwiseFirstAttendancerecords != null && UserwiseLastAttendancerecords != null)
                                {
                                    DeviceRecord dr = new DeviceRecord();
                                    dr.PersonalId = personId;
                                    dr.AttendanceDate = UserwiseFirstAttendancerecords.DateOnlyRecord;
                                    dr.AttendanceDateTime = Convert.ToDateTime(UserwiseFirstAttendancerecords.DateTimeRecord);
                                    dr.InTime = UserwiseFirstAttendancerecords.TimeOnlyRecord;
                                    dr.OutTime = UserwiseLastAttendancerecords.TimeOnlyRecord;

                                    int a = bllGetAttendanceData.Insert(dr);
                                }
                            }
                        }
                    }
                }
                int b = bllGetAttendanceData.InsertLatestPullRecord(todaysDateTime);
                DisplayListOutput("Attendance pulled sucessfully");

            }
            catch (Exception ex)
            {
                DisplayListOutput(ex.Message);
            }
        }
    }
}
