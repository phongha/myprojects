using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MvcWebRole.Models;

namespace AzureEmailServiceControllers
{
    public class SubscribeController : Controller
    {
        private CloudTable mailingListTable;

        public SubscribeController()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            // If this is running in a Windows Azure Web Site (not a Cloud Service) use the Web.config file:
            //    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            // Get context object for working with tables.
            var tableClient = storageAccount.CreateCloudTableClient();
            mailingListTable = tableClient.GetTableReference("mailinglist");
        }

        private MailingList FindRow(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<MailingList>(partitionKey, rowKey);
            var retrievedResult = mailingListTable.Execute(retrieveOperation);
            var mailingList = retrievedResult.Result as MailingList;
            if (mailingList == null)
            {
                throw new Exception("No mailing list found for: " + partitionKey);
            }
            return mailingList;
        }
        
        // GET: /Subscribe/

        public ActionResult Index(string id, string listName)
        {
            // We get to this method when they click on the Confirm link in the
            // email that's sent to them after the subscribe service method is called.
            string filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, listName),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("SubscriberGUID", QueryComparisons.Equal, id));
            var query = new TableQuery<Subscriber>().Where(filter);
            var subscriber = mailingListTable.ExecuteQuery(query).ToList().Single();

            //subscriberTableRow.Status = "Verified";
            subscriber.Verified = true;
            var replaceOperation = TableOperation.Merge(subscriber);
            mailingListTable.Execute(replaceOperation);

            var newSubscriber = new SubscribeVM();
            newSubscriber.EmailAddress = subscriber.EmailAddress;
            newSubscriber.ListDescription = FindRow(subscriber.ListName, "mailinglist").Description;
            return View(newSubscriber);
        }
    }
}