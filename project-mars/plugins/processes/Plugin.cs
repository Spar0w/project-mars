using MarsClient.Plugins;
using System;
using System.Diagnostics;

class Processes : Plugin {
    public override string Name => "Process Handling";
    public override string Author => "Samuel Barrows";
    public override string Version => "2";
    
    [CommandAttributes("StartProc", arguments = 1, description = "Starts a Process")]
    public IDictionary<string, dynamic> StartProc(string processPath){
        //start a process
        Process proc = new Process();
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.FileName = processPath;
        proc.StartInfo.CreateNoWindow = false;

        try{
            proc.Start();
            return Exit(0, $"{processPath} started successfully!");
        } catch (Exception e) {
            Console.WriteLine(e);
            return Exit(1, $"Failed to start {processPath}. Exception: {e}");
        }
    }

    [CommandAttributes("StopProc", arguments = 1, description = "Stops a Process")]
    public IDictionary<string, dynamic> StopProc(string p){
        //try to get a process
        //attempt to get the process using the ID.
        //the user may have put in the name of the process, so we try that next
        Process proc = null;
        try{
            int pId = Int32.Parse(p);
            proc = Process.GetProcessById(pId);
        } catch (FormatException){
            Process[] procs = Process.GetProcessesByName(p);
            //just get the first one who cares
            proc = procs[0];
        }

        //kill the process and respond to the server
        try{
            proc.Kill();
            return Exit(0, $"{p} stopped successfully!");
        } catch (Exception e) {
            Console.WriteLine(e);
            return Exit(1, $"Failed to stop {p}. Exception: {e}");
        }
    }

    [CommandAttributes("GetProc", arguments = 1, description = "Gets a Processes Information")]
    public IDictionary<string, dynamic> GetProc(string processName){
        //get a specific process

        //if no process is provided, get all of them
        if (processName == ""){
            return GetAllProcs();
        }

        Process[] proc = null;
        try{
            proc = Process.GetProcessesByName(processName);
        } catch (Exception e) {
            return Exit(1, $"Failed to get {processName}. Exception: {e}");
        }
        if (proc.Length == 0){
            return Exit(1, $"Failed to get {processName}. It might not be running or the name is wrong.");
        } else {
            //a big string if there are multiple processes with the same name
            string processList = "";
            foreach (var p in proc){
                processList += $"{p.Id},{processName}\n";
            }
            return Exit(0, processList);
        }
    }

    [CommandAttributes("GetAllProcs", arguments = 0, description = "Gets all processes information")]
    public IDictionary<string, dynamic> GetAllProcs(){
        //get all processes and return a csv list of ID and Name
        Process[] procs = Process.GetProcesses();

        //a really big string
        string processList = "";

        foreach(var p in procs){
            processList += $"{p.Id},{p.ProcessName}\n";
        }
        return Exit(0, processList);
    }

    private static IDictionary<string, dynamic> Exit(int status, string message){
        IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();

        //string processName = processPath.Split(@"\").Last();
    
        Processes p = new Processes();
        //List<string> message = null;
        if (status == 0){
            Console.WriteLine(message);
            ReturnDict.Add("Status", status);
            ReturnDict.Add("Message", message);
        } else {
            ReturnDict.Add("Message", message);
        }
        return ReturnDict;
    }
}