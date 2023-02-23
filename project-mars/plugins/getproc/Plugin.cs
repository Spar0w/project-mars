using MarsClient.Plugins;
using System;
using System.Diagnostics;

class Processes : Plugin {
    public override string Name => "Get Process";
    public override string Author => "Samuel Barrows";
    public override string Version => "1";
    
    public static IDictionary<string, dynamic> PluginMain(string processName){
        IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();

        Processes p = new Processes();
        int status = -1;
        string message = null;
        //List<string> message = null;
        try{
            (status, message) = p.GetProc(processName);
        } catch (Exception e){
            ReturnDict.Add("Message", $"Exception: {e}");
            return ReturnDict;
        }
        if (status == 0){
            ReturnDict.Add("Status", status);
            ReturnDict.Add("Message", message);
        } else {
            ReturnDict.Add("Message", message);
        }
        return ReturnDict;
    }

        public (int, string) GetProc(string processName){
        //get a specific process

        //if no process is provided, get all of them
        if (processName == ""){
            return GetProc();
        }

        Process[] proc = null;
        try{
            proc = Process.GetProcessesByName(processName);
        } catch (Exception e) {
            return (1, $"Failed to get {processName}. Exception: {e}");
        }
        if (proc.Length == 0){
            return (1, $"Failed to get {processName}. It might not be running or the name is wrong.");
        } else {
            //a big string if there are multiple processes with the same name
            string processList = "";
            foreach (var p in proc){
                processList += $"{p.Id},{processName}\n";
            }
            return (0, processList);
        }
    }

    public (int, string) GetProc(){
        //get all processes and return a csv list of ID and Name
        Process[] procs = Process.GetProcesses();

        //a really big string
        string processList = "";

        foreach(var p in procs){
            processList += $"{p.Id},{p.ProcessName}\n";
        }
        return (0, processList);
    }

}