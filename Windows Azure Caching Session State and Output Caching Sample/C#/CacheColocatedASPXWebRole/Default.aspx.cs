using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Diagnostics;
using System.Web.Configuration;
using System.Data.SqlClient;
using Microsoft.ApplicationServer.Caching;

namespace CacheColocatedASPXWebRole
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MembershipUser memberDetails = Membership.GetUser(Context.User.Identity.Name);
            if (null != memberDetails)
            {
                UserDetails userDetails = new UserDetails();
                userDetails.userName = memberDetails.UserName;
                userDetails.eMail = memberDetails.Email;
                userDetails.loggedInTime = memberDetails.LastLoginDate;
                Session["User"] = userDetails;
            }
        }
    }
}
