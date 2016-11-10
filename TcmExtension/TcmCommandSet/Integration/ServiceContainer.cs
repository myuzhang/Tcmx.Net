using Autofac;

namespace TcmCommandSet.Integration
{
	/// <summary>
	/// Singleton services container.
	/// </summary>
	public sealed class ServicesContainer
	{
		public IContainer Container { get; private set; }

		/// <summary>
		/// Prevents a default instance of the <see cref="ServicesContainer"/> class from being created.
		/// </summary>
		public ServicesContainer(Option commandOption)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new ServicesModule()
			{
				CommandOption = commandOption
			});
			Container = containerBuilder.Build();
		}
	}
}
