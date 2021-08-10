<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs" Inherits="MusicTuitionAttendance.Attendance" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .heading4 {
            text-decoration: underline;
        }

        .auto-style1 {
            font-weight: normal;
        }

        #StudentsGridView {
            text-align: left;
        }

        .divider{
            width:5px;
            height:auto;
            display:inline-block;
        }

        .attendanceButton {
            margin-left:20px;
            width: 100px;
            font-size: 1.2em
        }
    </style>
</head>
<body style="font-weight: 700; font-size: 1.5em">

    <form id="form1" runat="server">

        <asp:ScriptManager ID="MyScriptManager" runat="server"></asp:ScriptManager>
        <asp:Timer ID="UserActivityTimer" runat="server" Interval="60000" OnTick="UserActivityTimer_Tick" Enabled="False"></asp:Timer>

        <div>
            <table style="width: 750px">
                <tr>
                    <td>
                        <p>
                            <asp:Label ID="TutorNameTitle" runat="server" Text="Tutor name: " Font-Bold="True"></asp:Label>
                            <asp:DropDownList ID="TutorNameLbx" runat="server" style="width:350px;font-size: 1em;" AutoPostBack="True" OnSelectedIndexChanged="TutorNameLbx_SelectedIndexChanged" ></asp:DropDownList>
                        </p>
                    </td>
                    <td style="text-align: right">
                        <asp:Button runat="server" ID="LogoutBtn" Text="Logout" Font-Bold="true" OnClick="LogoutBtn_Click" CausesValidation="False" style= "font-size: 1em;"/>
                    </td>
                </tr>
            </table>
        </div>
        <asp:GridView ID="StudentsGridView" runat="server"
            AutoGenerateColumns="False"
            DataKeyNames="StudentId"
            OnRowCommand="StudentsGridView_RowCommand"
            style="width:750px" 
            Font-Size="Large" 
            CellPadding="5"
            Showheader="false"
            Visible="False">
            <EmptyDataTemplate>
                <p style="font-weight: bold; color: #CC0000;font-size:1em">
                    No students found for this tutor! 
                </p>
            </EmptyDataTemplate>
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Label ID="StudentNameLbl" Text='<%# Eval("StudentName").ToString() %>'
                            runat="server" Width="350px" style="font-size: 1.5em" Font-Bold="False"></asp:Label>
                        <asp:Button ID="PresentBtn" runat="server" 
                            CssClass="attendanceButton" 
                            CausesValidation="false" 
                            CommandName="Present"
                            Text="Present" 
                            CommandArgument='<%# Eval("StudentId") %>' />
                        <asp:Button ID="LateBtn" runat="server" 
                            CssClass="attendanceButton" 
                            CausesValidation="false" 
                            CommandName="Late"
                            Text="Late" 
                            CommandArgument='<%# Eval("StudentId") %>' />
                        <asp:Button ID="AbsentBtn" runat="server" 
                            CssClass="attendanceButton" 
                            CausesValidation="false" 
                            CommandName="Absent"
                            Text="Absent" 
                            CommandArgument='<%# Eval("StudentId") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <br />
        <div id="NewStudentAttedanceDiv" runat="server" visible="false">
            <strong>Student not on list above? </strong><span class="auto-style1">Enter name below. </span>
            <br />
            <asp:TextBox ID="NewStudentNameTbx" runat="server"
                Font-Size="Large"
                Width="390px"
                style="font-size: 1em"></asp:TextBox>
            <asp:Button ID="NewStudentPresentBtn" runat="server" 
                OnClick="NewStudentBtn_Click" 
                CssClass="attendanceButton" 
                Text="Present" 
                CausesValidation="true" 
                style="font-size:0.9em"
                CommandName="Present" />
            <asp:Button ID="NewStudentLateBtn" runat="server" 
                OnClick="NewStudentBtn_Click" 
                CssClass="attendanceButton" 
                Text="Late" 
                style="font-size:0.9em"
                CausesValidation="true" 
                CommandName="Late" />
            <asp:Button ID="NewStudentAbsentBtn" runat="server" 
                OnClick="NewStudentBtn_Click" 
                CssClass="attendanceButton" 
                style="font-size:0.9em"
                Text="Absent" 
                CausesValidation="true" 
                CommandName="Absent" />
            <br />
            <asp:RequiredFieldValidator ID="NewStudentNameVld" runat="server" 
                ControlToValidate="NewStudentNameTbx" Font-Bold="True" ForeColor="#CC0000">
                * Please enter a name for the current student!</asp:RequiredFieldValidator>
        </div>
        <p>
            Problem? 
            <span class="auto-style1">Contact Data Management [<% DataManagementLnk.NavigateUrl = "mailto:" + ConfigurationManager.AppSettings["dataManagementEmail"]; %>
                <asp:HyperLink ID="DataManagementLnk" NavigateUrl="mailto:" runat="server">
                    <%= ConfigurationManager.AppSettings["dataManagementEmail"].ToString() %></asp:HyperLink>] for assistance.
            </span>
        </p>
        <span class="heading4">Message Log</span><asp:GridView ID="MessageLogGridView" runat="server" AutoGenerateColumns="False" ForeColor="#666666" GridLines="None" ShowHeader="False">
            <Columns>
                <asp:BoundField DataField="Message">
                    <ItemStyle Font-Bold="False" />
                </asp:BoundField>
            </Columns>
        </asp:GridView>
    </form>
</body>
</html>
