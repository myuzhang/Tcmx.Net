using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using OfficeOpenXml;

namespace TcmCommandSet.Integration
{
	public class TestCaseManager : Manager
	{
		private readonly TfsClient _client;

		public TestCaseManager(TfsClient client)
		{
			_client = client;
		}

		public IEnumerable<ITestCase> AllTestCases
		{
			get
			{
				string fullQuery =
					String.Format(
						"SELECT * FROM WorkItems WHERE [System.WorkItemType] = 'Test Case' AND [Team Project] = '{0}'",
						_client.TeamProject.TeamProjectName
						);
				return _client.TeamProject.TestCases.Query(fullQuery);
			}
		}

		public IEnumerable<ITestCase> AllAutomatedTestCases
		{
			get
			{
				string fullQuery =
					String.Format(
						"SELECT [ID], [Title], [Automated test name] FROM WorkItems WHERE [Automation Status] = 'Automated' AND [Team Project] = '{0}'",
						_client.TeamProject.TeamProjectName
						);
				return _client.TeamProject.TestCases.Query(fullQuery);
			}
		}

		public ITestCase GeTestCaseByTitle(string title)
		{
			return
				AllTestCases
					.FirstOrDefault(t => t.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase));
		}

		public void ListTestCasesInSuite(ITestSuiteBase suite)
		{
			foreach (ITestCase testCase in suite.AllTestCases)
			{
				Console.WriteLine(testCase.Title);
			}
		}

		public void CreateTestCases(string testCollectionFile, string testFieldsFile, ITestSuiteBase suite = null,
			string configIds = null)
		{
			var fields = FileHelper.GetTestFieldFromFile(testFieldsFile);
			var tests = FileHelper.GetTestCaseFromFile(testCollectionFile);
			IList<ITestCase> testCases = null;

			if (suite != null)
				testCases = new List<ITestCase>();

			// reference: http://automatetheplanet.com/manage-tfs-test-cases-csharp-code/
			foreach (var test in tests)
			{
				ITestCase tc = _client.TeamProject.TestCases.Create();
				tc.Title = test.Key;

				// reference: http://www.ewaldhofman.nl/post/2009/12/11/TFS-SDK-2010-e28093-Part-5-e28093-Create-a-new-Test-Case-work-item.aspx
				foreach (var step in test.Value)
				{
					var tcs = tc.CreateTestStep();
					string[] stepAndExpectedResult = step.Split('#');
					tcs.Title = stepAndExpectedResult[0];
					if (stepAndExpectedResult.Count() > 1)
					{
						StringBuilder builder = new StringBuilder();
						for (int i = 1; i < stepAndExpectedResult.Count(); i++)
							builder.Append(stepAndExpectedResult[i]);
						tcs.ExpectedResult = builder.ToString();
					}
					tc.Actions.Add(tcs);
				}

				foreach (var field in fields)
					tc.CustomFields[field.Key].Value = field.Value;

				tc.Save();

				if (suite != null)
					testCases.Add(tc);
			}
			Console.WriteLine($"\nTotal test cases created: {tests.Count} ");

			if (suite != null)
			{
				AddTestCasesToSuite(testCases, suite);
				if (!string.IsNullOrWhiteSpace(configIds))
				{
					TfsService.SuiteManager.UpdateConfigToSuites(suite.Id.ToString(), configIds, "all", "false", null,
						false);
				}
			}
			else
			{
				if (string.IsNullOrWhiteSpace(configIds))
				{
					Console.WriteLine("Test configuration can't be assigned to test cases without test suite!");
				}
			}
		}

