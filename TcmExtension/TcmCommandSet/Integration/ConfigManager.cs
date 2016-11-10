using System;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace TcmCommandSet.Integration
{
	public class ConfigManager : Manager
	{
		private readonly TfsClient _client;

		public ConfigManager(TfsClient client)
		{
			_client = client;
		}

		public ITestConfigurationCollection AllTestConfigurations
		{
			get
			{
				return _client.TeamProject.TestConfigurations.Query(
					"Select * from TestConfiguration");
			}
		}

		public ITestConfiguration GetTestConfigurationByName(string name)
		{
			return AllTestConfigurations.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		public ITestConfiguration GetTestConfigurationById(int id)
		{
			return AllTestConfigurations.FirstOrDefault(c => c.Id.Equals(id));
		}
	}
}
