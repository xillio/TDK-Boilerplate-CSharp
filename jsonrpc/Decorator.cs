namespace TDK_Boilerplate_C_;

public class Decorators
{
    public container? container { get; set; }
    public contentType? contentType{ get; set; }
    public created? created { get; set; }
    public language? language { get; set; }
    public mimeType? mimeType { get; set; }
    public file? file { get; set; }
    public modified? modified { get; set; }
    public name? name { get; set; }
    public parent? parent { get; set; }
}

public class container {
    public bool hasChildren { get; set; }
}
public class contentType {
    public string systemName { get; set; }
}
public class created {
    public DateTime date { get; set; }
}
public class language {
    public string tag { get; set; }
}
public class mimeType {
    public string type { get; set; }
}
public class file {
    public string rawExtension { get; set; } 
    public int size { get; set; }
}
public class modified {
    public DateTime date { get; set; }
}
public class name {
    public string displayName { get; set; }
    public string systemName { get; set; }
}
public class parent {
    public string id { get; set; }
}