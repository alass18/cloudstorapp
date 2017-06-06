#r "Microsoft.WindowsAzure.Storage"

using System.Text;
using System.Net;
using System.Net.Http;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob; 
using Microsoft.WindowsAzure.Storage.Table;
using HttpMultipartParser;

// test de modification ;) 
// cette partie c'est une logique pour decoupler le code du nom de la chaine de connexion 
// comme ca on peut a n'importe changer de connection vers le stockage sans changer le code 
// il suffit de changer le parametre ConnectionStringName dans la section de configuration Application Settings

static string ConnectionStringNameParameter = CloudConfigurationManager.GetSetting("ConnectionStringName");
// la premiere ligne c'est pour recuperer le nom du parametre a recuperer depuis les Application Settings
// dans notre cas la valeur de ce parametre "cloudstoraa47_STORAGE"
static string ConnectionString = CloudConfigurationManager.GetSetting(ConnectionStringNameParameter);
// et la on lui demande de recuperer la valeur du parametre avec le nom stocker 
// dans la variable ConnectionStringNameParameter 
    // autrement dis on utilise le valeur du premier parametre
    // comme nom du parametre sur lequel pointer 
    // pour nous c'est la chaine de connection a utiliser 
    // ca nous epargne l'utilisation d'un nom fixe comme suivant :
    //  
    //    CloudStorageAccount account = CloudStorageAccount
    //             .Parse(CloudConfigurationManager.GetSetting("cloudstoraa47_STORAGE"));
    // 
    // si ta remarquer le nom de la chaine de connexion est code en brut,
    // dans notre cas le programme va utiliser la connection cloudstoraa47_STORAGE car c'est 
    // le nom de chaine de connection specifier sur ConnectionStringName;


public static async Task<HttpResponseMessage> 
            Run(HttpRequestMessage req, TraceWriter log, 
                ICollector<UserFilesRecord> outRecords, string username)
{
    //CloudConfigurationManager.SetSetting("FUNCTION_APP_EDIT_MODE", "readonly");
    log.Info("C# HTTP trigger function processed a request.");
    // cette partie du code c'est pour remedier a un bug qui ce declenche 
    // lors de la lecture du contenu de la requette, precisant que le contenu
    // ne contient pas de fin , cad : il faut ajouter /r/n 
    // on recupere le flux de donnee 
    //MemoryStream tempStream = new MemoryStream();
    //reqStream.CopyTo(tempStream); // on copie le flux vers un nouveau espace en memoire 
    //tempStream.Seek(0,SeekOrigin.End); // on pointe sur la derniere position du contenu 
    //StreamWriter writer = new StreamWriter(tempStream); // on passe le stream temporaire a un objet qui va ecrire des byte dessus
    //writer.WriteLine(); // on ajoute un retour de ligne "/r/n" au contenu du flux 
    //writer.Flush(); 
    //tempStream.Position = 0; // on se pointe sur la position 0 pour recommencer la copie 
    /*StreamContent streamContent = new StreamContent(tempStream);
    foreach(var header in req.Content.Headers)
    {
        streamContent.Headers.Add(header.Key, header.Value);
    }*/   // je laisse le commentaire 
    Stream reqStream = await req.Content.ReadAsStreamAsync(); 
    var parser = new MultipartFormDataParser(reqStream, Encoding.UTF8);
    // le MultipartFormDataParser est un objet qui fais partie de la librairies HttpMultipartParser
    // elle sert a decomposer le contenu d'une requette http POST avec un contenu multipart/form-data
    // est c'est ce qu'on utilise pour l'upload des fichiers
    //
    string containerName = String.Format("{0}-files", username); // ont prepare le nom du container sur le storage
    string filename = "";
    var parsedFiles = parser.Files; 
    foreach(var current in parsedFiles)
    {
        //log.Info(current.Name);
        filename = current.FileName;
        
        var lastLength = current.Name.Split('/').Last().Length;
        var uriLength = current.Name.Length;
        var lengthToExtract = uriLength - (lastLength + 1);
        var parentFolder = "";

        if(current.Name != "/")
        {
            parentFolder = (current.Name.StartsWith("/"))  
                ? current.Name.Substring(1, (lengthToExtract - 1))
                : current.Name.Substring(0, lengthToExtract); 

            log.Info(String.Format("ParentFolder : {0}", parentFolder));

            filename = String.Format("{0}/{1}", parentFolder, filename); 

            log.Info(String.Format("FileName : {0}", filename));                
        } 
 
        var exists = BlobExists(containerName, filename, log);
            // on verifie si le fichier existe deja sue le container 
            // est ce avant de l'uploader, ainsi on peux avoir une condition pour ajouter
            // l'enregistrement sur la table ou pas, ou meme garder une trace comme quoi 
            // le fichiers a etait remplace.

        var fileuri = await UploadToBlob(containerName, filename, current.Data, log);// on appel la fonction pour gerer l'upload est on recupere 
                                                                       // le lien pour acceder au fichier, ainsi on l'ajoute a l'enregistrement
        var record = new UserFilesRecord(){
                PartitionKey = username,
                RowKey = Guid.NewGuid().ToString(), 
                ContainerName = containerName,
                ParentFolder = parentFolder  ,              
                IsFolder = false,
                Uri = fileuri,
                Name = filename                
            };
        if(!exists)
        {
            outRecords.Add(record);
        }
    }

    return req.CreateResponse(HttpStatusCode.OK, "file recieved"); 
}

