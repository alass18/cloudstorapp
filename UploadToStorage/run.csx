#r "Microsoft.WindowsAzure.Storage"

using System.Text; // pour utiliser Encoding 
using System.IO;
using System.Net;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using HttpMultipartParser;  

public class UserFiles: TableEntity
{
    //public string PartitionKey { get; set; } // hadou il sont deja herite on peu les enlever 
    //public new string RowKey { get; set; } // hadou ou bien andirou lihoum new , ou bien carrement n7ydouhoum
    public string ContainerName { get; set; } 
    public string Name { get ; set ; } 
    public string Uri { get; set; }  
}

public static TraceWriter _log;
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    _log = log;
    log.Info("C# HTTP trigger function processed a request.");
    
    string name = req.GetQueryNameValuePairs() // ici il recupere tout les parametre de la query string en pair cle valeur 
                                                // et il cherche le premier parametre avec comme cle la avec la Key = name et il retourne la valeur 
                                                 // dans notre cas hassan 
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;
    log.Info(name);
    Stream donneesRequette = await req.Content.ReadAsStreamAsync();//.Result;
    var content = req.Content;
    
    MultipartFormDataParser parser = new MultipartFormDataParser(donneesRequette, Encoding.UTF8);
    //log.Info(content.ToString()); 
    // c'est pour faire de la journalisation des evenemnt 
    // c'est comme si ta fais printfn et a toi de choisir la sortie, dans notre cas c'est les fichiers journeaux 
    //getContentTable(name);
    List<string> filesLinks = new List<string>();
    
    var files = parser.Files;
    Dictionary<string, bool> result = new Dictionary<string, bool>();
    if(files.Count() > 0 )
        result = UploadFiles(files, name, log);
     // on peux envoyer des parametres au serveur sous forme de chaine de requtte  query string 
     // et on peu aussi la recuperer depuis la route : 
    // parse query parameter


    return name == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, result);
}

public static CloudStorageAccount GetStorageAccount(){
    string connectionStringName = CloudConfigurationManager.GetSetting("ConnectionStringName");
    string connectionString = CloudConfigurationManager.GetSetting(connectionStringName);  
    
    var account = CloudStorageAccount.Parse(connectionString)  ;
    return account;
}

public static CloudTable GetTable(string tableName)
{
    CloudStorageAccount account = GetStorageAccount(); // comment il fais pour creer le compte ?
    // il fait appelle l la methode GetStorageAccount pour le créer 
    
    //récuperer le compte de stockage 
    CloudTableClient client = account.CreateCloudTableClient();
    ApplyRetryPolicy(ref client); // methode pour appliquer une strategie de retentative , RetryPolicy et cette fois
                                // on lui envois comme parametre une reference vers le clients qu'on utilise 
    //créer un client du service table pour être capable de lire enregistrer des données dans le stockage de table 
    CloudTable table  = client.GetTableReference(tableName);
    // récuperer une réference de la table client 
    table.CreateIfNotExists();
    return table;
}

public static Dictionary<string, bool> UploadFiles(List<FilePart> files, string userName, TraceWriter log)
{ 
    
    //string MyConnectionString = CloudConfigurationManager.GetSetting("cloudstoraa47_STORAGE");
    //string connectionStringName = CloudConfigurationManager.GetSetting("ConnectionStringName");
    // log.Info(String.Format("la valeur de la chaine de connexion : {0} \r\n est = {1}", connectionStringName, MyConnectionString)); 
                                   // c'est juste pour garder une trace de ce qui se passe dans le programme 
                                  // comme ca si il y'a une erreur en cherche le log file et on cherche ou ca c'est bloque 
    //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(MyConnectionString);
    var containerName = $"{userName}-files";
    var blobName = "";
    // had le code $"{nomVariable}-files" , c'est comme = nomVariable + "-files" 
    CloudStorageAccount account = GetStorageAccount();

    CloudBlobClient client = account.CreateCloudBlobClient();
    CloudBlobContainer container = client.GetContainerReference(containerName);// les noms de container doivents etre en min 
    container.CreateIfNotExists();
    //CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
    
    Dictionary<string, bool> resultOfUploads = new Dictionary<string, bool>();
    
    foreach(var f in files){ 
        var blob = container.GetBlockBlobReference(f.FileName);
        blob.UploadFromStream(f.Data); 
        AddFile(userName, f.FileName, blob.Uri.ToString()); // on a pas mis d'appel a la methode ;) ya un dernier souci , on test pour voir
        // meme si ya ni error ni warning 
        log.Info(String.Format("{0} file uploaded successfuly, generated Uri = {1}", f.FileName, blob.Uri.ToString()));        
        resultOfUploads.Add(blob.Uri.ToString(), (!String.IsNullOrEmpty(blob.Uri.ToString()))); 
        // derna la cle hya lien vers le fichier w la valeur c'est le status hna andirou comme quoi koulchi dayez mzyan 
    }    

// mlli l9it le constructeur necessaire l9itou tay7taj l objet Uri , wana rdit l uri string ligne 64
// donc mchit cheft la classe uri wach fiha chi methode lli tatparser un string w t3tini uri , mal9ithach mais l9it hadik trycreate , 
//w fles parametres dyalha kayn l'enumeration urikind , donc mchit cheft les valeurs dyalha 
    // for(var r = 0; r < resultOfUploads.Count(); r++)
    // {
    //     var key = resultOfUploads.ElementAt(r).Key;
    //     Uri absoluteUri;
    //     Uri.TryCreate(key, UriKind.Absolute, out absoluteUri);
    //     var blob = new CloudBlockBlob(absoluteUri);
    //     resultOfUploads.ElementAt(r).Value = blob.Exists(); // hna ma3ejbouch l7al 7it brit nremplacer une valeur w houwa tay iterer         
    //  tant que on peu avoir le uri donc le resultat est success
    return resultOfUploads;
}

// on va creer une claasse qui va representer les enregistrements f la base de donnes 
public static void AddFile(string username, string filename, string uri) 
{    CloudTable table = GetTable("UserFiles");
    UserFiles userFile = 
        new UserFiles(){
            PartitionKey = username,// kifach jebna username ?
            RowKey = Guid.NewGuid().ToString(),
            ContainerName = $"{username}-files",
            Name = filename,
            Uri = uri
        };
    
    TableOperation insert = TableOperation.Insert(userFile);
    table.Execute(insert);

} // 7na gelna lih UserFiles machi UsersFiles // w derna upload z3ma , lach ma dara walou ? 
// au fait ansemiwha b3da AddFile , pck c'est les donnees concernants les fichiers qu'on collecte 

public static void ApplyRetryPolicy(ref CloudTableClient client)
{
    TableRequestOptions options = new TableRequestOptions()
        {
            RetryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(500), 5),
            LocationMode = LocationMode.PrimaryThenSecondary,
            MaximumExecutionTime = TimeSpan.FromSeconds(2)
        };


    client.DefaultRequestOptions = options;
}
public static  void  getContentTable(string userName){
    CloudTable table = GetTable("UserFiles"); 
    TableQuery<UserFiles> rangeQuery = new TableQuery<UserFiles>().Where(
        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userName)
    );
    foreach (UserFiles file in table.ExecuteQuery(rangeQuery)){
        _log.Info(String.Format("{0}", file.Name));
    }

}