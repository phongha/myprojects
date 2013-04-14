using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace CacheColocatedASPXWebRole.Account
{
    public partial class Profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            UserDetails userDetails = Session["User"] as UserDetails;
            if (null == userDetails)
            {
                Response.Redirect("Login.aspx");
            }

           
            // This is to demonstrate use of caching provided by session state provider
            string email = userDetails.eMail;
            this.UserNameLabel.Text = userDetails.userName;
            this.EmailLabel.Text = email;
            this.LoginTimeLable.Text = userDetails.loggedInTime.ToUniversalTime().ToString();
            this.PageSaveTimeLable.Text = DateTime.UtcNow.ToString();
        }
    }
}
