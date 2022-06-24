namespace TDK_Boilerplate_C_.Service;

public abstract class AbstractService {

    public abstract bool validate(object config);
    
    public abstract bool authorize(object config);
    
    public abstract Output get(object config, object xdip);

    public abstract List<Output> getChildren(object config, object xdip);

    public abstract string getBinary(object config, object xdip);

    public abstract Output create(object config, object entity, object binaryContents);
}