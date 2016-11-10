using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TcmCommandSet.Integration
{
	public class SuiteManager : Manager
	{
		private readonly TfsClient _client;

		public SuiteManager(TfsClient client)
		{
			_client = client;
		}

		public ITestSuiteBase GetSuiteById(int id)
		{
			return GetSuiteById(_client, id);
		}

		/// <summary>
		/// Get test suite by their id
		/// </summary>
		/// <param name="tfsClient">
		/// The <see cref="TfsClient"/> used for communicating with team foundation server.
		/// </param>
		/// <param name="id">
		/// The test suite id.
		/// </param>
		/// <returns>
		/// The <see cref="ITestSuiteBase"/> that matches the suite id.
		/// </returns>
		public static ITestSuiteBase GetSuiteById(TfsClient tfsClient, int id)
		{
			if (tfsClient == null)
				throw new ArgumentNullException(nameof(tfsClient));

			return tfsClient.TeamProject.TestSuites.Query("SELECT * FROM TestSuite")
				.SingleOrDefault(s => s.Id.Equals(id));
		}

		public IEnumerable<ITestSuiteBase> GetSuites(string tfsPlanName, string tfsSuiteName = null)
		{
			var suites = _client.TeamProject.TestSuites.Query("SELECT * FROM TestSuite")
				.Where(
					s =>
						Regex.IsMatch(s.Plan.Name, tfsPlanName, RegexOptions.IgnoreCase) &&
						(string.IsNullOrEmpty(tfsSuiteName) || Regex.IsMatch(s.TestSuiteEntry.Title, tfsSuiteName, RegexOptions.IgnoreCase)));

			// ReSharper disable once PossibleMultipleEnumeration
			if (suites.ToList().Count == 0)
				throw new ItemNotFoundException("Could not find Test Suite with attibutes: Suite Name : " + tfsSuiteName +
				                                ", and Plan Name : " + tfsPlanName);

			return suites;
		}

	    public string GetSuiteQuery(string suiteId)
	    {
	        var id = Int32.Parse(suiteId);
	        var suite = GetSuiteById(id);
            if (suite == null)
            {
                throw new TestObjectNotFoundException(
                    $"Test suite with ID:{suiteId} is not found.");
            }
	        if (suite is IDynamicTestSuite)
	        {
                IDynamicTestSuite dynamicTestSuite = suite as IDynamicTestSuite;
                return dynamicTestSuite.Query.QueryText;
            }
	        throw new TestObjectNotFoundException(
	            $"Test suite with ID:{suiteId} is not query based test suite.");
	    }

		public string GetSuiteQuery(string testPlanName, string testSuiteName)
		{
			ITestSuiteBase suite = GetSuites(testPlanName, testSuiteName)
				.FirstOrDefault(
					s =>
						s.TestSuiteType.Equals(TestSuiteType.DynamicTestSuite));
			if (suite == null)
			{
				throw new TestObjectNotFoundException(
					String
						.Format(
							"Could not find query based test suite {0} .",
							testSuiteName));
			}
			IDynamicTestSuite dynamicTestSuite = suite as IDynamicTestSuite;
			return dynamicTestSuite.Query.QueryText;
		}

		public IEnumerable<ITestSuiteBase> CreateSuite(
			string testPlanName,
			string rootSuiteName,
			string newSuiteName,
			string newSuiteDescription,
			string suiteType)
		{
			ITestSuiteBase rootSuite = GetSuites(testPlanName, rootSuiteName)
				.FirstOrDefault(
					s =>
						s.TestSuiteType.Equals(TestSuiteType.StaticTestSuite));
			if (rootSuite == null)
			{
				throw new ItemNotFoundException(
					String
						.Format(
							"Could not find Static Test Suite {0} to create new test suite undernearth.",
							rootSuiteName));
			}

			ITestSuiteBase newSuite;
			if (suiteType.Equals("static", StringComparison.InvariantCultureIgnoreCase))
			{
				newSuite = _client.TeamProject.TestSuites.CreateStatic();
			}
			else if (suiteType.Equals("dynamic", StringComparison.InvariantCultureIgnoreCase))
			{
				newSuite = _client.TeamProject.TestSuites.CreateDynamic();
			}
			else if (suiteType.Equals("requirement", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new NotImplementedException("add requirement suite");
			}
			else
			{
				throw new NotImplementedException("add basic suite");
			}

			//var dt = DateTime.Now;
			newSuite.Title = newSuiteName;
			newSuite.Description = newSuiteDescription;

			var suiteEntries = rootSuite as IStaticTestSuite;
			if (suiteEntries != null)
			{
				suiteEntries.Entries.Add(newSuite);
				rootSuite.Plan.Save();
			}
			return GetSuites(testPlanName, newSuiteName);
		}

		// copied from http://automatetheplanet.com/manage-tfs-test-suites-csharp-code/
		public void DeleteSuite(ITestPlan testPlan, int suiteToBeRemovedId, IStaticTestSuite parent = null)
		{
			ITestSuiteBase currentSuite = _client.TeamProject.TestSuites.Find(suiteToBeRemovedId);
			if (currentSuite == null)
			{
				throw new TestObjectNotFoundException(string.Format("Suite can't be found"));
			}
			// Remove the parent child relation. This is the only way to delete the suite.
			if (parent != null)
			{
				parent.Entries.Remove(currentSuite);
			}
			else if (currentSuite.Parent != null)
			{
				currentSuite.Parent.Entries.Remove(currentSuite);
			}
			else
			{
				// If it's initial suite, remove it from the test plan.
				testPlan.RootSuite.Entries.Remove(currentSuite);
			}

			// Apply changes to the suites
			testPlan.Save();
		}

	    public void AppendQueryConditions(string suiteId , string queryConditions, string recursive)
	    {
            var id = Int32.Parse(suiteId);
            ITestSuiteBase suite = GetSuiteById(id);
            if (suite == null)
            {
                throw new TestObjectNotFoundException(
                    $"Test suite with ID \"{suiteId}\" is not found.");
            }
	        AppendQueryConditions(suite, queryConditions, recursive);
	    }

        public void AddQueryConditions(ITestPlan testPlan, string testSuiteName, string queryConditions)
		{
			if (string.IsNullOrWhiteSpace(testSuiteName))
			{
				List<IDynamicTestSuite> dynamicTestSuites = new List<IDynamicTestSuite>();
				ITestSuiteEntryCollection suiteCollection = testPlan.RootSuite.Entries;
				GetAllQueryBasedTestSuiteFromSuiteNode(suiteCollection, dynamicTestSuites);
				foreach (IDynamicTestSuite dynamicTestSuite in dynamicTestSuites)
				{
					AddQueryConditionsToSuite(dynamicTestSuite, queryConditions);
				}
			}
			else
			{
				ITestSuiteBase testSuite = GetSuites(testPlan.Name, testSuiteName)
					.FirstOrDefault(
						s =>
							s.TestSuiteType.Equals(TestSuiteType.DynamicTestSuite));
				if (testSuite == null)
				{
					throw new TestObjectNotFoundException(string.Format("Query based test suite - {0} not found", testSuiteName));
				}
				var dynamicTestSuite = testSuite as IDynamicTestSuite;
				AddQueryConditionsToSuite(dynamicTestSuite, queryConditions);
			}
		}

        public void AppendQueryConditions(ITestSuiteBase suite, string queryConditions, string recursive = "false")
        {
            if (suite == null)
            {
                Console.WriteLine("No suite found");
                return;
            }
            if (recursive.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                if (suite is IDynamicTestSuite)
                {
                    var dynamicTestSuite = suite as IDynamicTestSuite;
                    AddQueryConditionsToSuite(dynamicTestSuite, queryConditions);
                    Console.WriteLine("!!!Query is updated but the test suite is a dynamic one and recursive should not be true!!!");
                }
                else
                {
                    List<IDynamicTestSuite> dynamicTestSuites = new List<IDynamicTestSuite>();
                    var staticSuite = suite as IStaticTestSuite;
                    if (staticSuite != null)
                    {
                        GetAllQueryBasedTestSuiteFromSuiteNode(staticSuite.Entries, dynamicTestSuites);
                        foreach (IDynamicTestSuite dynamicTestSuite in dynamicTestSuites)
                        {
                            AddQueryConditionsToSuite(dynamicTestSuite, queryConditions);
                        }
                    }
                }
            }
            else
            {
                if (suite is IDynamicTestSuite)
                {
                    var dynamicTestSuite = suite as IDynamicTestSuite;
                    AddQueryConditionsToSuite(dynamicTestSuite, queryConditions);
                    return;
                }
                Console.WriteLine("Test suite is not dynamic suite and can't be set query");
            }
        }

        public void ReplaceQueryConditions(string suiteId, string replaced, string replacing, string recursive = "false")
        {
            var id = Int32.Parse(suiteId);
            var suite = GetSuiteById(id);
            if (suite == null)
            {
                throw new TestObjectNotFoundException(
                    $"Test suite with ID \"{suiteId}\" is not found.");
            }
            if (recursive.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                if (suite is IDynamicTestSuite)
                {
                    var dynamicTestSuite = suite as IDynamicTestSuite;
                    UpdateQueryConditionsToSuite(dynamicTestSuite, replaced, replacing);
                    Console.WriteLine("!!!Query string is updated but the test suite is a dynamic one and recursive should not be true!!!");
                }
                else
                {
                    List<IDynamicTestSuite> dynamicTestSuites = new List<IDynamicTestSuite>();
                    var staticSuite = suite as IStaticTestSuite;
                    if (staticSuite != null)
                    {
                        GetAllQueryBasedTestSuiteFromSuiteNode(staticSuite.Entries, dynamicTestSuites);
                        foreach (IDynamicTestSuite dynamicTestSuite in dynamicTestSuites)
                        {
                            UpdateQueryConditionsToSuite(dynamicTestSuite, replaced, replacing);
                        }
                    }
                }
            }
            else
            {
                if (suite is IDynamicTestSuite)
                {
                    var dynamicTestSuite = suite as IDynamicTestSuite;
                    UpdateQueryConditionsToSuite(dynamicTestSuite, replaced, replacing);
                    return;
                }
                Console.WriteLine("Test suite is not dynamic suite and can't be set query");
            }
        }

        public void UpdateQueryConditions(ITestPlan testPlan, string testSuiteName, string replaced, string replacing)
		{
			if (string.IsNullOrWhiteSpace(testSuiteName))
			{
				List<IDynamicTestSuite> dynamicTestSuites = new List<IDynamicTestSuite>();
				ITestSuiteEntryCollection suiteCollection = testPlan.RootSuite.Entries;
				GetAllQueryBasedTestSuiteFromSuiteNode(suiteCollection, dynamicTestSuites);
				foreach (IDynamicTestSuite dynamicTestSuite in dynamicTestSuites)
				{
					UpdateQueryConditionsToSuite(dynamicTestSuite, replaced, replacing);
				}
			}
			else
			{
				ITestSuiteBase testSuite = GetSuites(testPlan.Name, testSuiteName)
					.FirstOrDefault(
						s =>
							s.TestSuiteType.Equals(TestSuiteType.DynamicTestSuite));
				if (testSuite == null)
				{
					throw new TestObjectNotFoundException(string.Format("Query based test suite - {0} not found", testSuiteName));
				}
				var dynamicTestSuite = testSuite as IDynamicTestSuite;
				UpdateQueryConditionsToSuite(dynamicTestSuite, replaced, replacing);
			}
		}

		public void UpdateConfigToSuites(string suiteId, string configIds, string status, string recursive, string testFieldsFile, bool overWrite = false)
		{
			int id = Int32.Parse(suiteId);
			string[] ids = configIds.Split('+');
			List<IdAndName> idAndNames = new List<IdAndName>();
			foreach (var i in ids)
			{
				ITestConfiguration config = TfsService.ConfigManager.GetTestConfigurationById(Int32.Parse(i));
				if (config != null)
					idAndNames.Add(new IdAndName(config.Id, config.Name));
			}
			if ("true".Equals(recursive, StringComparison.InvariantCultureIgnoreCase))
			{
				ICollection<ITestSuiteBase> testSuites = new List<ITestSuiteBase>();
				GetAllTestSuitesFromSuiteNode(id, testSuites);
				foreach (ITestSuiteBase testSuiteBase in testSuites)
					UpdateConfigToSuite(testSuiteBase.Id, idAndNames, status, testFieldsFile, overWrite);
			}
			else
			{
				UpdateConfigToSuite(id, idAndNames, status, testFieldsFile, overWrite);
			}
		}

		public void UpdateConfigToSuite(int suiteId, List<IdAndName> idAndNames, string status, string testFieldsFile, bool overWrite)
		{
			var suite = GetSuiteById(suiteId);
			IEnumerable<ITestSuiteEntry> entries = null;
			if (status.Equals("all", StringComparison.InvariantCultureIgnoreCase))
			{
				entries = suite.TestCases;
			}
			if (status.Equals("automated", StringComparison.InvariantCultureIgnoreCase))
			{
				entries = suite.TestCases.Where(t => t.TestCase.IsAutomated);
			}
			if (status.Equals("nonauto", StringComparison.InvariantCultureIgnoreCase))
			{
				entries = suite.TestCases.Where(t => !t.TestCase.IsAutomated);
			}
			var matchedEntries = TfsService.TestCaseManager.MatchedTestSuiteEntries(testFieldsFile, entries);
			if (matchedEntries != null && matchedEntries.Any())
			{
				if (overWrite.Equals(true))
				{
					suite.SetEntryConfigurations(matchedEntries, idAndNames);
				}
				else
				{
					foreach (ITestSuiteEntry testSuiteEntry in matchedEntries)
					{
						var addedConfigs = new List<IdAndName>(testSuiteEntry.Configurations);
						addedConfigs.AddRange(idAndNames);
						var distinctConfigs = addedConfigs.Distinct();
						testSuiteEntry.SetConfigurations(distinctConfigs);
					}
				}
			}
			// ITestPoint point = suite.Plan.QueryTestPoints("SELECT * FROM TestPoint WHERE SuiteId = '" + id + "'").FirstOrDefault();
		}

		#region Private Method

		private void GetAllTestSuitesFromSuiteNode(
			int rootSuiteId,
			ICollection<ITestSuiteBase> testSuites)
		{
			ITestSuiteBase suite = GetSuiteById(rootSuiteId);
			if (suite == null) return;

			testSuites.Add(suite);

			if (suite.TestSuiteType.Equals(TestSuiteType.StaticTestSuite))
			{
				IStaticTestSuite staticSuite = suite as IStaticTestSuite;
				if (staticSuite != null)
					foreach (ITestSuiteEntry testSuiteEntry in staticSuite.Entries)
						GetAllTestSuitesFromSuiteNode(testSuiteEntry.Id, testSuites);
			}
		}

	    private string AddQueryConditionsToSuite(IDynamicTestSuite suite, string query)
	    {
	        string newQuery = null;
	        try
	        {
	            string queryWithoutOrder;
	            string order;

	            if (suite.Query.QueryText.Contains("order by"))
	            {
	                queryWithoutOrder = suite.Query.QueryText
	                    .Substring(0, suite.Query.QueryText
	                        .IndexOf("order", StringComparison.InvariantCultureIgnoreCase) - 1);
	                order = suite.Query.QueryText
	                    .Substring(suite.Query.QueryText
	                        .IndexOf("order", StringComparison.InvariantCultureIgnoreCase));
	            }
	            else
	            {
	                queryWithoutOrder = suite.Query.QueryText;
	                order = string.Empty;
	            }

	            if (suite.Query.QueryText.Contains("where"))
	            {
	                newQuery = queryWithoutOrder + " " + query + " " + order;
	            }
	            else
	            {
	                if (query.StartsWith("and", StringComparison.CurrentCultureIgnoreCase))
	                {
	                    query = query.Substring(4);
	                }
	                if (query.StartsWith("or", StringComparison.CurrentCultureIgnoreCase))
	                {
	                    query = query.Substring(3);
	                }
	                newQuery = queryWithoutOrder + " where " + query + " " + order;
	            }
	            suite.Query = _client.TeamProject.CreateTestQuery(newQuery);

	            suite.Plan.Save();
	            Console.WriteLine("Test Suite \"{0}\" the query string now is:\n{1}", suite.Title, newQuery);
	        }
	        catch (Exception e)
	        {
	            Console.BackgroundColor = ConsoleColor.Red;
	            Console.ForegroundColor = ConsoleColor.White;
	            Console.WriteLine($"Test Suite \"{suite.Title}\" with ID \"{suite.Id}\" meets errors:{e.InnerException.Message}");
	            Console.ResetColor();
	        }
	        return newQuery;
	    }

	    private void GetAllQueryBasedTestSuiteFromSuiteNode(ITestSuiteEntryCollection suiteCollection, List<IDynamicTestSuite> dynamicTestSuites)
		{
			foreach (var suiteEntry in suiteCollection)
			{
				if (suiteEntry.TestSuite != null)
				{
					if (suiteEntry.TestSuite.TestSuiteType == TestSuiteType.DynamicTestSuite)
					{
						IDynamicTestSuite suite = suiteEntry.TestSuite as IDynamicTestSuite;
						dynamicTestSuites.Add(suite);
					}
					else if (suiteEntry.TestSuite.TestSuiteType == TestSuiteType.StaticTestSuite)
					{
						IStaticTestSuite parentStaticSuite = suiteEntry.TestSuite as IStaticTestSuite;
						if (parentStaticSuite != null)
							GetAllQueryBasedTestSuiteFromSuiteNode(parentStaticSuite.Entries, dynamicTestSuites);
					}
				}
			}
		}

	    private void UpdateQueryConditionsToSuite(IDynamicTestSuite suite, string replaced, string replacing)
	    {
	        bool hasReplacedString = true;
	        try
	        {
	            if (!suite.Query.QueryText.Contains(replaced))
	            {
	                hasReplacedString = false;
	                goto Out;
	            }
	            string newQuery = suite.Query.QueryText.Replace(replaced, replacing);

	            suite.Query = _client.TeamProject.CreateTestQuery(newQuery);

	            Console.WriteLine("Test Suite \"{0}\" the query string now is:\n{1}", suite.Title, newQuery);
	        }
	        catch (Exception e)
	        {
	            Console.BackgroundColor = ConsoleColor.Red;
	            Console.ForegroundColor = ConsoleColor.White;
	            Console.WriteLine(
	                $"Test Suite \"{suite.Title}\" with ID \"{suite.Id}\" meets errors:{e.InnerException.Message}");
	            Console.ResetColor();
	        }
Out:
	        if (!hasReplacedString)
	        {
	            throw new TestObjectNotFoundException($"Replaced query condition \"{replaced}\" not found");
	        }
	    }

	    #endregion
	}
}
