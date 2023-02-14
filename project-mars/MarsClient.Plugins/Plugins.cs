// Inspired by code from https://github.com/Exiled-Team/EXILED/blob/master/Exiled.API/Features/Plugin.cs

namespace MarsClient.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

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
        /// Plugin commands and their methods.
        /// </summary>
        public virtual Dictionary<string,Func<string,string[]>> CommandMethods { get; set;}

        public Plugin()
        {
            // since constructor is called by plugin class itself, this will save a copy of itself to the class (i hope lol)
            //Assembly = Assembly.GetExecutingAssembly();
        }


        public IDictionary<string, dynamic> RunCommand(string command, string[]? args)
        {
            // turn string args into list
            
            // find relevent method from commandMethods dictionary
            if(CommandMethods.TryGetValue(command, out Func<string, string[]> method)){
                method.Invoke(args);
            }

            // parent listener demands a dictionary with at least exit code and message
            // exit code is 0 for success, 1 for failure
            // message is the message to be displayed to the user
            // if you want to return more data, you can add more keys to the dictionary

            IDictionary<string, dynamic> returnData = new Dictionary<string, dynamic>();

            return returnData;
            //yeah
        }
        
    }
}