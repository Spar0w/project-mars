using System;
using System.Collections.Generic;

namespace server{
    public class Agent{
        // a class to represent and hold data for each agent/client
        public string name{get; set;}
        public string ipAddress{get; set;}
        public LinkedList<dynamic[]> commandQue{get; set;}
        public List<string> commandResp{get; set;}
        public Agent(string name, string ipAddress){
            this.name = name;
            this.ipAddress = ipAddress;
            this.commandQue = new LinkedList<dynamic[]>();
            this.commandResp = new List<string>();
        }
    }

}
