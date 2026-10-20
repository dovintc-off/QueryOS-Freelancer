// Copyright (C) 2026 Dovintc
// This project is licensed under the GNU General Public License, version 3.0 only.
// The full license text is available at: https://gnu.org
namespace QueryOS_Freelancer.Core._Task;
using QueryOS_Freelancer.Core._Terminal;

public abstract class Task {
    public string Name {get; protected set;}
    public Terminal Terminal {get; private set;} 
    public int PID {get; private set;} = -1;
    public bool IsRunning {get; protected set;}
    
    protected abstract void Execute(string[] args);
    public virtual void HandleInput(string input){}

    protected Task(string Name){
        this.Name = Name;
        this.PID = TaskManager.GetPID();
        this.Terminal = new Terminal();

        TaskManager.RegistryTask(this);
    }  

    public virtual void Run(string[] args){
        this.IsRunning = true;
        this.Execute(args);
    }

    public virtual void Stop(){
        IsRunning = false;
    }

    public void SetTerminalWith(Task parentTask){
        if (parentTask != null && parentTask.Terminal != null) {
            this.Terminal = parentTask.Terminal;
        }
    }
}

public static class TaskManager {
    private static List<Task> Tasks = new List<Task>();
    public static Task? ActiveTask { get; set; }

    public static Task? GetTasBykPID(int PID){
        if (!int.IsPositive(PID)) {
            return null;
        }
        foreach (Task task in Tasks) {
            if (task.PID == PID) {
                return task;
            }
        }
        return null;
    } 

    public static Task? GetTaskByName(string name){
        if (string.IsNullOrWhiteSpace(name)) {
            return null;
        }
        foreach (Task task in Tasks) {
            if (task.Name == name) {
                return task;
            }
        }
        return null;
    } 

    public static void StopTask(Task task){
        if (task == null) return;
        task.Stop();
    }

    public static int GetPID(){
        int MaxPID = 0;
        foreach (Task task in Tasks) {
            if (task.PID > MaxPID) {
                MaxPID = task.PID;
            } else {
                continue;
            }
        }
        return MaxPID + 1;
    }

    public static void RegistryTask(Task task){
        if (task != null) {
            Tasks.Add(task);
        }
    }
}