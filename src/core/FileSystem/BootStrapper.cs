namespace QueryOS_Freelancer.Core.FileSystem;

using System.IO;

public static class BootStrapper {
    public static bool Boot() {
        string exePath = AppDomain.CurrentDomain.BaseDirectory;
        string FSR = Path.Combine(exePath, "FSR");
        try {
            if (!Directory.Exists(FSR)) {
                Directory.CreateDirectory(FSR);
            }  
            return true;
        } catch {
            return false;
        }

    }
}