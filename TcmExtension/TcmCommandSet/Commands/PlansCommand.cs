using ClusterCommandLine;
using ClusterCommandLine.CommandLineRoute;

namespace TcmCommandSet.Commands
{
	[CommandRoutePrefix("plans", @"/[collection:teamprojectcollectionurl]/[teamproject:project]/[login:username,[password]]",
		HelpText = "Provides operations to create and update test plans")]
	public class PlansCommand : Command
	{
		[CommandRoute("create", "/planname:name")]
		public void Create(Option option)
		{
			// ShowOption is for debug only
			ShowOption(option);
		}

		public override void ActionHelpHeader()
		{
		}

		public override void ActionHelpExample()
		{
		}
	}
}
