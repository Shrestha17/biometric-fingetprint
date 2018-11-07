///
///    Experimented By : Ozesh Thapa
///    Email: dablackscarlet@gmail.com
///
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using BioMetrixCore.ViewModel;

using System.Data.SqlClient;
using System.Data;
namespace BioMetrixCore
{
    public partial class Master : Form
    {
        DeviceManipulator manipulator = new DeviceManipulator();
        public ZkemClient objZkeeper;
        private bool isDeviceConnected = false;
        // BLLPullAttendance blu = new BLLPullAttendance();

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
                List<AttendanceLog> model = new List<AttendanceLog>();
                List<FilterAttendanceLog> filterAttendanceLog = new List<FilterAttendanceLog>();

                DateTime currentDate = DateTime.Now.Date;
                DateTime currentTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
                DateTime currentDateTime = Convert.ToDateTime(DateTime.Now.ToString());
              
              
                foreach (var item in lstMachineInfo) //retrieve each item and assign to model
                {
                    model.Add(new AttendanceLog()
                {

                    MachineNumber = item.MachineNumber,
                    PersonalId = item.IndRegID,
                    AttendanceDateTime = Convert.ToDateTime(item.DateTimeRecord),  
                    AttendanceDate = (item.DateOnlyRecord),
                    InTime = Convert.ToDateTime(item.DateTimeRecord),
                    OutTime = Convert.ToDateTime(item.DateTimeRecord)

                });
                }
                    foreach (var i in model)
                            {
                                filterAttendanceLog.Add(new FilterAttendanceLog()
                                {
                                    MachineNumber = i.MachineNumber,
                                    PersonalId = i.PersonalId,
                                    AttendanceDate = i.AttendanceDate,
                                    AttendanceDateTime = i.AttendanceDateTime,
                                    InTime = i.InTime,
                                    OutTime = i.InTime
                                });
                            }
                    List<EntryData> entryData=new List<EntryData>();
                    List<LeaveingData> leaveData=new List<LeaveingData>();
                            if ((filterAttendanceLog != null))
                            {
                                FilterAttendanceLog TopData = new FilterAttendanceLog();
                                FilterAttendanceLog LastData = new FilterAttendanceLog();

                                TopData = filterAttendanceLog[0];
                                entryData.Add(new EntryData()
                                {
                                    MachineNumber = TopData.MachineNumber,
                                    PersonalId = TopData.PersonalId,
                                    AttendanceDate = TopData.AttendanceDate,
                                    AttendanceDateTime = TopData.AttendanceDateTime,
                                    InTime = TopData.InTime,
                                    OutTime = TopData.OutTime

                                });                                

                                LastData = filterAttendanceLog[filterAttendanceLog.Count-1];
                                leaveData.Add(new LeaveingData()
                                {
                                    PersonalId = LastData.PersonalId,
                                    AttendanceDate = LastData.AttendanceDate,
                                    AttendanceDateTime = LastData.AttendanceDateTime,
                                    OutTime = LastData.OutTime
                                });

                            }

                
                if ((entryData != null && model.Count > 0))
                {
                        BindToGridView(model);
                        ShowStatusBar(model.Count + " records found !!", true);
                   
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

        private void button1_Click(object sender, EventArgs e)
        {
            AttendanceLog attendanceLog = new AttendanceLog();
            attendanceLog.PersonalId = 1;
            attendanceLog.AttendanceDate = DateTime.Now;
            attendanceLog.InTime = Convert.ToDateTime("10:00:00");
            attendanceLog.OutTime = Convert.ToDateTime("17:00:00");
            AttendanceLogDA.InsertAll(attendanceLog);
        }

     

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //Updating status of DV_Device
                DataTable ListOfDevice = AttendanceLogDA.GetListOfDevice_From_Device();
                foreach (DataRow lstDev in ListOfDevice.Rows)
                {
                    this.Cursor = Cursors.WaitCursor;
                    ShowStatusBar(string.Empty, true);

                    if (IsDeviceConnected)
                    {
                        IsDeviceConnected = false;
                        this.Cursor = Cursors.Default;

                        return;
                    }
                    string ipAddress = ((string)lstDev["IPAddress"]).Trim();
                    string port = ((string)lstDev["Port"]).Trim();

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

                    this.Cursor = Cursors.Default;

                    //After connection completed

                    MakeAttendanceLog();


                }

                ShowStatusBar(" records found and saved to database !!", true);
            }
            catch (Exception ex)
            {
                DisplayListOutput(ex.Message);
            }
          
        }


