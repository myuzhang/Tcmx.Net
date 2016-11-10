using System;
using System.Linq;
using ClusterCommandLine;
using ClusterCommandLine.CommandLineRoute;
using Microsoft.TeamFoundation.TestManagement.Client;
using TcmCommandSet.Integration;

namespace TcmCommandSet.Commands
{
	[CommandRoutePrefix("testcase",
		@"/[collection:teamprojectcollectionurl]/[teamproject:project]/[login:username,[password]]",
		HelpText = "Provides operations to create and update test cases")]
	public class TestCaseCommand : Command
	{
		[CommandRoute("import",
			"/storage:path/[maxpriority:priority]/[minpriority:priority]/[category:filter]/[syncsuite:id]/[testfields:testfieldsfile]"
			)]
		public void ImportTestCases(Option option)
		{
			ITestSuiteBase tempSuite = null;
			if (string.IsNullOrWhiteSpace(option.SyncSuite) && string.IsNullOrWhiteSpace(option.TestFields))
			{
				TfsService.TestCaseManager.ImportTestCases(option);
			}
			else
			{
				try
				{
					var destinationSuite = TfsService.SuiteManager.GetSuiteById(Int32.Parse(option.SyncSuite));
					if (destinationSuite is IStaticTestSuite)
					{
						TfsService.TestCaseManager.ImportTestCases(option);
						if (!string.IsNullOrWhiteSpace(option.TestFields))
						{
							TfsService.TestCaseManager.AddCustomFieldsToTestCases(option.TestFields, destinationSuite.AllTestCases);
						}
					}
					else
					{
						tempSuite = TfsService
							.SuiteManager
							.CreateSuite(destinationSuite.Plan.Name, null, "temp" + DateTime.Now.Minute.ToString(), "temp", "static")
							.FirstOrDefault();

						if (tempSuite != null)
						{
							option.SyncSuite = tempSuite.Id.ToString();
							TfsService.TestCaseManager.ImportTestCases(option);
							tempSuite = TfsService.SuiteManager.GetSuiteById(tempSuite.Id);
							if (!string.IsNullOrWhiteSpace(option.TestFields))
							{
								TfsService.TestCaseManager.AddCustomFieldsToTestCases(option.TestFields, tempSuite.AllTestCases);
							}
							Console.WriteLine(string.Format("\n{0} test cases uploaded.", tempSuite.TestCaseCount));
							TfsService.TestCaseManager.CloneTestCases(tempSuite, destinationSuite);
						}
					}
				}
				finally
				{
					if (tempSuite != null)
					{
						TfsService.SuiteManager.DeleteSuite(tempSuite.Plan, tempSuite.Id);
					}
				}
			}
		}

		[CommandRoute("list", "/suiteid:id")]
		public void ListTestCasesInSuite(Option option)
		{
			var suite = TfsService.SuiteManager.GetSuiteById(Int32.Parse(option.SuiteId));
			if (suite != null)
			{
				TfsService.TestCaseManager.ListTestCasesInSuite(suite);
			}
		}

		[CommandRoute("bulkcreate",
			"/testcollection:testcollectionfile/testfields:testfieldsfile/[suiteid:id]/[configid:configurationid1+configurationid2]"
			)]
		public void CreateTestCasesInSuite(Option option)
		{
			ITestSuiteBase suite = null;
			if (option.SuiteId != null)
			{
				suite = TfsService.SuiteManager.GetSuiteById(Int32.Parse(option.SuiteId));
			}

			TfsService.TestCaseManager.CreateTestCases(option.TestCollection, option.TestFields, suite, option.ConfigId);
		}

		[CommandRoute("bulkupdatefield", "/field:field/value:value/suiteid:id/[recursive:true|false]")]
		public void BulkUpdateFiled(Option option)
		{
			ITestSuiteBase suite = TfsService.SuiteManager.GetSuiteById(Int32.Parse(option.SuiteId));

		    if (suite == null)
		    {
		        Console.WriteLine("Test suite does not exist.");
		        return;
		    }

		    TfsService.TestCaseManager.BulkUpdateFiled(
				option.Field,
				option.Value,
				suite,
				!"false".Equals(option.Recursive, StringComparison.CurrentCultureIgnoreCase));
		}

		[CommandRoute("bulkcreate2", "/testcollection:testcollectionexcelfile/[suiteid:id]")]
		public void CreateTestCasesInSuiteWithExcel(Option option)
		{
			ITestSuiteBase suite = null;
			if (option.SuiteId != null)
			{
				suite = TfsService.SuiteManager.GetSuiteById(Int32.Parse(option.SuiteId));
			}

			TfsService.TestCaseManager.CreateTestCases(option.TestCollection, suite);
		}


		[CommandRoute("details", "/testcaseid:id")]
		public void GetTestCaseDetails(Option option)
		{
			TfsService.TestCaseManager.GetTestCaseDetails(option.TestCaseId);
		}

		public override void ActionHelpHeader()
		{
		}

		public override void ActionHelpExample()
		{
			Console.WriteLine(
				"Example1: Import test cases from local to TFS server static or requirement based suite (In Developer Command Prompt Console)");
			Console.Write(
				"Command1: tcmx testcase import /collection:https://tfs.yourUrl/tfs/DefaultCollection ");
			Console.Write(
				"/teamproject:yourTeamProject /syncsuite:12 /storage:Tests.dll /category:\"project-id\"\n");
			Console.WriteLine(
				"\nExample2: Bulk add test cases from files (to static or requirement based suite) with test fields");
			Console.WriteLine(
                "Command2: tcmx testcase bulkcreate /collection:https://tfs.yourUrl/tfs/DefaultCollection /teamproject:yourTeamProject /testcollection:testcases.txt /testfields:testfields.txt /[suiteid:id]");
			Console.WriteLine(@"========================================================================
testcases.txt is a file defining test cases, which has this format:
Test Case Title 1
-test step 1 #expected result 1
Test Case Title 2
-test step 1
-test step 2 #expected result 2
Test Case Title 3
========================================================================");
			Console.WriteLine(@"testfields.txt is a file specifying test fields, which has this format:
Category:Regression
Release:R5.1
Area Path:yourTeamProject\Service Provisioning
========================================================================
You can find details of test case fields by running command <tcmx testcase details>.");

			Console.WriteLine(
				"\nExample3: Bulk add test cases from excel files (and assign to static or requirement based suite)");
			Console.WriteLine("Command2: tcmx testcase bulkcreate2 /testcollection:testcollectionexcelfile /[suiteid:id]");

			Console.WriteLine("\nExample4: Get field details of a test case");
			Console.WriteLine(
				"Command4: tcmx testcase details /collection:https://tfs.your.tfs.url/tfs/DefaultCollection /teamproject:yourTeamProject /testcaseid:21808");
		}
	}
}
