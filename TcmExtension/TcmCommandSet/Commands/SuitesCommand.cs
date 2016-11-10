using System;
using System.Collections.Generic;
using ClusterCommandLine;
using ClusterCommandLine.CommandLineRoute;
using Microsoft.TeamFoundation.TestManagement.Client;
using TcmCommandSet.Integration;

namespace TcmCommandSet.Commands
{
    [CommandRoutePrefix("suites",
        @"/[collection:teamprojectcollectionurl]/[teamproject:project]/[login:username,[password]]",
        HelpText = "Provides operations to create and update test suites")]
    public class SuitesCommand : Command
    {
        [CommandRoute("list", "/planname:name")]
        public void List(Option option)
        {
            const int pad = 12;
            IEnumerable<ITestSuiteBase> suites = TfsService.SuiteManager.GetSuites(option.PlanName);
            Console.WriteLine("{0}Suite Name", "Suite ID".PadRight(pad));
            for (int i = 1; i < pad; i++)
                Console.Write("-");
            Console.WriteLine(" ----------------------------------------------------------------");
            foreach (ITestSuiteBase testSuiteBase in suites)
                Console.WriteLine("{0}{1}", testSuiteBase.Id.ToString().PadRight(pad), testSuiteBase.Title);
        }

        [CommandRoute("create",
            "/planname:name/suitename:parentsuitename/newsuitename:childsuitename/newsuitedescription:description/suitetype:static|dynamic|requirement"
            )]
        public void Create(Option option)
        {
            // ShowOption is for debug only
            ShowOption(option);

            TfsService.SuiteManager
                .CreateSuite(
                    option.PlanName,
                    option.SuiteName,
                    option.NewSuiteName,
                    option.NewSuiteDescription,
                    option.SuiteType);
        }

        [CommandRoute("getquery", "/suiteid:id")]
        public void GetQuery(Option option)
        {
            var query = TfsService.SuiteManager.GetSuiteQuery(option.SuiteId);
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new TestObjectNotFoundException("Query string not found");
            }
            Console.WriteLine(query);
        }

        [CommandRoute("appendquery", "/suiteid:id/query:querycondition/[recursive:false|true]")]
        public void AppendQuery(Option option)
        {
            TfsService.SuiteManager.AppendQueryConditions(option.SuiteId, option.Query, option.Recursive ?? "false");
        }

        [CommandRoute("replacequery",
            "/suiteid:id/replaced:replacedquery/[replacing:replacingquery]/[recursive:false|true]")]
        public void ReplaceQuery(Option option)
        {
            TfsService.SuiteManager.ReplaceQueryConditions(option.SuiteId, option.Replaced, option.Replacing, option.Recursive ?? "false");
        }

        //[CommandRoute("GetQueryConditions", "/planname:name/suitename:name")]
        public void GetQueryConditions(Option option)
        {
            var query = TfsService.SuiteManager.GetSuiteQuery(option.PlanName, option.SuiteName);
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new TestObjectNotFoundException("Query string not found");
            }
            Console.WriteLine(query);
        }

        //[CommandRoute("AddQueryConditions", "/planname:name/[suitename:name]/queryconditions:querystring")]
        public void AddQueryConditions(Option option)
        {
            var plan = TfsService.PlanManager.GetTestPlanByName(option.PlanName);
            if (plan == null)
            {
                throw new TestObjectNotFoundException(string.Format("Test Plan - {0} not found", option.PlanName));
            }
            TfsService.SuiteManager.AddQueryConditions(plan, option.SuiteName, option.QueryConditions);
        }

        //[CommandRoute("AddQueryCondition",
        //    "/planname:name/[suitename:name]/andor:and|or/field:field/operator:operator/value:yourvalue")]
        public void AddQueryCondition(Option option)
        {
            string newCondition = option.AndOr + " " + option.Field + " " + option.Operator + " " + option.Value;
            var plan = TfsService.PlanManager.GetTestPlanByName(option.PlanName);
            if (plan == null)
            {
                throw new TestObjectNotFoundException(string.Format("Test Plan - {0} not found", option.PlanName));
            }
            TfsService.SuiteManager.AddQueryConditions(plan, option.SuiteName, newCondition);
        }

        //[CommandRoute("UpdateQueryCondition",
        //    "/planname:name/[suitename:name]/replaced:replacedquery/[replacing:replacingquery]")]
        public void UpdateQueryCondition(Option option)
        {
            var plan = TfsService.PlanManager.GetTestPlanByName(option.PlanName);
            if (plan == null)
            {
                throw new TestObjectNotFoundException(string.Format("Test Plan - {0} not found", option.PlanName));
            }
            TfsService.SuiteManager.UpdateQueryConditions(plan, option.SuiteName, option.Replaced, option.Replacing);
        }

        [CommandRoute("appendconfig",
            "/suiteid:suiteid/configid:configurationid1+configurationid2/status:all|automated|nonauto/[recursive:false|true]/[testfields:testcasefieldsconditionfile]"
            )]
        public void AppendConfig(Option option)
        {
            TfsService.SuiteManager.UpdateConfigToSuites(option.SuiteId, option.ConfigId, option.Status,
                option.Recursive, option.TestFields, false);
        }

        [CommandRoute("replaceconfig",
            "/suiteid:suiteid/configid:configurationid1+configurationid2/status:all|automated|nonauto/[recursive:false|true]/[testfields:testcasefieldsconditionfile]"
            )]
        public void ReplaceConfig(Option option)
        {
            TfsService.SuiteManager.UpdateConfigToSuites(option.SuiteId, option.ConfigId, option.Status,
                option.Recursive, option.TestFields, true);
        }

        public override void ActionHelpHeader()
        {
            Console.WriteLine("This command set is for operations of suites.\n");
        }

        public override void ActionHelpExample()
        {
            Console.WriteLine("Example1: Get query string from query based test suite");
            Console.WriteLine(
                "Command1: tcmx suites getquery /collection:https://tfs.your.tfs.url/tfs/DefaultCollection /teamproject:CloudBackup /suitename:\"release testing - manual\" /planname:\"Release 3.5 - Gladiator\"");
            Console.WriteLine("\nExample2: Add query conditions to query based test suite");
            Console.WriteLine(
                "Command2: tcmx suites addqueryconditions /collection:https://tfs.your.tfs.url/tfs/DefaultCollection /teamproject:CloudBackup /suitename:\"release testing - manual\" /planname:\"Release 3.5 - Gladiator\" /queryconditions:\"and [DD.Test.Release] < 'R2'\"");
        }
    }
}
