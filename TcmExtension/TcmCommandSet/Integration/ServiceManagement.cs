using System;
using Autofac;

namespace TcmCommandSet.Integration
{
	public class ServiceManagement
	{
		private readonly ILifetimeScope _scope;

		//private PlanManager _planManager;

		//private SuiteManager _suiteManager;

		//private TestCaseManager _testCaseManager;

		//private ConfigManager _configManager;

		//public PlanManager PlanManager
		//{
		//	// get { return _container.BeginLifetimeScope().Resolve<PlanManager>(); }
		//	get { return _planManager ?? (_planManager = _scope.Resolve<PlanManager>()); }
		//}

		//public SuiteManager SuiteManager
		//{
		//	// get { return _container.BeginLifetimeScope().Resolve<SuiteManager>(); }
		//	get { return _suiteManager ?? (_suiteManager = _scope.Resolve<SuiteManager>()); }
		//}

		//public TestCaseManager TestCaseManager
		//{
		//	// get { return _container.BeginLifetimeScope().Resolve<TestCaseManager>(); }
		//	get { return _testCaseManager ?? (_testCaseManager = _scope.Resolve<TestCaseManager>()); }
		//}

		//public ConfigManager ConfigManager
		//{
		//	// get { return _container.BeginLifetimeScope().Resolve<TestCaseManager>(); }
		//	get { return _configManager ?? (_configManager = _scope.Resolve<ConfigManager>()); }
		//}

		public object GetManager (Type t)
		{
			return _scope.Resolve(t);
		}

		public ServiceManagement(Option option)
		{
			ServicesContainer servicesContainer = new ServicesContainer(option);
			_scope = servicesContainer.Container.BeginLifetimeScope();
		}
	}
}
