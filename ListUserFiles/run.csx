#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string username, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    var list = await GetUserFiles(username, log);
    
    return req.CreateResponse(HttpStatusCode.OK, list);
}

public static async Task<IEnumerable<UserFilesRecord>> GetUserFiles(string user, TraceWriter log)
{
    // Retrieve the storage account from the connection string.
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
        CloudConfigurationManager.GetSetting("cloudstoraa47_STORAGE"));

    // Create the table client.
    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

    // Create the CloudTable object that represents the "people" table.
    CloudTable table = tableClient.GetTableReference("UserFiles");

    // Construct the query operation for all customer entities where PartitionKey="Smith".
    TableQuery<UserFilesRecord> query = new TableQuery<UserFilesRecord>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, user));

    var list = new List<UserFilesRecord>();
    // Print the fields for each customer.
    foreach (UserFilesRecord entity in table.ExecuteQuery(query))
    {     
        list.Add(entity);
    }   

    return list;
}

public static async Task<IEnumerable<IListBlobItem>> GetBlobsForUser(string user, TraceWriter log)
{
    var containerName = String.Format("{0}-files", user);

    // Retrieve storage account from connection string.
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
        CloudConfigurationManager.GetSetting("cloudstoraa47_STORAGE"));

    // Create the blob client.
    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

    // Retrieve reference to a previously created container.
    CloudBlobContainer container = blobClient.GetContainerReference(containerName);
    var list = new List<IListBlobItem>();
    // Loop over items within the container and output the length and URI.
    foreach (IListBlobItem item in container.ListBlobs(null, false))
    {
        if (item.GetType() == typeof(CloudBlockBlob))
        {
            CloudBlockBlob blob = (CloudBlockBlob)item;

            log.Info(String.Format("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri));

            list.Add(blob);
        }
        else if (item.GetType() == typeof(CloudPageBlob))
        {
            CloudPageBlob pageBlob = (CloudPageBlob)item;

            log.Info(String.Format("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri));

        }
        else if (item.GetType() == typeof(CloudBlobDirectory))
        {
            CloudBlobDirectory directory = (CloudBlobDirectory)item;

            log.Info(String.Format("Directory: {0}", directory.Uri));
        }
    }    

    return list;
}

public class UserFilesRecord: TableEntity
{
    public string UserName { get; set; }
    public string ContainerName { get; set; }
    public bool IsFolder { get; set; }
    public string Name { get; set; }
    public string Uri { get; set; }
}