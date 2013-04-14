<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Profile.aspx.cs" Inherits="CacheColocatedASPXWebRole.Account.Profile" %>
    <%@ OutputCache Duration="30" VaryByParam="none" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Your Profile
    </h2>
    <p>
       User&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; :&nbsp;&nbsp;<asp:Label ID="UserNameLabel" runat="server"> </asp:Label> <br />
       Email&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; :&nbsp;&nbsp;<asp:Label ID="EmailLabel" runat="server"></asp:Label><br />
       Loggin Time (UTC)&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; :&nbsp;&nbsp;<asp:Label ID="LoginTimeLable" runat="server"></asp:Label><br />
       Page Save Time (UTC) :&nbsp;&nbsp;<asp:Label ID="PageSaveTimeLable" runat="server"></asp:Label><br />
    </p>
</asp:Content>
