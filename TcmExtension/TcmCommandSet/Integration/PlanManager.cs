using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace TcmCommandSet.Integration
{
	public class PlanManager : Manager
	{
		private readonly TfsClient _client;

		public PlanManager(TfsClient client)
		{
			_client = client;
		}

		public IList<ITestPlan> AllTestPlans
		{
			get
			{
				IList<ITestPlan> plans = _client.TeamProject.TestPlans.Query("Select * From TestPlan");
				return plans;
			}
		}

		public ITestPlan GetTestPlanByName(string name)
		{
			return AllTestPlans.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