		public void CreateTestCases(string testCollectionFile, ITestSuiteBase suite = null)
		{
			IList<ITestCase> testCases = null;
			int testCaseCreated = 0;

			if (suite != null)
				testCases = new List<ITestCase>();

			// Use CSV file, please refer to: http://www.codeproject.com/Articles/9258/A-Fast-CSV-Reader
			//using (CsvReader csv = new CsvReader(new StreamReader(testCollectionFile), true))
			//{
			//    int fieldCount = csv.FieldCount;

			//    string[] headers = csv.GetFieldHeaders();
			//    while (csv.ReadNextRecord())
			//    {
			//        for (int i = 0; i < fieldCount; i++)
			//            Console.Write($"{headers[i]} = {csv[i]};");
			//        Console.WriteLine();
			//    }
			//}

			// Use Excel file, please refer to: https://codealoc.wordpress.com/2012/04/19/reading-an-excel-xlsx-file-from-c/
			var package = new ExcelPackage(new FileInfo(testCollectionFile));
			ExcelWorksheet workSheet = package.Workbook.Worksheets.FirstOrDefault();
			if (workSheet != null)
			{
				if (!workSheet.Cells["A1"].Text.Equals("Title", StringComparison.InvariantCultureIgnoreCase) ||
				    !workSheet.Cells["B1"].Text.Equals("Steps", StringComparison.InvariantCultureIgnoreCase) ||
				    !workSheet.Cells["C1"].Text.Equals("Expected Result", StringComparison.InvariantCultureIgnoreCase))
				{
					Console.WriteLine("The excel file header should be [Title], [Steps], [Expected Result] in order");
					return;
				}
				if (string.IsNullOrWhiteSpace(workSheet.Cells["A2"].Text))
				{
					Console.WriteLine("The first test case doesn't have name");
					return;
				}
				int colNum = workSheet.Dimension.Columns;
				int rowNum = workSheet.Dimension.Rows;
				string[] headers = new string[colNum + 1]; // excel starts from 1 instead of 0
				// fill headers:
				for (int h = 1; h <= colNum; h++)
				{
					headers[h] = workSheet.Cells[1, h].Text;
				}
				// add test cases:
				for (int r = 2; r <= rowNum; r++)
				{
					string title = workSheet.Cells[r, 1].Text;
					ITestCase tc = _client.TeamProject.TestCases.Create();
					var owner = tc.Owner;
					tc.Title = title;

					// add fields:
					for (int c = 4; c <= colNum; c++)
					{
						if (!string.IsNullOrWhiteSpace(headers[c]) &&
						    !string.IsNullOrWhiteSpace(workSheet.Cells[r, c].Text))
						{
							tc.CustomFields[headers[c]].Value = workSheet.Cells[r, c].Text;
						}
					}

					// add the first step and result:
					if (!string.IsNullOrWhiteSpace(workSheet.Cells[r, 2].Text) ||
					    !string.IsNullOrWhiteSpace(workSheet.Cells[r, 3].Text))
					{
						var tcs = tc.CreateTestStep();
						tcs.Title = workSheet.Cells[r, 2].Text;
						tcs.ExpectedResult = workSheet.Cells[r, 3].Text;
						tc.Actions.Add(tcs);
					}

					// add more steps and results:
					while (string.IsNullOrWhiteSpace(workSheet.Cells[r + 1, 1].Text) ||
					       workSheet.Cells[r + 1, 1].Text.Equals(title, StringComparison.InvariantCultureIgnoreCase))
					{
						r++;
						if (r > rowNum) break;
						if (!string.IsNullOrWhiteSpace(workSheet.Cells[r, 2].Text) ||
						    !string.IsNullOrWhiteSpace(workSheet.Cells[r, 3].Text))
						{
							var tcs = tc.CreateTestStep();
							tcs.Title = workSheet.Cells[r, 2].Text;
							tcs.ExpectedResult = workSheet.Cells[r, 3].Text;
							tc.Actions.Add(tcs);
						}
					}

					try
					{
						if (tc.OwnerTeamFoundationId == Guid.Empty)
						{
							Console.WriteLine(
								$"The AssignTo/Owner {tc.OwnerName} of the test case is not found in TFS, revert back to {owner.DisplayName}");
							tc.Owner = owner;
						}
						tc.Save();
						testCaseCreated++;
					}
					catch (TestManagementValidationException e)
					{
						Console.WriteLine($"Test Case Add Failed: {title}");
						Console.WriteLine($"Test case added failed probably due to invliad fields: {e.Message}");
					}

					if (suite != null)
						testCases.Add(tc);
				}
				Console.WriteLine($"\nTotal test cases created: {testCaseCreated} ");

				if (suite != null)
					AddTestCasesToSuite(testCases, suite);
			}
		}

		public void CloneTestCases(ITestSuiteBase suite, ITestSuiteBase destinationSuite)
		{
			ITestCaseCollection testCases = suite.AllTestCases;
			AddTestCasesToSuite(testCases, destinationSuite);
		}

		public void AddTestCasesToSuite(IEnumerable<ITestCase> testCases, ITestSuiteBase destinationSuite)
		{
			if (destinationSuite.TestSuiteType == TestSuiteType.StaticTestSuite)
			{
				IStaticTestSuite staticTestSuite = destinationSuite as IStaticTestSuite;
				if (staticTestSuite != null) staticTestSuite.Entries.AddCases(testCases);
			}
			else if (destinationSuite.TestSuiteType == TestSuiteType.RequirementTestSuite)
			{
				IRequirementTestSuite requirementTestSuite = destinationSuite as IRequirementTestSuite;
				if (requirementTestSuite != null)
				{
					WorkItemStore store = requirementTestSuite.Project.WitProject.Store;
					WorkItem tfsRequirement = store.GetWorkItem(requirementTestSuite.RequirementId);
					foreach (ITestCase testCase in testCases)
					{
						tfsRequirement.Links.Add(new RelatedLink(store.WorkItemLinkTypes.LinkTypeEnds["Tested By"],
							testCase.WorkItem.Id));
					}
					tfsRequirement.Save();
				}
			}
			destinationSuite.Plan.Save();
		}

		public void ImportTestCases(Option option)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("testcase /import");
			builder.AppendFormat(" /collection:{0}", option.Collection);
			builder.AppendFormat(" /teamproject:{0}", option.TeamProject);
			builder.AppendFormat(" /storage:{0}", option.Storage);

