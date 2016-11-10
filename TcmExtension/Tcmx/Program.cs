using System;
using ClusterCommandLine;
using TcmCommandSet;
using TcmCommandSet.Integration;

namespace Tcmx
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TfsClientService.Instance.LoadTfsClientService(args.ToTcmOption());
                ClusterCommand.Exec<Option>(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("Command failed at {0}", e.Message);
            }
        }
    }
}
