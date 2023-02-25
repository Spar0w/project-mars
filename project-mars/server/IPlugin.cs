using System;
using System.Collections.Generic;

namespace server {
    public class IPlugin{
        /*
        * Basic class to store basic information about plugins
        */
        public string Name{get; set;}
        public Dictionary<string, IPluginMethod> Methods { get; set;}
        public string Path {get; set;}

        public IPlugin(string Name, Dictionary<string, IPluginMethod> meth, string Path){
            this.Name = Name;
            this.Methods = meth;
            this.Path = Path;
        }

        
        
    }

    public class IPluginMethod
    {
        public string name { get; set; }
        public string description { get; set; }
        public int arguements { get; set; }

        public IPluginMethod(string Name, string Description, int Arguements)
        {
            this.name = Name;
            this.description = Description;
            this.arguements = Arguements;
        }
    }
}
