#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
                                                                                          /*la on recupere le username*/ 
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, string username, ICollector<UserFilesRecord> outRecords)
{
// au depart on devais recuperer le username depuis le contenus des donnees de la requette post 
// la c'est devenus un parametre de route https://......./{username}/folders/
// si la requette est un get on recupere la liste des dossier si c'est un post on cree le dossier 
// notre fonction est lier avec les requettes POST
// a voire les parametre sur "Integrer"

    log.Info("C# HTTP trigger function processed a request.");

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    
// avant username est un champ f data 
//string username = data?.username; 


    string foldername = data?.foldername;
    var responseMessage = String.Format("Directory: '{0}' created for user: {1}.", foldername, username);

    await CreateFolder(username, foldername, outRecords);

    return (username == null || foldername == null)
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, responseMessage);
}

public static async Task<string> CreateFolder(string user, string foldername, ICollector<UserFilesRecord> outRecords)
{
    string containerName = String.Format("{0}-files", user);
    string blobname = String.Format("{0}/.", foldername);

    var record = new UserFilesRecord(){
            PartitionKey = user,
            RowKey = Guid.NewGuid().ToString(), 
            Name = foldername,
            ContainerName = containerName,
            Uri = String.Format("{0}/{1}", containerName, foldername),
            IsFolder = true
        };
    outRecords.Add(record);

    return String.Format("{0}/{1}", containerName, foldername);
}

public class UserFilesRecord: TableEntity
{
    public string UserName { get; set; }
    public string ContainerName { get; set; }
    public bool IsFolder { get; set; }
    public string Name { get; set; }
    public string Uri { get; set; }
} 