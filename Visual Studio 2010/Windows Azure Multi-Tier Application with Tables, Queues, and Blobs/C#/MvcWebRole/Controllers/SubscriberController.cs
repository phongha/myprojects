using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MvcWebRole.Models;

namespace AzureEmailServiceControllers
{
    public class SubscriberController : Controller
    {
        private CloudTable mailingListTable;

        public SubscriberController()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            // If this is running in a Windows Azure Web Site (not a Cloud Service) use the Web.config file:
            //    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);

            // Get table object for working with mailinglist table.
            var tableClient = storageAccount.CreateCloudTableClient();
            mailingListTable = tableClient.GetTableReference("mailinglist");
        }

        private Subscriber FindRow(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Subscriber>(partitionKey, rowKey);
            var retrievedResult = mailingListTable.Execute(retrieveOperation);
            var subscriber = retrievedResult.Result as Subscriber;
            if (subscriber == null)
            {
                throw new Exception("No subscriber found for: " + partitionKey + ", " + rowKey);
            }
            return subscriber;
        }

        private List<MailingList> GetListNames()
        {
            var query = (new TableQuery<MailingList>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "mailinglist")));
            var lists = mailingListTable.ExecuteQuery(query).ToList();
            return lists;
        }

        // GET: /Subscriber/

        public ActionResult Index()
        {
            var query = (new TableQuery<Subscriber>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "mailinglist")));
            var subscribers = mailingListTable.ExecuteQuery(query).ToList();
            return View(subscribers);
        }

        // GET: /Subscriber/Details/5

        public ActionResult Details(string partitionKey, string rowKey)
        {
            var subscriber = FindRow(partitionKey, rowKey);
            return View(subscriber);
        }

        // GET: /Subscriber/Create

        public ActionResult Create()
        {
            var lists = GetListNames();
            ViewBag.ListName = new SelectList(lists, "ListName", "Description");
            var model = new Subscriber() { Verified = false };
            return View(model);
        }

        // POST: /Subscriber/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Subscriber subscriber)
        {
            if (ModelState.IsValid)
            {
                subscriber.SubscriberGUID = Guid.NewGuid().ToString();
                if (subscriber.Verified.HasValue == false)
                {
                    subscriber.Verified = false;
                }

                var insertOperation = TableOperation.Insert(subscriber);
                mailingListTable.Execute(insertOperation);
                return RedirectToAction("Index");
            }

            var lists = GetListNames();
            ViewBag.ListName = new SelectList(lists, "ListName", "Description", subscriber.ListName);

            return View(subscriber);
        }

        // GET: /Subscriber/Edit/5

        public ActionResult Edit(string partitionKey, string rowKey)
        {
            var subscriber = FindRow(partitionKey, rowKey);

            var lists = GetListNames();
            ViewBag.ListName = new SelectList(lists, "ListName", "Description", subscriber.ListName);

            return View(subscriber);
        }

        // POST: /Subscriber/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string partitionKey, string rowKey, string listName, string emailAddress, Subscriber editedSubscriber)
        {
            // Since MailingList and UpdateModel are in the view as EditorFor fields,
            // and since they refer to the same properties as PartitionKey and RowKey,
            // exclude PartitionKey and RowKey from model binding when calling UpdateModel.
            var excludeProperties = new string[] { "PartitionKey", "RowKey" };

            if (ModelState.IsValid)
            {
                try
                {
                    UpdateModel(editedSubscriber, string.Empty, null, excludeProperties);
                    if (editedSubscriber.PartitionKey == partitionKey && editedSubscriber.RowKey == rowKey)
                    {
                        //Keys didn't change -- Update the row
                        var replaceOperation = TableOperation.Replace(editedSubscriber);
                        mailingListTable.Execute(replaceOperation);
                    }
                    else
                    {
                        // Keys changed, delete the old record and insert the new one.
                        if (editedSubscriber.PartitionKey != partitionKey)
                        {
                            // PartitionKey changed, can't do delete/insert in a batch.
                            var deleteOperation = TableOperation.Delete(new Subscriber { PartitionKey = partitionKey, RowKey = rowKey, ETag = editedSubscriber.ETag });
                            mailingListTable.Execute(deleteOperation);
                            var insertOperation = TableOperation.Insert(editedSubscriber);
                            mailingListTable.Execute(insertOperation);
                        }
                        else
                        {
                            // RowKey changed, do delete/insert in a batch.
                            var batchOperation = new TableBatchOperation();
                            batchOperation.Delete(new Subscriber { PartitionKey = partitionKey, RowKey = rowKey, ETag = editedSubscriber.ETag });
                            batchOperation.Insert(editedSubscriber);
                            mailingListTable.ExecuteBatch(batchOperation);
                        }
                    }
                    return RedirectToAction("Index");
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == 412)
                    {
                        // Concurrency error.
                        // Only catching concurrency errors for non-key fields.  If someone
                        // changes a key field we'll get a 404 and we have no way to know
                        // what they changed it to.
                        var currentSubscriber = FindRow(partitionKey, rowKey);
                        if (currentSubscriber.Verified != editedSubscriber.Verified)
                        {
                            ModelState.AddModelError("Verified", "Current value: " + currentSubscriber.Verified);
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you got the original value. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to edit this record, click "
                            + "the Save button again. Otherwise click the Back to List hyperlink.");
                        ModelState.SetModelValue("ETag", new ValueProviderResult(currentSubscriber.ETag, currentSubscriber.ETag, null));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var lists = GetListNames();
            ViewBag.ListName = new SelectList(lists, "ListName", "Description", listName);
            return View(editedSubscriber);
        }

        private void UpdateSubscriber(string partitionKey, string rowKey, Subscriber editedSubscriber)
        {
            try
            {
                var replaceOperation = TableOperation.Replace(editedSubscriber);
                mailingListTable.Execute(replaceOperation);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 412)
                {
                    // Concurrency error.
                    // Only catching concurrency errors for non-key fields.  If someone
                    // changes a key field we'll get a 404 and we have no way to know
                    // what they changed it to.
                    var currentSubscriber = FindRow(partitionKey, rowKey);
                    if (currentSubscriber.Verified != editedSubscriber.Verified)
                    {
                        ModelState.AddModelError("Verified", "Current value: " + currentSubscriber.Verified);
                    }
                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                        + "was modified by another user after you got the original value. The "
                        + "edit operation was canceled and the current values in the database "
                        + "have been displayed. If you still want to edit this record, click "
                        + "the Save button again. Otherwise click the Back to List hyperlink.");
                    ModelState.SetModelValue("ETag", new ValueProviderResult(currentSubscriber.ETag, currentSubscriber.ETag, null));
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: /Subscriber/Delete/5

        public ActionResult Delete(string partitionKey, string rowKey)
        {
            var subscriber = FindRow(partitionKey, rowKey);
            return View(subscriber);
        }

        // POST: /Subscriber/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string partitionKey, string rowKey)
        {
            var deleteOperation = TableOperation.Delete(new Subscriber { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" });
            mailingListTable.Execute(deleteOperation);
            return RedirectToAction("Index");
        }
    }
}