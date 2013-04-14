<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="CacheColocatedASPXWebRole._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Co-located Caching Topology,<br /> ASP.NET Session State &amp; output caching</h2>
    This application demonstrates the use of Caching (Preview)
    for the following scenarios:
    <ul>
        <li>Caching (Preview) hosted on an existing web role (co-located topology).</li>
        <li>ASP.NET Session State Caching: &quot;Log In&quot;, to see the &quot;Profile&quot; info (in 
            the &quot;Profile&quot; 
            tab) being stored and retrieved from session state.</li>
        <li>ASP.NET Page Output Caching: After logon, &quot;Profile&quot; page is cached in output cache for 
            30 seconds. &quot;Page Save Time&quot; value changes only after 30 seconds, 
            while 
            switching between &quot;Home&quot; and &quot;Profile&quot; tabs.</li>
    </ul>
    </asp:Content>
