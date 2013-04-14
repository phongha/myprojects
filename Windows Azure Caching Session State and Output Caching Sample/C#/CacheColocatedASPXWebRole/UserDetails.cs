using System;
namespace CacheColocatedASPXWebRole
{
    /// <summary>
    /// User Details
    /// </summary>
    /// <remarks>This is used for storing user information in session state</remarks>
    public class UserDetails
    {
        /// <summary>
        /// User Name
        /// </summary>
        public string userName;

        /// <summary>
        /// E-Mail Address
        /// </summary>
        public string eMail;

        /// <summary>
        /// Last Logged In Time
        /// </summary>
        public DateTime loggedInTime;
    }

}