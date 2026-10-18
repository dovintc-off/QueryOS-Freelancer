namespace QueryOS_Freelancer.Core.FileSystem;

using QueryOS_Freelancer._Console;
using QueryOS_Freelancer.Loc;
using System.IO;

public static class SystemFileManager {
    public static void Initialize(){
        bool already = BootStrapper.Boot();
        if (already) {
            RealRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FSR");
        } else {
            Console.Log(LogType.Error, Localization.Get("Crash.FileSystemNotLoaded"), true);
        }
        
    }

    public static string RealRootPath { get; private set; } = "";
    public static string VirtualCurrentPath { get; private set; } = "/";

    public static void ChangeDirectory(string to) {
        if (string.IsNullOrWhiteSpace(to)) return;

        string combined;

        if (to.StartsWith("/")) {
            combined = Path.Combine(RealRootPath, to.TrimStart('/'));
        } else {
            string currentSubDir = VirtualCurrentPath.TrimStart('/');
            combined = Path.Combine(RealRootPath, currentSubDir, to);
        }

        string newRealPath = Path.GetFullPath(combined);

        if (!newRealPath.StartsWith(RealRootPath, StringComparison.OrdinalIgnoreCase)) {
            // TODO: Ошибка в терминал: Доступ запрещен (выход за пределы ОС)
            return;
        }

        if (Directory.Exists(newRealPath)) {
            var meta = GetMetadata(newRealPath, true);
            // TODO: Сделать здесь проверку прав и т.д.
            string relative = newRealPath.Substring(RealRootPath.Length).Replace('\\', '/');
            VirtualCurrentPath = string.IsNullOrEmpty(relative) ? "/" : relative;
        } else {
            // TODO: Ошибка в терминал: Папка не найдена
        }
    }

    public static string[] ListDirectory () {
        List<string> data = new List<string>(); 

        string[] subDirectories = Directory.GetDirectories(Path.Combine(RealRootPath, VirtualCurrentPath.TrimStart('/')));
        foreach (string dir in subDirectories) {
            var meta = GetMetadata(dir, true);
            if (meta.Stage <= GlobalStates.GlobalStates.User.stage) {
                data.Add($"{meta.Permissions}|D|{Path.GetFileName(dir)}");
            }
        }

        string[] files = Directory.GetFiles(Path.Combine(RealRootPath, VirtualCurrentPath.TrimStart('/')));
        foreach (string file in files) {
            var meta = GetMetadata(file, false);
            if (meta.Stage <= GlobalStates.GlobalStates.User.stage && meta.Owner == GlobalStates.GlobalStates.User.name) {
                data.Add($"{meta.Permissions}|F|{Path.GetFileName(file)}");
            }
        }

        return data.ToArray();
    }

    public static void CreateFile(string name, string content){
        if (string.IsNullOrWhiteSpace(name)) return;

        string FilePath = Path.GetFullPath(Path.Combine(RealRootPath, VirtualCurrentPath.TrimStart('/').TrimStart('\\'), name));
        if (FilePath.StartsWith(RealRootPath)) {
            try {
                File.WriteAllText($"{FilePath}.inf", "S:1,O:*,P:755");
                File.WriteAllText(FilePath, content);
            } catch {
                // TODO: лог ошибки в терминал и в консоль (на всякий)
            }
        } else {
            // TODO: ошибка в терминал: выход за пределы системы
        }
    }

    public static void RemoveFile(string name){
        if (string.IsNullOrWhiteSpace(name)) return;
        string FilePath = Path.GetFullPath(Path.Combine(RealRootPath, VirtualCurrentPath.TrimStart('/').TrimStart('\\'), name));
        if (FilePath.StartsWith(RealRootPath)) {
            File.Delete(FilePath);
            File.Delete(FilePath+".inf");
        }
    } 

    public static void RemoveDirectory(string name){
        if (string.IsNullOrWhiteSpace(name)) return;
        string DirectoryPath = Path.GetFullPath(Path.Combine(RealRootPath, VirtualCurrentPath.TrimStart('/').TrimStart('\\'), name));
        if (DirectoryPath.StartsWith(RealRootPath)) {
            Directory.Delete(DirectoryPath, true);
            Directory.Delete(DirectoryPath+".fold-inf");
        }
    }

    // TODO: Создание папки
    // TODO: Изменение разрешения у папок и файлов
    // TODO: Чтение файлов 

    private static NodeMetadata GetMetadata(string path, bool isFolder) {
        var meta = new NodeMetadata();
        if (string.IsNullOrWhiteSpace(path)) {return meta;}

        string infPath = isFolder ? path.TrimEnd('\\').TrimEnd('/') + ".fold-inf" : path + ".inf";
        if (File.Exists(infPath)) {
            try {
                string content = File.ReadAllText(infPath).Trim();
                string[] parts = content.Split(',');
                foreach (string part in parts) {
                    string[] keyValue = part.Split(':');
                    if (keyValue.Length != 2) continue;

                    string key = keyValue[0].Trim().ToUpper();
                    string value = keyValue[1].Trim();

                    if (key == "S" && int.TryParse(value, out int stage)) {
                        meta.Stage = stage;
                    } else if (key == "O") {
                        meta.Owner = value;
                    } else if (key == "P") {
                        meta.Permissions = value;
                    }
                }
            } catch {
                // TODO: Localize: Console.Log(LogType.Error, $"Failed to parse metadata file: {infPath}");
            }
        } else {
            try {File.WriteAllText(infPath, "S:0,O:root,P:755");}
            catch {}
        }
        return meta;
    }
}