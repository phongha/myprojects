using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MvcWebRole.Models;

//Don't review this, we'll make this a vnext function.

namespace AzureEmailServiceControllers
{
    public class UnsubscribeController : Controller
    {
        private CloudTable mailingListTable;

        public UnsubscribeController()
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

        public ActionResult Index(string id, string listName)
        {
            if (string.IsNullOrEmpty(id) == true || string.IsNullOrEmpty(listName))
            {
                ViewBag.errorMessage = "Empty subscriber ID or list name.";
                return View("Error");
            }
            string filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, listName),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("SubscriberGUID", QueryComparisons.Equal, id));
            var query = new TableQuery<Subscriber>().Where(filter);
            var subscriber = mailingListTable.ExecuteQuery(query).ToList().Single();

            if (subscriber == null)
            {
                ViewBag.Message = "You are already unsubscribed";
                return View("Message");
            }

            var unsubscribeVM = new UnsubscribeVM();
            unsubscribeVM.EmailAddress = MaskEmail(subscriber.EmailAddress);
            unsubscribeVM.ListDescription = FindRow(subscriber.ListName, "mailinglist").Description;
            unsubscribeVM.SubscriberGUID = id;
            unsubscribeVM.Confirmed = null;
            return View(unsubscribeVM);
        }

        [HttpPost] 
        [ValidateAntiForgeryToken]
        public ActionResult Index(string subscriberGUID, string listName, string action)
        {
            string filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, listName),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("SubscriberGUID", QueryComparisons.Equal, subscriberGUID));
            var query = new TableQuery<Subscriber>().Where(filter);
            var subscriber = mailingListTable.ExecuteQuery(query).ToList().Single();

            var unsubscribeVM = new UnsubscribeVM();
            unsubscribeVM.EmailAddress = MaskEmail(subscriber.EmailAddress);
            unsubscribeVM.ListDescription = FindRow(subscriber.ListName, "mailinglist").Description;
            unsubscribeVM.SubscriberGUID = subscriberGUID;
            unsubscribeVM.Confirmed = false;

            if (action == "Confirm")
            {
                unsubscribeVM.Confirmed = true;
                var deleteOperation = TableOperation.Delete(subscriber);
                mailingListTable.Execute(deleteOperation);
            }

            return View(unsubscribeVM);
        }

        private string MaskEmail(string emailAddress)
        {
            string newEmailAddress = string.Empty;
            bool atFound = false;
            for (int i = 0; i < emailAddress.Length; i++)
            {
                var currentChar = emailAddress.Substring(i, 1);

                // Show first two characters
                if (i < 2)
                {
                    newEmailAddress += currentChar;
                    continue;
                }

                if (currentChar == "@")
                {
                    newEmailAddress += currentChar;
                    atFound = true;
                    continue;
                }

                // copy remaining substring after @ so we get ".com" or ".org"
                if (atFound == true && currentChar == ".")
                {
                    var x = emailAddress.Substring(i);
                    newEmailAddress += emailAddress.Substring(i);
                    break;
                }

                newEmailAddress += "*";
            }
            return newEmailAddress;
        }
    }
}