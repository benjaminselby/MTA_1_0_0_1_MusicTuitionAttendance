using System;
using System.Diagnostics;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using ApplicationCode;
using System.Data;
using System.Collections.Generic;

namespace MusicTuitionAttendance
{
    public partial class Attendance : System.Web.UI.Page
    {
        private readonly int userActivityTimeoutMin = 60;
        private readonly string staffListBoxDefaultValueTxt = "Please select your name.";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack == false)
            {
                PopulateTutorList();
                TutorNameLbx.SelectedIndex = 0;
            }
        }


        private void PopulateTutorList()
        {
            TutorNameLbx.Items.Clear();

            TutorNameLbx.Items.Add(new ListItem(staffListBoxDefaultValueTxt, "0"));

            using (SqlConnection synergyConnection = new SqlConnection())
            {
                try
                {
                    synergyConnection.ConnectionString =
                        ConfigurationManager.ConnectionStrings["Synergy"].ConnectionString;

                    using (SqlCommand staffListCmd = new SqlCommand(
                            ConfigurationManager.AppSettings["getStaffListProc"],
                            synergyConnection))
                    {
                        staffListCmd.CommandType = System.Data.CommandType.StoredProcedure;

                        using (SqlDataAdapter staffListAdapter = new SqlDataAdapter(staffListCmd))
                        {
                            synergyConnection.Open();

                            DataSet staffList = new DataSet();
                            staffListAdapter.Fill(staffList);

                            if (staffList.Tables[0].Rows.Count == 0)
                            {
                                TutorNameLbx.Items.Add("No staff found - contact Data Management");
                                TutorNameLbx.SelectedIndex = 0;
                            }
                            else
                            {
                                // We have to add items manually rather than binding to the dataset because 
                                // we want the custom "Please select your name..." item at first position. 
                                foreach(DataRow row in staffList.Tables[0].Rows)
                                {
                                    TutorNameLbx.Items.Add(new ListItem(
                                        row["StaffName"].ToString(), row["StaffId"].ToString()));

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);
                    Response.Redirect("./Error.aspx");
                    //throw (ex);
                }
                finally
                {
                    synergyConnection.Close();
                }
            }
        }



        private void PopulateStudentList()
        {
            using (SqlConnection synergyOneConnection = new SqlConnection())
            {
                try
                {
                    synergyOneConnection.ConnectionString =
                        ConfigurationManager.ConnectionStrings["Synergy"].ConnectionString;

                    using (SqlCommand studentListCmd = new SqlCommand(
                            ConfigurationManager.AppSettings["studentsForStaffProc"],
                            synergyOneConnection))
                    {
                        studentListCmd.CommandType = System.Data.CommandType.StoredProcedure;
                        string currentUserId = (Session["currentUser"] as User).id;
                        studentListCmd.Parameters.AddWithValue("StaffId", currentUserId);

                        using (SqlDataAdapter studentsAdapter = new SqlDataAdapter(studentListCmd))
                        {
                            synergyOneConnection.Open();

                            DataSet students = new DataSet();
                            studentsAdapter.Fill(students);

                            if(students.Tables[0].Rows.Count == 0)
                            {
                                NewStudentAttedanceDiv.Visible = false;
                            }

                            StudentsGridView.DataSource = students;
                            StudentsGridView.DataBind();

                            Session["studentList"] = students;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);
                    Response.Redirect("./Error.aspx");
                    //throw (ex);
                }
                finally
                {
                    synergyOneConnection.Close();
                }
            }
        }


        private void PopulateMessageLog()
        {
            MessageLogGridView.DataSource = null;
            
            using (SqlConnection synergyOneConnection = new SqlConnection())
            {
                try
                {
                    synergyOneConnection.ConnectionString =
                        ConfigurationManager.ConnectionStrings["Synergy"].ConnectionString;
                    
                    using (SqlCommand messageLogCommand = new SqlCommand(
                        ConfigurationManager.AppSettings["attendanceLogForStaffTodayProc"],
                        synergyOneConnection))
                    {
                        messageLogCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        string currentUserId = (Session["currentUser"] as User).id;
                        messageLogCommand.Parameters.AddWithValue("StaffId", currentUserId);

                        DataSet messageLog = new DataSet();
                        using (SqlDataAdapter messageLogAdapter = new SqlDataAdapter())
                        {
                            messageLogAdapter.SelectCommand = messageLogCommand;
                            synergyOneConnection.Open();
                            messageLogAdapter.Fill(messageLog);
                        };
                        MessageLogGridView.DataSource = messageLog;
                        MessageLogGridView.DataBind();

                        Session["messageLog"] = messageLog;
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);
                    Response.Redirect("./Error.aspx");
                    //throw (ex);
                }
                finally
                {
                    synergyOneConnection.Close();
                }
            }
        }


        private void WriteLogMessage(string type, string message)
        {
            SqlConnection synergyOneConnection = new SqlConnection();
            try
            {
                synergyOneConnection.ConnectionString =
                    ConfigurationManager.ConnectionStrings["Synergy"].ConnectionString;

                using (SqlCommand messageLogCmd = new SqlCommand(
                    ConfigurationManager.AppSettings["attendanceLogAddMessageProc"], synergyOneConnection))
                {
                    messageLogCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    string currentUserId = (Session["currentUser"] as User).id;
                    messageLogCmd.Parameters.AddWithValue("StaffId", currentUserId);
                    messageLogCmd.Parameters.AddWithValue("Type", type);
                    messageLogCmd.Parameters.AddWithValue("Message", message);

                    synergyOneConnection.Open();

                    int rowsUpdated = messageLogCmd.ExecuteNonQuery();
                    // TODO: Confirm rows updated == 1 here? 
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError(ex.Message, true);
                Response.Redirect("./Error.aspx");
                //throw (ex);
            }
            finally
            {
                synergyOneConnection.Close();
            }
        }


        protected void RecordAttendance(string staffId, string studentId, string status)
        {
            using (SqlConnection synergyOneConnection = new SqlConnection())
            {
                string currentUserId = (Session["currentUser"] as User).id;
                string currentUserFullName = (Session["currentUser"] as User).fullName;

                string studentName;
                if (studentId == null)
                {
                    studentName = NewStudentNameTbx.Text;
                }
                else
                {
                    DataTable students = (Session["studentList"] as DataSet).Tables[0];
                    DataRow[] foundRows = students.Select("StudentId = " + studentId);

                    if(foundRows.Length != 1)
                    {
                        throw new Exception(String.Format(
                            "Non-unique or non-matching key value ID:{0} in STUDENTS data table query.",
                            studentId));
                    }

                    studentName = foundRows[0][1].ToString();
                }

                studentId = studentId ?? "UNKNOWN";

                string attendanceMessage = String.Format(
                    "{0} - {1} [ID:{2}] marked as {3} for music tuition class with {4} [ID:{5}].",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    studentName,
                    studentId,
                    status.ToUpper(),
                    currentUserFullName,
                    currentUserId);

                NotifyStudentServices(attendanceMessage);

                // Log the attendance event in database table. 
                try
                {
                    synergyOneConnection.ConnectionString =
                        ConfigurationManager.ConnectionStrings["Synergy"].ConnectionString;

                    using (SqlCommand messageLogCmd = new SqlCommand(
                        ConfigurationManager.AppSettings["attendanceLogAddEventProc"], synergyOneConnection))
                    {
                        messageLogCmd.CommandType = System.Data.CommandType.StoredProcedure;
                        messageLogCmd.Parameters.AddWithValue("StaffId", staffId);
                        messageLogCmd.Parameters.AddWithValue("StudentId", studentId);
                        messageLogCmd.Parameters.AddWithValue("DateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        messageLogCmd.Parameters.AddWithValue("Status", status);

                        synergyOneConnection.Open();

                        int rowsUpdated = messageLogCmd.ExecuteNonQuery();
                        // TODO: Confirm rows updated == 1 here? 
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);
                    Response.Redirect("./Error.aspx");
                    //throw (ex);
                }
                finally
                {
                    synergyOneConnection.Close();
                }
            }
        }


        private void LogoutUser()
        {
            PopulateTutorList();
            TutorNameLbx.SelectedIndex = 0;
            TutorNameLbx_SelectedIndexChanged(TutorNameLbx, null);
        }


        private void NotifyStudentServices(string message)
        {
            string currentUserEmail = (Session["currentUser"] as User).emailAddress;

            // Send notification email. 
            MailHandler.SendMail(
                currentUserEmail,
                ConfigurationManager.AppSettings["studentServicesEmailAddresses"],
                "Music Tuition Attendance Notification",
                message);

            // Write log message and refresh the contents of the message log page element. 
            WriteLogMessage("EMAIL", message);
            PopulateMessageLog();
        }



        // ========================================================================================
        // EVENT HANDLERS
        // ========================================================================================



        protected void StudentsGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Present" || e.CommandName == "Late" || e.CommandName == "Absent")
            {
                string studentId = e.CommandArgument.ToString();
                string currentUserId = (Session["currentUser"] as User).id;
                RecordAttendance(currentUserId, studentId, e.CommandName.ToUpper());
                Session["UserActivityTimeout"] = DateTime.Now.AddMinutes(userActivityTimeoutMin);
            }
        }


        protected void NewStudentBtn_Click(object sender, EventArgs e)
        {
            /* It is hoped that this can be avoided as much as possible because we have no way 
             * of knowing the student ID etc. Music tutors are unreliable and often don't provide 
             * us with good information regarding their students, so we need a provision for students 
             * who the tutor has accepted but we have not been notified of. */

            string senderAddress, currentUserId, currentUserEmail, currentUserFullName;

            if (!Page.IsValid)
            {                
                // User has not entered the name of the new student. Do nothing and let the 
                // field validator in the page inform the user of their error. 
                return;
            }

            currentUserId = (Session["currentUser"] as User).id;
            currentUserEmail = (Session["currentUser"] as User).emailAddress;
            currentUserFullName = (Session["currentUser"] as User).fullName;

            RecordAttendance(currentUserId, null, ((Button)sender).CommandName.ToUpper());

            senderAddress = (currentUserEmail == null || currentUserEmail == "") ?
                ConfigurationManager.AppSettings["dataManagementEmail"] :
                currentUserEmail;

            // Notify relevant staff that a new student has been marked as attending music tuition.
            // =====================================================================================
            // =============================== ENABLE FOR PRODUCTION =============================== 
            //List<string> emailRecipients = new List<string> {
            //    ConfigurationManager.AppSettings["dataManagementEmail"],
            //    ConfigurationManager.AppSettings["studentManagementEmail"]};
            List<string> emailRecipients = new List<string> {
                ConfigurationManager.AppSettings["dataManagementEmail"]};
            // =====================================================================================
            // =====================================================================================

            string attendanceMessage = String.Format(
                "{0} - {1} [ID:UNKNOWN] marked as {2} for music tuition class with {3} [ID:{4}].",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                NewStudentNameTbx.Text,
                ((Button)sender).CommandName.ToUpper(),
                currentUserFullName,
                currentUserId);

            MailHandler.SendMail(
                senderAddress,
                emailRecipients,
                "New student added to Music Tuition Attendance system.",
                attendanceMessage);

            Session["UserActivityTimeout"] = DateTime.Now.AddMinutes(userActivityTimeoutMin);
        }


        protected void TutorNameLbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TutorNameLbx.SelectedValue == "0")
            {
                // Zero is the value we use for the default status message in the staff list box. 
                // Selecting it equates to selecting nobody. 
                Session["CurrentUser"] = null;
                StudentsGridView.DataSource = null;
                StudentsGridView.Visible = false;
                NewStudentAttedanceDiv.Visible = false;
                MessageLogGridView.Visible = false;

                UserActivityTimer.Enabled = false;
                Session["UserActivityTimeout"] = null;
            }
            else
            {
                try
                {
                    User currentUser = new User(int.Parse(TutorNameLbx.SelectedValue));
                    Session["CurrentUser"] = currentUser;

                    PopulateStudentList();
                    PopulateMessageLog();

                    StudentsGridView.Visible = true;
                    NewStudentAttedanceDiv.Visible = true;
                    MessageLogGridView.Visible = true;

                    UserActivityTimer.Enabled = true;
                    Session["UserActivityTimeout"] = DateTime.Now.AddMinutes(userActivityTimeoutMin);
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError(ex.Message, true);
                    Response.Redirect("./Error.aspx");
                    //throw (ex);
                }
            }
        }

        protected void LogoutBtn_Click(object sender, EventArgs e)
        {
            LogoutUser();
            UserActivityTimer.Enabled = false;
            Session["UserActivityTimeout"] = null;
        }

        protected void UserActivityTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= (DateTime)Session["UserActivityTimeout"])
            {
                LogoutUser();
            }
        }
    }
}
