using System;
using System.Diagnostics;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using TcmCommandSet.Profile;

namespace TcmCommandSet.Integration
{
	public class TfsClient
	{
		private TfsClientCredentials _credential;

		public ITestManagementTeamProject TeamProject { get; private set; }

		public TfsTeamProjectCollection TeamProjectCollection { get; private set; }

		public TfsClient(Option option)
		{
			if (string.IsNullOrEmpty(option.Collection))
				if (string.IsNullOrEmpty(PreferenceManager.Instance.Collection))
                    throw new ArgumentNullException(option.Collection, "Collection is not specified");
				else
					option.Collection = PreferenceManager.Instance.Collection;

			if (string.IsNullOrEmpty(option.TeamProject))
				if (string.IsNullOrEmpty(PreferenceManager.Instance.TeamProject))
                    throw new ArgumentNullException(option.TeamProject, "TeamProject is not specified");
				else
					option.TeamProject = PreferenceManager.Instance.TeamProject;

			SetupTfsClient(
				option.Collection,
				option.TeamProject,
				option.UserName ?? PreferenceManager.Instance.UserName,
				option.Password ?? PreferenceManager.Instance.Password
				);
		}

		private void SetupTfsClient(
			string tfsProjectCollectionUri,
			string tfsProject,
			string user = null,
			string pass = null)
		{
			var projectCollectionUri = new Uri(tfsProjectCollectionUri);
			Authenticate(user, pass);
			TeamProjectCollection = new TfsTeamProjectCollection(projectCollectionUri, _credential);

			TeamProjectCollection.Authenticate();

			// TODO: throw exception if HasAuthenticated failed:
			Debug.WriteLine("Has Authenticated : {0}", TeamProjectCollection.HasAuthenticated);
			Debug.WriteLine("InstanceId : {0}", TeamProjectCollection.InstanceId);

			SetProject(tfsProject);
		}

		public ITestManagementTeamProject SetProject(string tfsProject)
		{
			var service = TeamProjectCollection.GetService<ITestManagementService>();
			TeamProject = service.GetTeamProject(tfsProject);
			return TeamProject;
		}

		private void Authenticate(string user, string pass)
		{
			if (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass))
			{
				_credential = new TfsClientCredentials() { AllowInteractive = true };
			}
			else
			{
				var netCred = new NetworkCredential(user, pass);
				var basicCred = new BasicAuthCredential(netCred);
				_credential = new TfsClientCredentials(basicCred) { AllowInteractive = true };
			}
		}
	}
}
