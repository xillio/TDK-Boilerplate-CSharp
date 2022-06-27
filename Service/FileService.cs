using TDK_Boilerplate_C_.Service;
using TDK_Boilerplate_C_;
using TDK_Boilerplate_C_.jsonrpc;
using System.Text.Json;
using System.IO;

namespace TDK_Boilerplate_C_.Service;

public class FileService : AbstractService {
    private string appConfigPath;
    public FileService(){
        appConfigPath = "/sample-connector";
    }
    
    public object? getProperty(object? obj, string name)
    {
        if (obj == null) return null;
        if (((JsonElement)obj).ValueKind != JsonValueKind.Object) return null;

        JsonElement tmp;
        if (!((JsonElement)obj).TryGetProperty(name, out tmp)) return null;

        return tmp;
    } 

    public string childXdip(string xdip, string child){
        return xdip.EndsWith('/') ? xdip + child : xdip + "/" + child;
    }

    // XDIP (assumed to be valid) -> Path.
    public string fromXdip(string xdip){
        var url = new Uri(xdip);

        if ('/' + url.Host != appConfigPath){ // if ('/' + url.hostname !== this.appConfig.path)
            var ex = new Exception(ErrorCode.INVALID_CONFIGURATION + " Invalid Configuration");
            throw ex;
        }
        return "./contents" + (url.LocalPath.EndsWith('/') ? url.LocalPath.Substring(0, url.LocalPath.Length-1) : url.LocalPath);
    }

    // Path (assumed to start with ./contents/) -> XDIP.
    public string toXdip(string xPath){
        return "xdip:/" + appConfigPath + xPath.Substring(10); // + this.appConfig.path + xPath.slice(10); 
    }

    public override bool validate(object config){
        return true;
    }
    
    public override bool authorize(object config){
        return true;
    }
    
    public override Output get(object config, object xdip){
        var xPath = fromXdip(xdip.ToString() ?? "");
         
        // Check if it exists.
        FileInfo stat;
        try {
            stat = new FileInfo(xPath);
        } catch (Exception err){
            var ex = new Exception(ErrorCode.NO_SUCH_ENTITY + " No Such Entity " + err.Message);
            throw ex;
        }
        var attr = stat.Attributes;

        return new Output {
            xdip = xdip,
            isFolder = attr.HasFlag(FileAttributes.Directory),
            created = stat.CreationTime, 
            modified = stat.LastWriteTime,
            systemName = "",
            size = 0
        };
    }

    public override List<Output> getChildren(object config, object xdip){
        var xPath = fromXdip(xdip.ToString());
         
        // Check if it exists.
        FileInfo stat;
        try {
            stat = new FileInfo(xPath);
        } catch (Exception err){
            throw new Exception(ErrorCode.NO_SUCH_ENTITY + " No Such Entity " + err.Message);
        }
        var attr = stat.Attributes;

        if (!attr.HasFlag(FileAttributes.Directory)){
            return new List<Output>();
        }

        var result = new List<Output>();

        // Get all files
        var children = Directory.GetFiles(xPath).Select(Path.GetFileName);        
        foreach (var child in children){
            var xdipChild = childXdip(xdip.ToString(), child);
            var pathChild = fromXdip(xdipChild);
            var statChild = new FileInfo(pathChild);

            result.Add( new Output {
                xdip = xdipChild,
                isFolder = false,
                created = statChild.CreationTime,
                modified = statChild.LastWriteTime,
                systemName = Path.GetFileName(child),
                size = (int)statChild.Length
            });
        }

        // Get all directories
        children = Directory.GetDirectories(xPath).Select(Path.GetFileName);        
        foreach (var child in children){
            var xdipChild = childXdip(xdip.ToString(), child);
            var pathChild = fromXdip(xdipChild);
            var statChild = new DirectoryInfo(pathChild);

            result.Add( new Output {
                xdip = xdipChild,
                isFolder = true,
                created = statChild.CreationTime,
                modified = statChild.LastWriteTime,
                systemName = Path.GetFileName(child)
            });
        }

        return result;
    }

    public override string getBinary(object config, object xdip){
        var xPath = fromXdip(xdip.ToString());
        
        // Check if it exists.
        FileInfo stat;
        try {
            stat = new FileInfo(xPath);
        } catch (Exception err){
            var ex = new Exception(ErrorCode.NO_SUCH_ENTITY + " No Such Entity " + err.Message);
            throw ex;
        }
        var attr = stat.Attributes;

        if (attr.HasFlag(FileAttributes.Directory)){
            var ex = new Exception(ErrorCode.NO_BINARY_CONTENT + " No Binary Content");
            throw ex;
        }

        // Read Contents.
        return System.IO.File.ReadAllText(xPath, encoding: System.Text.Encoding.UTF8);
    }

    public override Output create(object config, object entity, object binaryContents){
        object? original = getProperty(entity, "original");
        object? name = getProperty(original, "name");
        object? systemName = getProperty(name, "systemName");

        string xPath = "./contents/" + systemName;

        return new Output {
            xdip = toXdip(xPath),
            systemName = systemName.ToString() ?? "",
        };
    }
}