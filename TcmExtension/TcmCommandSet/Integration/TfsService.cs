namespace TcmCommandSet.Integration
{
	public static class TfsService
	{
		//public static PlanManager PlanManager
		//{
		//	get { return TfsClientService.ServiceManagement.PlanManager; }
		//}

		//public static SuiteManager SuiteManager
		//{
		//	get { return TfsClientService.ServiceManagement.SuiteManager; }
		//}

		//public static TestCaseManager TestCaseManager
		//{
		//	get { return TfsClientService.ServiceManagement.TestCaseManager; }
		//}

		//public static ConfigManager ConfigManager
		//{
		//	get { return TfsClientService.ServiceManagement.ConfigManager; }
		//}

		public static PlanManager PlanManager
		{
			get { return TfsClientService.ServiceManagement.GetManager(typeof(PlanManager)) as PlanManager; }
		}

		public static SuiteManager SuiteManager
		{
			get { return TfsClientService.ServiceManagement.GetManager(typeof(SuiteManager)) as SuiteManager; }
		}

		public static TestCaseManager TestCaseManager
		{
			get { return TfsClientService.ServiceManagement.GetManager(typeof(TestCaseManager)) as TestCaseManager; }
		}

		public static ConfigManager ConfigManager
		{
			get { return TfsClientService.ServiceManagement.GetManager(typeof(ConfigManager)) as ConfigManager; }
		}
	}
}
