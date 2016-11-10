using System;
using System.Collections.Generic;
using ClusterCommandLine;
using ClusterCommandLine.CommandLineRoute;
using Microsoft.TeamFoundation.TestManagement.Client;
using TcmCommandSet.Integration;

namespace TcmCommandSet.Commands
{
	[CommandRoutePrefix("config", @"/[collection:teamprojectcollectionurl]/[teamproject:project]/[login:username,[password]]",
		HelpText = "Provides operations of configuration")]
	public class ConfigCommand : Command
	{
		public override void ActionHelpHeader()
		{
		}

		public override void ActionHelpExample()
		{
		}

		[CommandRoute("addconfig", "/title:configurationname/description:configurationdescription")]
		public void AddConfig(Option option)
		{
			throw new NotImplementedException();
		}

		[CommandRoute("assignvariable", "/title:configurationname/name:variablename")]
		public void AddVariableToConfig(Option option)
		{
			throw new NotImplementedException();
		}

		[CommandRoute("deleteconfig", "/title:configurationname")]
		public void DeleteConfig(Option option)
		{
			throw new NotImplementedException();
		}

		[CommandRoute("addvariable", "/name:variablename/description:variabledescription")]
		public void AddVariableTo(Option option)
		{
			throw new NotImplementedException();
		}

		[CommandRoute("list", "/details:less|more")]
		public void ListConfig(Option option)
		{
			const int pad = 12;
			Console.WriteLine("{0}Name", "ID".PadRight(pad));
			for (int i = 1; i < pad; i++)
				Console.Write("-");
			Console.WriteLine(" ----------------------------------------------------------------");
			foreach (ITestConfiguration configuration in TfsService.ConfigManager.AllTestConfigurations)
			{
				Console.WriteLine("{0}{1}", configuration.Id.ToString().PadRight(pad), configuration.Name);
				if (!string.IsNullOrWhiteSpace(option.Details) && option.Details.Equals("more", StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (KeyValuePair<string, string> keyValuePair in configuration.Values)
					{
						for (int i = 0; i < pad; i++)
							Console.Write(" ");
						Console.WriteLine("variable=> Name:{0}, Description:{1}", keyValuePair.Key, keyValuePair.Value);
					}
				}
			}
		}
	}
}
