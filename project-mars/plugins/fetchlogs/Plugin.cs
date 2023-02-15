//namespace chromeopener;
using MarsClient.Plugins;
using System.Diagnostics;
using System.IO;

class FetchEventLogs : Plugin{

    public override string Name => "FetchEventLogs";
    public override string Author => "Samuel Barrows and Damir Hajrovic";
    public override string Version => "v1";



    public static IDictionary<string, dynamic> PluginMain(){
    //public static void PluginMain(){
        string temppath = Path.GetTempFileName();

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "powershell.exe";
        startInfo.Arguments = $"-command Get-EventLog -LogName System -Newest 1000 | Export-csv {temppath}; exit;";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = false;
        startInfo.Verb = "runas";

        using (Process process = Process.Start(startInfo))
        {
            string s = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            using (StreamReader reader = process.StandardOutput)
            {
                //string result = await reader.ReadToEndAsync();

                // return a dict with info for client to process
                IDictionary<string, dynamic> ReturnArray = new Dictionary<string, dynamic>();
                // return an exitcode, 0 for success, 1 for failure
                // ideally, we would return the exitcode of the process or dynamically set it based on the result of the process
                ReturnArray.Add("Status", "Powershell opened successfully!");
                ReturnArray.Add("Message", "Powershell opened successfully!");
                string[] FileArray = {temppath};
                ReturnArray.Add("Files", FileArray);
                return ReturnArray;
            }
        }
    }
}