        private void MakeAttendanceLog()  //if DV_AttendanceLog has previous record
        {
            try
            {
                ShowStatusBar(string.Empty, true);

                ICollection<MachineInfo> lstMachineInfo = manipulator.GetLogData(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                List<AttendanceLog> model = new List<AttendanceLog>();


                DateTime currentDate = DateTime.Now.Date;
                DateTime currentDateTime = Convert.ToDateTime(DateTime.Now.ToString());
                DateTime currentTime = Convert.ToDateTime(DateTime.Now.ToLongTimeString());
                DateTime? NextDateTime = null;
                DateTime? NextDate = null;
                int DaysToUpdate = 0;

                foreach (var item in lstMachineInfo) //retrieve each item and assign to model
                {
                    model.Add(new AttendanceLog()
                    {
                        MachineNumber = item.MachineNumber,
                        PersonalId = item.IndRegID,
                        AttendanceDateTime = Convert.ToDateTime(item.DateTimeRecord),  
                        AttendanceDate = (item.DateOnlyRecord),
                        InTime = Convert.ToDateTime(item.DateTimeRecord),
                        OutTime = Convert.ToDateTime(item.DateTimeRecord)

                    });
                }

                DataTable dt = AttendanceLogDA.GetLastAttendanceLogPull();
                if (dt.Rows.Count > 0)
                {
                    DateTime LastUpdateDateTime = Convert.ToDateTime(dt.Rows[0]["LastUpdateDateTime"]);
                    DateTime LastUpdateDate = Convert.ToDateTime(dt.Rows[0]["LastUpdateDate"]);
                    DaysToUpdate = (currentDate.Subtract(LastUpdateDate)).Days;
                    

                    for (int day = 0; day <= DaysToUpdate; day++)
                    {
                        NextDateTime = LastUpdateDateTime.AddDays(day);
                        NextDate = LastUpdateDateTime.AddDays(day).Date;                        
                       
                        List<FinalEntryData> finalEntryData = new List<FinalEntryData>();
                        List<FinalLeaveingData> finalLeavingData = new List<FinalLeaveingData>();

                        DataTable personalId = AttendanceLogDA.GetPersonalIdFromPersonalDetail();
                        foreach (DataRow pid in personalId.Rows)
                        {
                            List<EntryData> entryData = new List<EntryData>();
                            List<LeaveingData> leaveData = new List<LeaveingData>();
                            int pi = (int)pid["PersonalId"];
                            List<FilterAttendanceLog> filterAttendanceLog = new List<FilterAttendanceLog>();
                            if (day == 0)
                            {

                                DataTable dtTime = AttendanceLogDA.GetAllAttendance(pi, LastUpdateDate);
                                if (dtTime.Rows.Count > 0)    // next day ma hijo ko check garda pailai entry date bhai sake ko bhayear update gareko
                                {                                   
                                    if (pi >= 5000)
                                    {
                                        int pii = pi;
                                    }
                                    if (model.FindAll(m => m.PersonalId == pi && m.AttendanceDate.Date == NextDate).Count > 0)    //&& m.OutTime >= ((DateTime)(dtTime.Rows[0]["OutTime"]))
                                    {
                                        var attlog = model.FindAll(m => m.PersonalId == pi && m.AttendanceDate.Date == NextDate);  //&& m.OutTime >= ((DateTime)(dtTime.Rows[0]["OutTime"])
                                        foreach (var item in attlog)
                                        {
                                            filterAttendanceLog.Add(new FilterAttendanceLog()
                                            {
                                                MachineNumber = item.MachineNumber,
                                                PersonalId = item.PersonalId,
                                                AttendanceDate = item.AttendanceDate,
                                                AttendanceDateTime = item.AttendanceDateTime,
                                                InTime = item.InTime,
                                                OutTime = item.OutTime
                                            });
                                        }

                                        if ((filterAttendanceLog != null))
                                        {
                                           FilterAttendanceLog TopData = new FilterAttendanceLog();
                                            FilterAttendanceLog LastData = new FilterAttendanceLog();


                                            LastData = filterAttendanceLog[filterAttendanceLog.Count - 1];
                                            leaveData.Add(new LeaveingData()
                                            {
                                                PersonalId = LastData.PersonalId,
                                                AttendanceDate = LastData.AttendanceDate,
                                                AttendanceDateTime = LastData.AttendanceDateTime,
                                                OutTime = LastData.InTime
                                            });


                                        }
                                    }


                                }
                                else  //next day ma hijo ko check garda paila entry gareko thiyena so insert gareko
                                {                                    
                                    if (pi >= 5000)
                                    {
                                        int pii = pi;
                                    }
                                    if (model.FindAll(m => m.PersonalId == pi && m.AttendanceDateTime.Date == NextDate).Count > 0)    //&& m.OutTime >= ((DateTime)(dtTime.Rows[0]["OutTime"]))
                                    {
                                        var attlog = model.FindAll(m => m.PersonalId == pi && m.AttendanceDateTime.Date == NextDate);  //&& m.OutTime >= ((DateTime)(dtTime.Rows[0]["OutTime"])
                                        foreach (var item in attlog)
                                        {
                                            filterAttendanceLog.Add(new FilterAttendanceLog()
                                            {
                                                MachineNumber = item.MachineNumber,
                                                PersonalId = item.PersonalId,
                                                AttendanceDate = item.AttendanceDate,
                                                AttendanceDateTime = item.AttendanceDateTime,
                                                InTime = item.InTime,
                                                OutTime = item.InTime
                                            });
                                        }

                                        if ((filterAttendanceLog != null))
                                        {
                                            FilterAttendanceLog TopData = new FilterAttendanceLog();
                                            FilterAttendanceLog LastData = new FilterAttendanceLog();


                                            TopData = filterAttendanceLog[0];
                                            entryData.Add(new EntryData()
                                            {
                                                MachineNumber = TopData.MachineNumber,
                                                PersonalId = TopData.PersonalId,
                                                AttendanceDate = TopData.AttendanceDate,
                                                AttendanceDateTime = TopData.AttendanceDateTime,
                                                InTime = TopData.InTime,
                                                OutTime = TopData.InTime

                                            });

                                            LastData = filterAttendanceLog[filterAttendanceLog.Count - 1];
                                            leaveData.Add(new LeaveingData()
                                            {
                                                PersonalId = LastData.PersonalId,
                                                AttendanceDate = LastData.AttendanceDate,
                                                AttendanceDateTime = LastData.AttendanceDateTime,
                                                OutTime = LastData.InTime
                                            });
                                        }
                                    }
                                }
                            }

                            //day>=1
                            else  //next day ma hijo ko baki chainaaaa bhane ... jun din update gardai xa tei din ko
                            {                                
                                if (pi >= 5000)
                                {
                                    int pii = pi;
                                }
                                if (model.FindAll(m => m.PersonalId == pi && m.AttendanceDateTime.Date == NextDate).Count > 0)
                                {
                                    var attlog = model.FindAll(m => m.PersonalId == pi && m.AttendanceDateTime.Date == NextDate);
                                    foreach (var item in attlog)
                                    {
                                        filterAttendanceLog.Add(new FilterAttendanceLog()
                                        {
                                            MachineNumber = item.MachineNumber,
                                            PersonalId = item.PersonalId,
                                            AttendanceDate = item.AttendanceDate,
                                            AttendanceDateTime = item.AttendanceDateTime,
                                            InTime = item.InTime,
                                            OutTime = item.InTime
                                        });
                                    }

                                    if ((filterAttendanceLog != null))
                                    {
                                        FilterAttendanceLog TopData = new FilterAttendanceLog();
                                        FilterAttendanceLog LastData = new FilterAttendanceLog();

                                        TopData = filterAttendanceLog[0];
                                        entryData.Add(new EntryData()
                                        {
                                            MachineNumber = TopData.MachineNumber,
                                            PersonalId = TopData.PersonalId,
                                            AttendanceDate = TopData.AttendanceDate,
                                            AttendanceDateTime = TopData.AttendanceDateTime,
                                            InTime = TopData.InTime,
                                            OutTime = TopData.OutTime

                                        });

                                        LastData = filterAttendanceLog[filterAttendanceLog.Count - 1];
                                        leaveData.Add(new LeaveingData()
                                        {
                                            PersonalId = LastData.PersonalId,
                                            AttendanceDate = LastData.AttendanceDate,
                                            AttendanceDateTime = LastData.AttendanceDateTime,
                                            OutTime = LastData.InTime
                                        });

                                    }
                                }
                               
                            }

                            if (entryData.Count>0)
                            {
                                foreach (var e in entryData)
                                {
                                    finalEntryData.Add(new FinalEntryData()
                                    {
                                        MachineNumber = e.MachineNumber,
                                        PersonalId = e.PersonalId,
                                        AttendanceDate = e.AttendanceDate,
                                        AttendanceDateTime = e.AttendanceDateTime,
                                        InTime = e.InTime,
                                        OutTime = e.InTime
                                    });
                                }
                            }

                            if (leaveData.Count>0)
                            {
                                foreach (var l in leaveData)
                                {
                                    finalLeavingData.Add(new FinalLeaveingData()
                                    {
                                        PersonalId = l.PersonalId,
                                        AttendanceDate = l.AttendanceDate,
                                        AttendanceDateTime = l.AttendanceDateTime,
                                        OutTime = l.OutTime
                                    });
                                }
                            }
                        }


                        if ((finalEntryData != null && finalEntryData.Count > 0))
                            {
                                // BindToGridView(entryData);

                                foreach (FinalEntryData ed in finalEntryData)
                                {
                                    AttendanceLogDA.Insert(ed);
                                }

                            }
                        if (finalLeavingData != null && finalLeavingData.Count > 0)
                            {
                                //BindToGridView(Actual_levData);                       

                                foreach (FinalLeaveingData ld in finalLeavingData)
                                {
                                    AttendanceLogDA.Update(ld);
                                }

                            }
                        

                    }
                    AttendanceLogDA.InsertTimeRecord(currentDate, currentTime, currentDateTime);

                }
                else
                {
                    FirstRecordFetch();
                }
            }
            catch (Exception ex)
            {
               // DisplayListOutput(ex.Message);
            }
        }
        private void FirstRecordFetch()  //if system is fetching record for the first time
        {
            try
            {
                //ShowStatusBar(string.Empty, true);

                ICollection<MachineInfo> lstMachineInfo = manipulator.GetLogData(objZkeeper, int.Parse(tbxMachineNumber.Text.Trim()));
                List<AttendanceLog> model = new List<AttendanceLog>();


                DateTime currentDate = DateTime.Now.Date;
                DateTime currentDateTime = Convert.ToDateTime(DateTime.Now.ToString());
                DateTime currentTime = Convert.ToDateTime(DateTime.Now.ToLongTimeString());
                DateTime? NextDateTime = null;
                DateTime? NextDate = null;
                int DaysToUpdate = 0;

                foreach (var item in lstMachineInfo) //retrieve each item and assign to model
                {                   
                    model.Add(new AttendanceLog()
                    {
                        MachineNumber = item.MachineNumber,
                        PersonalId = item.IndRegID,
                        AttendanceDateTime = Convert.ToDateTime(item.DateTimeRecord),
                        AttendanceDate = (item.DateOnlyRecord),
                        InTime = Convert.ToDateTime(item.DateTimeRecord),
                        OutTime = Convert.ToDateTime(item.DateTimeRecord)

                    });
                }
                List<FilterAttendanceLog> filterAttendanceLog = new List<FilterAttendanceLog>();
                foreach (var item in model)
                {
                    filterAttendanceLog.Add(new FilterAttendanceLog()
                    {
                        MachineNumber = item.MachineNumber,
                        PersonalId = item.PersonalId,
                        AttendanceDate = item.AttendanceDate,
                        AttendanceDateTime = item.AttendanceDateTime,
                        InTime = item.InTime,
                        OutTime = item.InTime
                    });
                }

                var LowestData = filterAttendanceLog[filterAttendanceLog.Count - 1];
                DateTime StartDate = LowestData.InTime;

                DaysToUpdate = currentDateTime.Subtract(StartDate).Days;

                for (int day = 0; day <= DaysToUpdate; day++)
                {
                    NextDateTime = StartDate.AddDays(day);
                    NextDate = StartDate.AddDays(day).Date;
                    List<FinalEntryData> finalEntryData = new List<FinalEntryData>();
                    List<FinalLeaveingData> finalLeaveData = new List<FinalLeaveingData>();

                    DataTable personalId = AttendanceLogDA.GetPersonalIdFromPersonalDetail();
                    foreach (DataRow pid in personalId.Rows)
                    {
                        var pi = (int)pid["PersonalId"];

                        if (model.FindAll(m => m.PersonalId == pi && m.AttendanceDateTime.Date == NextDate).Count > 0)
                        {
                            var attlog = model.FindAll(m => m.PersonalId == pi && m.AttendanceDateTime.Date == NextDate);
                            foreach (var item in attlog)
                            {
                                filterAttendanceLog.Add(new FilterAttendanceLog()
                                {
                                    MachineNumber = item.MachineNumber,
                                    PersonalId = item.PersonalId,
                                    AttendanceDate = item.AttendanceDate,
                                    AttendanceDateTime = item.AttendanceDateTime,
                                    InTime = item.InTime,
                                    OutTime = item.InTime
                                });
                            }

                            if ((filterAttendanceLog != null))
                            {
                                FilterAttendanceLog TopData = new FilterAttendanceLog();
                                FilterAttendanceLog LastData = new FilterAttendanceLog();

                                TopData = filterAttendanceLog[0];
                                finalEntryData.Add(new FinalEntryData()
                                {
                                    MachineNumber = TopData.MachineNumber,
                                    PersonalId = TopData.PersonalId,
                                    AttendanceDate = TopData.AttendanceDate,
                                    AttendanceDateTime = TopData.AttendanceDateTime,
                                    InTime = TopData.InTime,
                                    OutTime = TopData.OutTime

                                });

                                LastData = filterAttendanceLog[filterAttendanceLog.Count - 1];
                                finalLeaveData.Add(new FinalLeaveingData()
                                {
                                    PersonalId = LastData.PersonalId,
                                    AttendanceDate = LastData.AttendanceDate,
                                    AttendanceDateTime = LastData.AttendanceDateTime,
                                    OutTime = LastData.InTime
                                });

                            }
                        }


                    }
                    if (finalEntryData != null && finalEntryData.Count > 0)
                        {
                            // BindToGridView(entryData);

                            foreach (FinalEntryData ed in finalEntryData)
                            {
                                AttendanceLogDA.Insert(ed);
                            }

                        }
                    if (finalLeaveData != null && finalLeaveData.Count > 0)
                        {
                            //BindToGridView(Actual_levData);                       

                            foreach (FinalLeaveingData ld in finalLeaveData)
                            {
                                AttendanceLogDA.Update(ld);
                            }

                        }
                }
                AttendanceLogDA.InsertTimeRecord(currentDate, currentTime, currentDateTime);      
                
            }
            catch (Exception ex)
            {
               // DisplayListOutput(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                MakeAttendanceLog();
               // ShowStatusBar(" records found and saved to database !!", true);
            }
            catch(Exception ex)
            {
                //DisplayListOutput(ex.Message);
            }
        }


    }      
    
}