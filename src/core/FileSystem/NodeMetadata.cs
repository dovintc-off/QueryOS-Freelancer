using System.Text.Json.Serialization;
namespace QueryOS_Freelancer.Core.FileSystem;
public class NodeMetadata {
    public int Stage { get; set; } = 0;
    public string Owner { get; set; } = "root";
    public string Permissions { get; set; } = "755";
}
