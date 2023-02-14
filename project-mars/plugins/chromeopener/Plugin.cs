//namespace chromeopener;
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
            IDictionary<string, dynamic> ReturnArray = new Dictionary<string, dynamic>();
            ReturnArray.Add("ExitCode", 1);
            ReturnArray.Add("ExitMessage", "Chrome not found!");
            return ReturnArray;
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
                IDictionary<string, dynamic> ReturnArray = new Dictionary<string, dynamic>();
                // return an exitcode, 0 for success, 1 for failure
                // ideally, we would return the exitcode of the process or dynamically set it based on the result of the process
                ReturnArray.Add("ExitCode", 0);
                ReturnArray.Add("ExitMessage", "Chrome opened successfully!");
                return ReturnArray;
            }
        }
    }
}
