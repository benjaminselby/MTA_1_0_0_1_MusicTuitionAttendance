<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="MusicTuitionAttendance.Error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <p style="color:red;font-weight:bold;font-size:1.5em">An error occurred. Please contact <a href="mailto:<%= ConfigurationManager.AppSettings["dataManagementEmail"] %>">Data Management</a> for assistance.</p>
        </div>        
    </form>
</body>
</html>
