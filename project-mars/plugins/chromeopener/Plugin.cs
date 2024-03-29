﻿//namespace chromeopener;
using System.Diagnostics;
using System.IO;
using MarsClient.Plugins;

class ChromeOpener : Plugin
{

    public override string Name => "ChromeOpener";
    public override string Author => "Damir Hajrovic";
    public override string Version => "v1";

    public static IDictionary<string, dynamic> PluginMain()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        if (File.Exists("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"))
        {
            startInfo.FileName = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
        }
        else if (File.Exists("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"))
        {
            startInfo.FileName = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
        }
        else
        {
            IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
            ReturnDict.Add("Message", "Chrome not found!");
            return ReturnDict;
        }
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = false;

        using (Process process = Process.Start(startInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                //string result = await reader.ReadToEndAsync();

                // return a dict with info for client to process
                IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
                // return an exitcode, 0 for success, 1 for failure
                // ideally, we would return the exitcode of the process or dynamically set it based on the result of the process
                //ReturnDict.Add("ExitCode", 0);
                ReturnDict.Add("Status", "0");
                ReturnDict.Add("Message", "Chrome Opened Successfully!");
                return ReturnDict;
            }
        }
    }

    [CommandAttributes("OpenURL", arguments = 1, description = "Open a URL in Chrome")]
    public IDictionary<string, dynamic> OpenURL(string url){
        // Demo method that returns true back to plugin handling system
        bool Status = true;
        string Message = $"Opened URL: {url} successfully!";

        ProcessStartInfo startInfo = new ProcessStartInfo();
        if (File.Exists("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"))
        {
            startInfo.FileName = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
        }
        else if (File.Exists("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"))
        {
            startInfo.FileName = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
        }
        else
        {
            IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
            ReturnDict.Add("Message", "Chrome not found!");
            return ReturnDict;
        }
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = false;
        startInfo.Arguments = url;

        using (Process process = Process.Start(startInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                //string result = await reader.ReadToEndAsync();

                // return a dict with info for client to process
                IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
                // return an exitcode, 0 for success, 1 for failure
                // ideally, we would return the exitcode of the process or dynamically set it based on the result of the process
                //ReturnDict.Add("ExitCode", 0);
                ReturnDict.Add("Status", "0");
                ReturnDict.Add("Message", $"Chrome Opened {url} Successfully!");
                return ReturnDict;
            }
        }
    }

    
    [CommandAttributes("OpenChrome", arguments = 0, description = "Open Chrome")]
    public IDictionary<string, dynamic> OpenChrome(string url){
        // Demo method that returns true back to plugin handling system
        bool Status = true;
        string Message = $"Opened URL: {url} successfully!";

        ProcessStartInfo startInfo = new ProcessStartInfo();
        if (File.Exists("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe"))
        {
            startInfo.FileName = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
        }
        else if (File.Exists("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"))
        {
            startInfo.FileName = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
        }
        else
        {
            IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
            ReturnDict.Add("Message", "Chrome not found!");
            return ReturnDict;
        }
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = false;

        using (Process process = Process.Start(startInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                //string result = await reader.ReadToEndAsync();

                // return a dict with info for client to process
                IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
                // return an exitcode, 0 for success, 1 for failure
                // ideally, we would return the exitcode of the process or dynamically set it based on the result of the process
                //ReturnDict.Add("ExitCode", 0);
                ReturnDict.Add("Status", "0");
                ReturnDict.Add("Message", "Chrome Opened Successfully!");
                return ReturnDict;
            }
        }
    }
}