			if (!string.IsNullOrWhiteSpace(option.MaxPriority))
				builder.AppendFormat(" /maxpriority:{0}", option.MaxPriority);
			if (!string.IsNullOrWhiteSpace(option.MinPriority))
				builder.AppendFormat(" /minpriority:{0}", option.MinPriority);
			if (!string.IsNullOrWhiteSpace(option.Category))
				builder.AppendFormat(" /category:{0}", option.Category);
			if (!string.IsNullOrWhiteSpace(option.SyncSuite))
				builder.AppendFormat(" /syncsuite:{0}", option.SyncSuite);

			Process process = new Process();
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				// WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "tcm.exe",
				Arguments = builder.ToString()
			};
			// Console.WriteLine("TCM.exe {0}", startInfo.Arguments);
			process.StartInfo = startInfo;
			try
			{
				// Start the process with the info we specified.
				// Call WaitForExit and then the using statement will close.
				using (Process exeProcess = Process.Start(startInfo))
				{
					if (exeProcess != null) exeProcess.WaitForExit();
				}
			}
			catch
			{
				Console.WriteLine("TCM execution failed with parameter: {0}", builder);
			}
		}

		public void GetTestCaseDetails(string testCaseId)
		{
			ITestCase testCase = AllTestCases.SingleOrDefault(t => t.Id.Equals(Int32.Parse(testCaseId)));
			if (testCase != null)
			{
				Console.WriteLine("Test Case title \"{0}\" with Id:{1} has below field details:", testCase.Title,
					testCase.Id);
				Console.WriteLine("Field Name:Field Value");
				for (int i = 0; i < testCase.CustomFields.Count; i++)
				{
					Console.WriteLine("{0}:{1}", testCase.CustomFields[i].Name, testCase.CustomFields[i].Value);
				}
			}
			else
			{
				Console.WriteLine("Test Case with Id:{0} not found.", testCaseId);
			}
		}

		public void AddCustomFieldsToTestCase(string testFieldsFile, ITestCase testCase)
		{
			var fields = FileHelper.GetTestFieldFromFile(testFieldsFile);
			foreach (var field in fields)
			{
				if (testCase.CustomFields.Contains(field.Key))
				{
					testCase.CustomFields[field.Key].Value = field.Value;
				}
				else
				{
					Console.WriteLine("The field [{0}] is not found and set for test case: {1}", field.Key,
						testCase.Title);
				}
			}
			testCase.Save();
		}

		/// <summary>
		/// Bulk update single filed for test cases within a <see cref="ITestSuiteBase"/>.
		/// </summary>
		/// <param name="fieldName">
		/// The filed to be updated
		/// </param>
		/// <param name="value">
		/// The new valude for the field.
		/// </param>
		/// <param name="suite">
		/// The <see cref="ITestSuiteBase"/> to update test cases for.
		/// </param>
		/// <param name="includeSubSuite">
		/// When true, also update the sub suites.
		/// </param>
		public void BulkUpdateFiled(string fieldName, string value, ITestSuiteBase suite, bool includeSubSuite = true)
		{
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			if (suite == null)
				throw new ArgumentNullException(nameof(suite));

			foreach (ITestCase testCase in suite.AllTestCases)
			{
				if (testCase.CustomFields.Contains(fieldName))
				{
					testCase.WorkItem.Open();
					testCase.CustomFields[fieldName].Value = value;
					testCase.WorkItem.Save(SaveFlags.MergeAll);
				}
				else
				{
				    throw new ArgumentException($"{fieldName} is not a valid field.", nameof(fieldName));
				}
			}
			if (!includeSubSuite)
				return;

			var staticSuite = suite as IStaticTestSuite;
			if (staticSuite == null)
				return;
			foreach (ITestSuiteBase subSuite in staticSuite.SubSuites)
			{
				this.BulkUpdateFiled(fieldName, value, subSuite);
			}
		}

		public void AddCustomFieldsToTestCases(string testFieldsFile, IList<ITestCase> testCases)
		{
			foreach (ITestCase testCase in testCases)
				AddCustomFieldsToTestCase(testFieldsFile, testCase);
		}

		public IEnumerable<ITestSuiteEntry> MatchedTestSuiteEntries(
			string testFieldsFile,
			IEnumerable<ITestSuiteEntry> entries
			)
		{
			if (entries == null && !entries.Any())
			{
				return null;
			}
			if (string.IsNullOrWhiteSpace(testFieldsFile))
			{
				return entries;
			}
			var fields = FileHelper.GetTestFieldFromFile(testFieldsFile);
			if (fields.Count.Equals(0))
			{
				return entries;
			}
			List<ITestSuiteEntry> matchedTestSuiteEntries = new List<ITestSuiteEntry>();
			Boolean isMatched = false;
			foreach (ITestSuiteEntry entry in entries)
			{
				foreach (var field in fields)
				{
					if (!entry.TestCase.CustomFields.Contains(field.Key))
					{
						isMatched = false;
						break;
					}
					else
					{
						if (!entry.TestCase.CustomFields[field.Key].Value.ToString().ToLower()
							.Contains(field.Value.ToLower()))
						{
							isMatched = false;
							break;
						}
					}
					isMatched = true;
				}
				if (isMatched)
				{
					matchedTestSuiteEntries.Add(entry);
				}
			}
			return matchedTestSuiteEntries;
		}
	}
}