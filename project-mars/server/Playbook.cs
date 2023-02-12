using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace server{
    public class Playbook{
        public string path {get; set;}
        public List<playbookCommand> playbookQue {get; set;}
        public List<KeyValuePair<string, string>> plugins {get; set;}
        public List<KeyValuePair<string, string>> commands {get; set;}

        public struct playbookCommand{
            public string type;
            public KeyValuePair<string, string> command;

        }

        public Playbook(string path){
            this.path = path;
            this.playbookQue = new List<playbookCommand>(); 

            LoadPlaybook(this);
            foreach(var o in playbookQue){
                Console.WriteLine(o);
            }
        }

        public List<playbookCommand> LoadPlaybook(Playbook pb){
            //local var to store the que
            var playbookQue = new List<playbookCommand>();

            try{
                //read the playbook file
                var reader =  new System.IO.StreamReader(pb.path);
                //get the plugins to be used
                JsonDocument pbContent = JsonDocument.Parse(reader.ReadToEnd());
                JsonElement plug = pbContent.RootElement.GetProperty("playbook");
                //parse the details from the json file
                var enumerator = plug.EnumerateObject();
                while(enumerator.MoveNext() == true){
                    //if it's an array, see if it's a plugin array or a command array
                    if (enumerator.Current.Value.ValueKind == JsonValueKind.Array){
                        //if it's an array, enumerate it and add it to the que
                        LoadPlaybookArray(enumerator);
                    } else {
                        //handle single instance commands or plugins
                        LoadPlaybookCommand(enumerator);
                    }
                }
                /*
                while(enumerator.MoveNext() == true){
                    //need to enumerate over the object to get the values
                    foreach (JsonProperty o in enumerator.Current.EnumerateObject()){
                        var pluginDeets = new KeyValuePair<string,string>(o.Name, o.Value.ToString());
                        plugins.Add(pluginDeets);
                    }
                }
                */
            } catch(Exception e){
                Console.WriteLine($"ERROR LOADING PLAYBOOK: {e}");
            }   
            return playbookQue;
        }
        private void LoadPlaybookCommand(JsonElement.ObjectEnumerator enumerator){
            //enumerates valutes of json objects into their respective lists
            if (enumerator.Current.Name == "plugin"){
                var enumer2 = enumerator.Current.Value;
                playbookCommand commandDeets = new playbookCommand{}; 
                foreach (JsonProperty o in enumer2.EnumerateObject()){
                    commandDeets.command = new KeyValuePair<string,string>(o.Name, o.Value.ToString());
                    commandDeets.type = "plugin";
                }
                if (playbookQue == null){
                    Console.WriteLine("Playbook que is null");
                }
                playbookQue.Add(commandDeets);
            } else if (enumerator.Current.Name == "command"){
                var enumer2 = enumerator.Current.Value.EnumerateObject();
                foreach (JsonProperty o in enumer2){
                    playbookCommand commandDeets = new playbookCommand{}; 
                    commandDeets.command = new KeyValuePair<string,string>(o.Name, o.Value.ToString());
                    commandDeets.type = "command";
                    playbookQue.Add(commandDeets);
                }

            }
        }
        
        private void LoadPlaybookArray(JsonElement.ObjectEnumerator enumerator){
            //enumerates and adds values of json arrays to their respective lists
            if (enumerator.Current.Name == "pluginArray"){
                //load up the array with the lable of "plugin"
                var enumer2 = enumerator.Current.Value.EnumerateArray();
                while(enumer2.MoveNext() == true){
                    foreach (JsonProperty o in enumer2.Current.EnumerateObject()){
                        playbookCommand pluginDeets; 
                        pluginDeets.command = new KeyValuePair<string,string>(o.Name, o.Value.ToString());
                        pluginDeets.type = "plugin";
                        playbookQue.Add(pluginDeets);
                    }
                }
            } else if (enumerator.Current.Name == "commandArray"){
                //load up the array with the lable of "command"
                var enumer2 = enumerator.Current.Value.EnumerateArray();
                while(enumer2.MoveNext() == true){
                    foreach (JsonProperty o in enumer2.Current.EnumerateObject()){
                        playbookCommand commandDeets; 
                        commandDeets.command = new KeyValuePair<string,string>(o.Name, o.Value.ToString());
                        commandDeets.type = "command";
                        playbookQue.Add(commandDeets);
                    }
                }
            } else {
                Console.WriteLine($"Playbook Error. A malformed array was provided: {enumerator.Current.Name}");
            }
        }
    }
}