// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://gnu.org
using System.Text.Json.Serialization;
namespace QueryOS_Freelancer.Core.FileSystem;
public class NodeMetadata {
    public int Stage { get; set; } = 0;
    public string Owner { get; set; } = "root";
    public string Permissions { get; set; } = "755";
}
