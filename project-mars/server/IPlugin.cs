using System;

namespace server{
    public class Plugin{
        /*
        * Basic class to store basic information about plugins
        */
        public string Name{get; set;}
        public string Commands{get; set;}
        public string Path {get; set;}

        public Plugin(string Name, string Commands, string Path){
            this.Name = Name;
            this.Commands = Commands;
        }
    }
}
