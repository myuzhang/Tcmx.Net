using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace TcmCommandSet
{
	public static class FileHelper
	{
		public static Dictionary<string, string> GetTestFieldFromFile(string file)
		{
			System.IO.StreamReader collection = null;
			try
			{
				var fields = new Dictionary<string, string>();
				collection = new System.IO.StreamReader(file);
				string line;
				while ((line = collection.ReadLine()) != null)
				{
					var words = line.Split(':');
					fields.Add(words[0], words[1]);
				}
				return fields;
			}
			catch (Exception)
			{
				throw new TestObjectNotFoundException(string.Format("{0} is not found or format is not correct", file));
			}
			finally
			{
				if (collection != null) collection.Close();
			}
		}

		public static Dictionary<string, IList<string>> GetTestCaseFromFile(string file)
		{
			System.IO.StreamReader collection = null;
			string currentTest = null;
			try
			{
				var tests = new Dictionary<string, IList<string>>();
				string test = null;
				List<string> actions = null;
				collection = new System.IO.StreamReader(file);
				string line;
				while ((line = collection.ReadLine()) != null)
				{
					if (string.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					if (line.StartsWith("-"))
					{
						if (actions != null)
							actions.Add(line.Substring(1));
					}
					else
					{
						if (test != null)
						{
							currentTest = test;
							tests.Add(test, actions);
						}
						test = line;
						actions = new List<string>();
					}
				}
				if (test != null)
				{
					currentTest = test;
					tests.Add(test, actions);
				}
				return tests;
			}
			catch (Exception e)
			{
				if (e.Message.Contains("An item with the same key has already been added"))
					throw new InvalidOperationException(string.Format("Duplicated Test Case Title: {0}", currentTest));

				throw new TestObjectNotFoundException(string.Format("{0} is not found or format is not correct", file));
			}
			finally
			{
				if (collection != null) collection.Close();
			}
		}
	}
}
