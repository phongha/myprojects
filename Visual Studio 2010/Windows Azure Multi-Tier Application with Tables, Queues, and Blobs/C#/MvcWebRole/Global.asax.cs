using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace MvcWebRole
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            // Verify that all of the tables, queues, and blob containers used in this application
            // exist, and create any that don't already exist.
            CreateTablesQueuesBlobContainers();
        }

        private static void CreateTablesQueuesBlobContainers()
        {
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            // If this is running in a Windows Azure Web Site (not a Cloud Service) use the Web.config file:
            //    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            var tableClient = storageAccount.CreateCloudTableClient();
            var mailingListTable = tableClient.GetTableReference("mailinglist");
            mailingListTable.CreateIfNotExists();

            var messageTable = tableClient.GetTableReference("message");
            messageTable.CreateIfNotExists();

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("azuremailblobcontainer");
            blobContainer.CreateIfNotExists();

            var queueClient = storageAccount.CreateCloudQueueClient();
            var subscribeQueue = queueClient.GetQueueReference("azuremailsubscribequeue");
            subscribeQueue.CreateIfNotExists();
        }
    }
}