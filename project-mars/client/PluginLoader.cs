namespace MarsClient 
{
    using MarsClient.Plugins;
    using System;
    using System.Reflection;
    using System.Text;
    
    public static class PluginLoader
    {
        /// <summary>
        /// Initializes a plugin from a base64 string.
        /// </summary>
        /// <param name="_binary">The base64 string to initialize the plugin from.</param>
        /// <returns>The plugin as Plugin class, or null.</returns>
        public static Plugin InitializeFromBase64(string _binary)
        {
            try {
                Assembly asm = Assembly.Load(Convert.FromBase64String(_binary));
                return InitializeFromAssembly(asm);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"PluginLoader> Failed to load plugin from base64 string. Exception message: {exception.Message}");
            }
            Console.WriteLine($"PluginLoader> Failed to load plugin from base64 string.");
            return null;
        }

        /// <summary>
        /// Initializes a plugin from an assembly.
        /// </summary>
        /// <param name="_asm">The assembly to initialize the plugin from.</param>
        /// <returns>The plugin as Plugin class, or null.</returns>
        public static Plugin InitializeFromAssembly(Assembly _asm)
        {
            Console.WriteLine($"PluginLoader> Initializing plugin from assembly {_asm.FullName}...");
            try {
                foreach(Type oType in _asm.GetTypes()){
                    //if(oType.BaseType.FullName == typeof(Plugin).FullName){
                    if(oType.IsSubclassOf(typeof(Plugin))){
                        Console.WriteLine($"PluginLoader> Found {oType.Name.ToString()} in assembly as subclass of Plugin.");
                        Plugin plugin = (Plugin)Activator.CreateInstance(oType);
                        // save assembly to plugin
                        // allows us to call whatever the fuck we want from the plugin!
                        plugin.Assembly = _asm;
                        plugin.TypeName = oType.Name;

                        return plugin;
                    }
                }
                Console.WriteLine($"PluginLoader> Could not find valid Plugin class in assembly.");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"PluginLoader> Failed to load plugin from assembly. Exception message: {exception.Message}");
            }

            return null;
            
        }
    }
}