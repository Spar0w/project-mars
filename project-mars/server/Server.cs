using System;

/*
    Init Class that is called to start the Server program.
    For actual functionality see the Listener class.
*/

namespace server
{
    public class Server{
        static void Main(string[] args){
            Console.Clear();
            //take command line arguments of Name, Port, IPaddress
            int tport;
            string tname;
            string tipaddress;
            try {
                tname = args[0];
                tipaddress = args[1];
                tport = Int32.Parse(args[2]);
                //Listener test = new Listener(args[0], tport, args[2]);
            } catch (System.IndexOutOfRangeException) {
                tport = 1234;
                tname = "Default";
                tipaddress = "127.0.0.1";
                
                Console.WriteLine("No arguments provided, using defaults...");
            }

            Listener test = new Listener(tname, tport, tipaddress);
            UserInput input = new UserInput(test);
            test.StartServer();
            //load already registered clients
            test.LoadRegisteredClients();
            test.LoadPlugins();
            //start the async listener
            //meant to not be awaited so that it runs in the background
            test.ListenAndRespond();
            //start our menu and take server input
            //acts as a block for the async functions
            input.MenuInput();

            //test plugin
            //var pb = new Playbook("./sampleplaybook.json");

            //stop the server
            test.Terminate();
        }
    }

}