using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using BioMetrixCore.ViewModel;
using System.Data.SqlClient;
using System.Data;
namespace BioMetrixCore.MainFile
{
    class MainClass:Form
    {
        DeviceManipulator manipulator = new DeviceManipulator();
        public ZkemClient objZkeeper;
        private Button AutoAttendancePuller;
         private bool isDeviceConnected = true;
        
        // BLLPullAttendance blu = new BLLPullAttendance();

         public bool IsDeviceConnected
         {
             get { return isDeviceConnected; }
             set
             {
                 isDeviceConnected = value;
                 if (isDeviceConnected)
                 {
                     //ShowStatusBar("The device is connected !!", true);
                     //btnConnect.Text = "Disconnect";
                     //ToggleControls(true);
                 }
                 else
                 {
                     //ShowStatusBar("The device is diconnected !!", true);
                     //objZkeeper.Disconnect();
                     //btnConnect.Text = "Connect";
                     //ToggleControls(false);
                 }
             }
         }
            private void RaiseDeviceEvent(object sender, string actionType)
        {
            switch (actionType)
            {
                case UniversalStatic.acx_Disconnect:
                    {
                        //ShowStatusBar("The device is switched off", true);
                        //DisplayEmpty();
                        //btnConnect.Text = "Connect";
                        //ToggleControls(false);
                        break;
                    }

                default:
                    break;
            }

        }

        private void MultipleDeviceProcessor()
        {
           
            try
            {
                //Updating status of DV_Device
                DataTable ListOfDevice = AttendanceLogDA.GetListOfDevice_From_Device();
                foreach (DataRow lstDev in ListOfDevice.Rows)
                {
                    
                    string ipAddress = ((string)lstDev["IPAddress"]).Trim();
                    int port = ((int)lstDev["Port"]);
                    string machineNumber = ((string)lstDev["DeviceId"]).Trim();
                    int portNumber = 4370;
                   

                    if (ipAddress != string.Empty && port==portNumber)
                    {
                        bool isValidIpA = UniversalStatic.ValidateIP(ipAddress);
                        if (isValidIpA)
                        {
                            isValidIpA = UniversalStatic.PingTheDevice(ipAddress);
                            if (isValidIpA)
                            {
                                objZkeeper = new ZkemClient(RaiseDeviceEvent);
                                IsDeviceConnected = objZkeeper.Connect_Net(ipAddress, portNumber);

                                if (IsDeviceConnected)
                                {
                                   // string deviceInfo = manipulator.FetchDeviceInfo(objZkeeper, int.Parse(machineNumber));
                                    //lblDeviceInfo.Text = deviceInfo;
                                    MakeAttendanceLog(machineNumber);
                                }
                            }
                        }
                    }

                    //this.Cursor = Cursors.Default;

                    //After connection completed

                                    


                }                
               
                //ShowStatusBar(" records found and saved to database !!", true);
            }
            catch (Exception ex)
            {
                // Device connect bhayena bhane notification dekhaune
            }
        }
        private void MakeAttendanceLog(string machineNumber)  //if DV_AttendanceLog has previous record
        {
            try
            {            
                ICollection<MachineInfo> lstMachineInfo = manipulator.GetLogData(objZkeeper, int.Parse(machineNumber));
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
                                                //OutTime = item.OutTime
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
                                                // OutTime = item.InTime
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
                                            //OutTime = item.InTime
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

                            if (entryData.Count > 0)
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
                                        //OutTime = e.OutTime
                                    });
                                }
                            }

                            if (leaveData.Count > 0)
                            {
                                foreach (var l in leaveData)
                                {
                                    finalLeavingData.Add(new FinalLeaveingData()
                                    {
                                        PersonalId = l.PersonalId,
                                        AttendanceDate = l.AttendanceDate,
                                        AttendanceDateTime = l.AttendanceDateTime,
                                        OutTime = l.InTime
                                    });
                                }
                            }
                        }


                        if ((finalEntryData != null && finalEntryData.Count > 0))
                        {
                            foreach (FinalEntryData ed in finalEntryData)
                            {
                                AttendanceLogDA.Insert(ed);
                            }
                        }
                        if (finalLeavingData != null && finalLeavingData.Count > 0)
                        {
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
                    FirstRecordFetch(machineNumber);
                }
            }
            catch (Exception ex)
            {
                //DisplayListOutput(ex.Message);
            }
        }
        private void FirstRecordFetch(string machineNumber)  //if system is fetching record for the first time
        {
            try
            {
                ICollection<MachineInfo> lstMachineInfo = manipulator.GetLogData(objZkeeper, int.Parse(machineNumber));
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
                        foreach (FinalEntryData ed in finalEntryData)
                        {
                            AttendanceLogDA.Insert(ed);
                        }
                    }
                    if (finalLeaveData != null && finalLeaveData.Count > 0)
                    {
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

        //private void InitializeComponent()
        //{
        //    this.SuspendLayout();
        //    // 
        //    // MainClass
        //    // 
        //    this.ClientSize = new System.Drawing.Size(282, 253);
        //    this.Name = "MainClass";
        //    this.Load += new System.EventHandler(this.MainClass_Load);
        //    this.ResumeLayout(false);
        //  //  MultipleDeviceProcessor();

        //}

        private void MainClass_Load(object sender, EventArgs e)
        {
            
        }

        private void InitializeComponent()
        {
            this.AutoAttendancePuller = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AutoAttendancePuller
            // 
            this.AutoAttendancePuller.Location = new System.Drawing.Point(56, 70);
            this.AutoAttendancePuller.Name = "AutoAttendancePuller";
            this.AutoAttendancePuller.Size = new System.Drawing.Size(194, 23);
            this.AutoAttendancePuller.TabIndex = 0;
            this.AutoAttendancePuller.Text = "AutoAttendancePuller";
            this.AutoAttendancePuller.UseVisualStyleBackColor = true;
            this.AutoAttendancePuller.Click += new System.EventHandler(this.AutoAttendancePuller_Click);
            // 
            // MainClass
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Controls.Add(this.AutoAttendancePuller);
            this.Name = "MainClass";
            this.Load += new System.EventHandler(this.AutoAttendancePuller_Click);
            this.ResumeLayout(false);

        }

        private void AutoAttendancePuller_Click(object sender, EventArgs e)
        {
            MultipleDeviceProcessor();
        }
    }
}
