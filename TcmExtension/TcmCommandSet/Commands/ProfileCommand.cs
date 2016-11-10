using System;
using ClusterCommandLine;
using ClusterCommandLine.CommandLineRoute;
using TcmCommandSet.Profile;

namespace TcmCommandSet.Commands
{
	[CommandRoutePrefix("profile", HelpText = "Set you local profile")]
	public class ProfileCommand : Command
	{
		[CommandRoute("set", @"/[path:currentpath|specifyyourenvironmentpath]/[collection:teamprojectcollectionurl]/[teamproject:project]/[login:username,[password]]")]
		public void SetLocal(Option option)
		{
		    string commandPath = null;
            if (!string.IsNullOrWhiteSpace(option.Path))
			{
				commandPath = option.Path.Equals("currentpath", StringComparison.InvariantCultureIgnoreCase)? Environment.CurrentDirectory : option.Path;

                string path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);

			    if (path.IndexOf(commandPath, StringComparison.InvariantCultureIgnoreCase) < 0)
			    {
                    string userPath = path + ";" + commandPath;
                    Environment.SetEnvironmentVariable("Path", userPath, EnvironmentVariableTarget.User);
                }

			    path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process);
                if (path.IndexOf(commandPath, StringComparison.InvariantCultureIgnoreCase) < 0)
			    {
                    string processPath = path + ";" + commandPath;
                    Environment.SetEnvironmentVariable("Path", processPath, EnvironmentVariableTarget.Process);
                }
			}

			PreferenceManager.Instance.SetPreference(
				option.Collection,
				option.TeamProject,
				option.UserName,
				option.Password,
                commandPath);
		}

		[CommandRoute("show", @"/details:All|Collection|TeamProject|UserName|Password")]
		public void GetLocal(Option option)
		{
			PreferenceManager.Instance.ShowPreference(option.Details);
		}

        [CommandRoute("sample", @"/copyto:c:\users")]
        public void GetSampleFiles(Option option)
        {
            String commandPath = PreferenceManager.Instance.GetPreference().CommandPath;
            if (string.IsNullOrWhiteSpace(commandPath))
            {
                Console.WriteLine("Please run command \"tcmx profile set /path:specifyyourenvironmentpath] to set your command path first.");
                return;
            }
            commandPath = commandPath + "\\Sample";
            PreferenceManager.Instance.DirectoryCopy(commandPath, option.CopyTo, true);
        }

        public override void ActionHelpHeader()
		{
		}

		public override void ActionHelpExample()
		{
		}
	}
}
