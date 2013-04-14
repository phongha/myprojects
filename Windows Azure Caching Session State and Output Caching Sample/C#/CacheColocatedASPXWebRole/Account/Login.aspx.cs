using System;


using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace CacheColocatedASPXWebRole.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterHyperLink.NavigateUrl = "Register.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
        }

        protected void UserLoggedIn(object sender, EventArgs e)
        {
            // This is to demonstrate use of caching provided by session state provider

            MembershipUser memberDetails = Membership.GetUser(LoginUser.UserName);

            UserDetails userDetails = new UserDetails();
            userDetails.userName = LoginUser.UserName;
            userDetails.eMail = memberDetails.Email;
            userDetails.loggedInTime = DateTime.UtcNow;
            Session["User"] = userDetails;
        }

    }
}
