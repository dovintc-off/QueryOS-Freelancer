// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://gnu.org
namespace QueryOS_Freelancer.Core.FileSystem;
using QueryOS_Freelancer.Core._Task;

using System.IO;

public class SystemFileManager : Task {    
    public SystemFileManager() : base("FileManager"){
        RealRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FSR");
    }

    public string RealRootPath { get; private set; } = "";
    public string VirtualCurrentPath { get; private set; } = "/";

    protected override void Execute(string[] args) {
        // TODO: Localize
        Terminal.WriteLine("[blue][ INFO ] Munting File System Root");

        bool already = BootStrapper.Boot();
        if (already) {
            // TODO: Localize
            Terminal.WriteLine("[ OK ] File System Root successfully mounted.");
        } else {
            DevConsole.Log(LogType.Error, Localization.Get("Crash.FileSystemNotLoaded"), true);
            Stop();
        }
    }

    public void ChangeDirectory(string to) {
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

    public string[] ListDirectory () {
        List<string> data = new List<string>(); 
        string currentDir = Path.Combine(RealRootPath, VirtualCurrentPath.TrimStart('/'));

        if (!Directory.Exists(currentDir)) return data.ToArray();

        string[] subDirectories = Directory.GetDirectories(currentDir);
        foreach (string dir in subDirectories) {
            if (dir.EndsWith(".fold-inf") || dir.EndsWith(".inf")) continue;

            var meta = GetMetadata(dir, true);
            if (meta.Stage <= GlobalStates.GlobalStates.User.stage) {
                data.Add($"{meta.Permissions}|D|{Path.GetFileName(dir)}");
            }
        }

        string[] files = Directory.GetFiles(currentDir);
        foreach (string file in files) {
            if (file.EndsWith(".inf") || file.EndsWith(".fold-inf")) continue;

            var meta = GetMetadata(file, false);
            
            if (meta.Stage <= GlobalStates.GlobalStates.User.stage && (meta.Owner == GlobalStates.GlobalStates.User.name || meta.Owner == "*" || meta.Owner == "root")) {
                data.Add($"{meta.Permissions}|F|{Path.GetFileName(file)}");
            }
        }

        return data.ToArray();
    }


    public void CreateFile(string name, string content){
        if (string.IsNullOrWhiteSpace(name)) return;

        string FilePath = Path.GetFullPath(Path.Combine(RealRootPath, VirtualCurrentPath.TrimStart('/').TrimStart('\\'), name));
        if (FilePath.StartsWith(RealRootPath)) {
            try {
                File.WriteAllText($"{FilePath}.inf", "S:1,O:*,P:755");
                File.WriteAllText(FilePath, content);
                DevConsole.Log(LogType.Info, "Created!");
            } catch {
                DevConsole.Log(LogType.Info, ":(1)");
            }
        } else {
            DevConsole.Log(LogType.Info, ":(2)");
        }
    }

    public void RemoveFile(string name){
        if (string.IsNullOrWhiteSpace(name)) return;
        string FilePath = Path.GetFullPath(Path.Combine(RealRootPath, VirtualCurrentPath.TrimStart('/').TrimStart('\\'), name));
        if (FilePath.StartsWith(RealRootPath)) {
            File.Delete(FilePath);
            File.Delete(FilePath+".inf");
        }
    } 

    public void RemoveDirectory(string name){
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

    private NodeMetadata GetMetadata(string path, bool isFolder) {
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