public static async Task<string> UploadToBlob(string containerName, string fileName, Stream outputBlob, TraceWriter log)
{
    outputBlob.Position = 0;
    //// remarque : cette partie du code delimiter par "#region code repetitif" on dois la transformer en methode
       
    // #region Code repetitif
    // CloudStorageAccount account = CloudStorageAccount
    //             .Parse(CloudConfigurationManager.GetSetting("cloudstoraa47_STORAGE"));
    // CloudStorageAccount account = CloudStorageAccount.Parse(ConnectionString);
    // - instanciation d'un objet CloudStorage account en utilisant une chaine de connexion 
    // CloudBlobClient client = account.CreateCloudBlobClient();
    // - instanciation d'un nouveau client CloudBlobClient pour se connecter au service BlobStorage
    // CloudBlobContainer container = client.GetContainerReference(containerName);
    // #endregion

    // - instanciation d'un objet CloudBlobContainer qui va nous permettre de manipuler 
    // - les fichier dans le conteneur sur le cloud 
    // container.SetPermissions(
    //        new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off });
    // container.CreateIfNotExists();

    var container = GetContainerReference(containerName, true); 

    CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
    await blockBlob.UploadFromStreamAsync(outputBlob);

    return blockBlob.Uri.ToString();
}

public static bool BlobExists(string containerName, string key, TraceWriter log)
{
    // #region Code repetitif
    // CloudStorageAccount account = CloudStorageAccount
    //            .Parse(CloudConfigurationManager.GetSetting("cloudstoraa47_STORAGE"));
    // CloudBlobClient client = account.CreateCloudBlobClient();
    // CloudBlobContainer container = client.GetContainerReference(containerName);
    // #endregion
    // au lieu de ca on appel la methode GetContainerReference

    var container = GetContainerReference(containerName, false);

    if(!container.Exists()) return false;
    log.Info(containerName);
    log.Info(key);
    var exists = container
                  .GetBlockBlobReference(key)
                  .Exists();  
    log.Info(Convert.ToString(exists));
 
    return exists;
}

public static CloudBlobClient GetBlobClient()
{
    CloudStorageAccount account = CloudStorageAccount.Parse(ConnectionString);
    CloudBlobClient client = account.CreateCloudBlobClient();

    return client;
}

public static CloudBlobContainer GetContainerReference(string containerName, bool createIfNotExists)
{
    var client = GetBlobClient();
    var container = client.GetContainerReference(containerName);

    if(createIfNotExists)
        container.CreateIfNotExists();

    return container;
}

public class UserFilesRecord: TableEntity
{
    public string UserName { get; set; }
    public string ContainerName { get; set; }
    public bool IsFolder { get; set; }
    public string ParentFolder { get; set; }
    public string Name { get; set; }
    public string Uri { get; set; }
}