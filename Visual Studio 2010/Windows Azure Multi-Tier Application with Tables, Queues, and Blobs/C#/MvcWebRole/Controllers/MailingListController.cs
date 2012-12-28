using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using MvcWebRole.Models;
using System.Diagnostics;

namespace AzureEmailServiceControllers
{
    public class MailingListController : Controller
    {
        private CloudTable mailingListTable;

        public MailingListController()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            // If this is running in a Windows Azure Web Site (not a Cloud Service) use the Web.config file:
            //    var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

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

        //
        // GET: /MailingList/

        public ActionResult Index()
        {
            TableRequestOptions reqOptions = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
            };
            List<MailingList> lists;
            try
            {
                var query = new TableQuery<MailingList>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "mailinglist"));
                lists = mailingListTable.ExecuteQuery(query, reqOptions).ToList();
            }
            catch (StorageException se)
            {
                ViewBag.errorMessage = "Timeout error, try again. ";
                Trace.TraceError(se.Message);
                return View("Error");
            }

            return View(lists);
        }

        //
        // GET: /MailingList/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /MailingList/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MailingList mailingList)
        {
            if (ModelState.IsValid)
            {
                var insertOperation = TableOperation.Insert(mailingList);
                mailingListTable.Execute(insertOperation);
                return RedirectToAction("Index");
            }

            return View(mailingList);
        }

        //
        // GET: /MailingList/Edit/5

        public ActionResult Edit(string partitionKey, string rowKey)
        {
            var mailingList = FindRow(partitionKey, rowKey);
            return View(mailingList);
        }

        //
        // POST: /MailingList/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string partitionKey, string rowKey, MailingList editedMailingList)
        {
            if (ModelState.IsValid)
            {
                var mailingList = new MailingList();
                UpdateModel(mailingList);
                try
                {
                    var replaceOperation = TableOperation.Replace(mailingList);
                    mailingListTable.Execute(replaceOperation);
                    return RedirectToAction("Index");
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == 412)
                    {
                        // Concurrency error
                        var currentMailingList = FindRow(partitionKey, rowKey);
                        if (currentMailingList.FromEmailAddress != editedMailingList.FromEmailAddress)
                        {
                            ModelState.AddModelError("FromEmailAddress", "Current value: " + currentMailingList.FromEmailAddress);
                        }
                        if (currentMailingList.Description != editedMailingList.Description)
                        {
                            ModelState.AddModelError("Description", "Current value: " + currentMailingList.Description);
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you got the original value. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to edit this record, click "
                            + "the Save button again. Otherwise click the Back to List hyperlink.");
                         ModelState.SetModelValue("ETag", new ValueProviderResult(currentMailingList.ETag, currentMailingList.ETag, null));
                    }
                    else
                    {
                        throw; 
                    }
                }
            }
            return View(editedMailingList);
        }

        //
        // GET: /MailingList/Delete/5

        public ActionResult Delete(string partitionKey, string rowKey)
        {
            var mailingList = FindRow(partitionKey, rowKey);
            return View(mailingList);
        }

        //
        // POST: /MailingList/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string partitionKey)
        {
            // Delete all rows for this mailing list, that is, 
            // Subscriber rows as well as MailingList rows.
            // Therefore, no need to specify row key.
            var query = new TableQuery<MailingList>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var listRows = mailingListTable.ExecuteQuery(query).ToList();
            var batchOperation = new TableBatchOperation();
            int itemsInBatch = 0;
            foreach (MailingList listRow in listRows)
            {
                batchOperation.Delete(listRow);
                itemsInBatch++;
                if (itemsInBatch == 100)
                {
                    mailingListTable.ExecuteBatch(batchOperation);
                    itemsInBatch = 0;
                    batchOperation = new TableBatchOperation();
                }
            }
            if (itemsInBatch > 0)
            {
                mailingListTable.ExecuteBatch(batchOperation);
            }
            return RedirectToAction("Index");
        }
    }
}