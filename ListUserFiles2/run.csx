using System.Net;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

public class UserFile : TableEntity
{
    public string UserName {get; set; }
    public string ContainerName {get;  set; }
    public string Name  { get; set; }
    public string Uri  {get; set; }
}
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string username, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

   var list  = await GetUserFile(username, log ) ;

    return req.CreateResponse(HttpStatusCode.OK, list);
}

public static CloudStorageAccount GetStorageAccount(){
    string connectionStringName = CloudConfigurationManager.GetSetting("ConnectionStringName");
    string connectionString = CloudConfigurationManager.GetSetting(connectionStringName);  
    
    var account = CloudStorageAccount.Parse(connectionString)  ;
    return account;
}


public static async Task<IEnumerable<UserFile>> GetUserFile(string username, TraceWriter log)
{
     CloudStorageAccount account = GetStorageAccount();
     CloudTableClient client = account.CreateCloudTableClient();
     CloudTable table = client.GetTableReference("UserFiles");

     TableQuery<UserFile> rangequery = new TableQuery<UserFile>().Where(TableQuery.GenerateFilterCondition
     ("PartitionKey", QueryComparisons.Equal, username));

     var list = new List<UserFile>(); 

    foreach (UserFile file in table.ExecuteQuery(rangequery)){
        log.Info(String.Format("{0}", file.Name));
        list.Add(file);

    }
    return list;
}


