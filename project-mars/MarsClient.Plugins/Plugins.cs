// Inspired by code from https://github.com/Exiled-Team/EXILED/blob/master/Exiled.API/Features/Plugin.cs

namespace MarsClient.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;


    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class CommandAttributes : System.Attribute
    {
        public string name;
        public int arguments;
        public string description;

        public CommandAttributes(string name)
        {
            this.name = name;
            arguments = 2;
            description = "No description provided";
        }
    }

    /// <summary>
    /// The base plugin class.
    /// </summary>
    public class Plugin 
    {

        // various variables that can be used by plugins

        /// <summary>
        /// Gets the plugin name.
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// Gets the plugin author.
        /// </summary>
        public virtual string Author { get; set; }
        /// <summary>
        /// Gets the plugin version.
        /// </summary>
        public virtual string Version { get; set; }
        /// <summary>
        /// Gets the plugin assembly.
        /// </summary>
        public Assembly Assembly { get; set; }
        /// <summary>
        /// Gets the plugin type name.
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// Gets the plugin api version number.
        /// </summary>
        public virtual int APIVersion { get; set; } = 1;

        public Plugin()
        {
            // since constructor is called by plugin class itself, this will save a copy of itself to the class (i hope lol)
            //Assembly = Assembly.GetExecutingAssembly();
        }


        public IDictionary<string, dynamic> RunCommand(string? command, string[]? args)
        {
            // turn string args into list

            // declare dict we will need to pass to main agent for processing of this result
            IDictionary<string, dynamic> returnDataForClient = new Dictionary<string, dynamic>();

            if(command == null){
                // no command? you want PluginMain()? ok, let's do that
                command = "PluginMain";
            }

            IDictionary<string, dynamic> pluginCmdReturn = null;

            // get the method from the assembly
            MethodInfo methodInfo = Assembly.GetType(TypeName).GetMethod(command);
            // invoke the method
            try {
                pluginCmdReturn = (IDictionary<string, dynamic>) methodInfo.Invoke(this, args);
            } catch (TargetException) {
                Console.WriteLine($"MarsClient.Plugins> Failed to invoke method {command} from plugin {Name}. The specified method does not exist.");
                if (!returnDataForClient.ContainsKey("ExitCode")){
                    // set exit code to 1 for failure
                    returnDataForClient.Add("ExitCode", 1);
                }
                if (!returnDataForClient.ContainsKey("ExitMessage")){
                    // set message to exception message
                    returnDataForClient.Add("ExitMessage", "Command not found.");
                }
            } catch (Exception e) {
                Console.WriteLine($"MarsClient.Plugins> Failed to invoke method {command} from plugin {Name}. Exception message: {e.Message}");
                if (!returnDataForClient.ContainsKey("ExitCode")){
                    // set exit code to 1 for failure
                    returnDataForClient.Add("ExitCode", 1);
                }
                if (!returnDataForClient.ContainsKey("ExitMessage")){
                    // set message to exception message
                    returnDataForClient.Add("ExitMessage", e.Message);
                }
            }
            
            try {
                if( pluginCmdReturn.ContainsKey("Status")){
                    // set exit code to 0 for success

                    // combine the plugin's return and the new return
                    // this should enable the custom file return function
                    foreach(KeyValuePair<string, dynamic> msgs in pluginCmdReturn){
                        returnDataForClient.Add(msgs);
                    }

                    returnDataForClient.Add("ExitCode", 0);
                    // set message to success message
                    returnDataForClient.Add("ExitMessage", pluginCmdReturn["Message"]);
                }else{
                    if (!returnDataForClient.ContainsKey("ExitCode")){
                        // set exit code to 1 for failure
                        returnDataForClient.Add("ExitCode", 1);
                    }
                    if (!returnDataForClient.ContainsKey("ExitMessage")){
                        // set message to exception message
                        returnDataForClient.Add("ExitMessage", pluginCmdReturn["Message"]);
                    }
                }
            } catch {
                // guesss i didn't get a status back? let's assume it failed
                if (!returnDataForClient.ContainsKey("ExitCode")){
                    // set exit code to 1 for failure
                    returnDataForClient.Add("ExitCode", 1);
                }
                if (!returnDataForClient.ContainsKey("ExitMessage")){
                    // set message to exception message
                    returnDataForClient.Add("ExitMessage", "Invalid data set, terminating.");
                }
            }

            // parent listener demands a dictionary with at least exit code and message
            // exit code is 0 for success, 1 for failure
            // message is the message to be displayed to the user
            // if you want to return more data, you can add more keys to the dictionary
            return returnDataForClient;
            //yeah
        }
        
    }
}