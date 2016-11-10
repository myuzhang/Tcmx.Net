using System;
using Autofac;
using Module = Autofac.Module;

namespace TcmCommandSet.Integration
{
	/// <summary>
	/// Services IOC modules.
	/// </summary>
	public class ServicesModule : Module
	{
		public Option CommandOption { get; set; }

		/// <summary>
		/// Override to add registrations to the container.
		/// </summary>
		/// <param name="builder">The builder through which components can be
		/// registered.</param>
		/// <exception cref="System.ArgumentNullException">Builder exception.
		/// </exception>
		/// <remarks>
		/// Note that the ContainerBuilder parameter is unique to this module.
		/// </remarks>
		protected override void Load(ContainerBuilder builder)
		{
			if (builder == null)
				throw new ArgumentNullException("builder");

			base.Load(builder);
			builder.Register(c => new TfsClient(CommandOption))
				.AsSelf()
				.InstancePerLifetimeScope();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            builder
				.RegisterAssemblyTypes(assemblies)
				.Where(t => t.IsSubclassOf(typeof(Manager)))
				.AsSelf();

			//builder.RegisterType<PlanManager>()
			//	.AsSelf()
			//	.InstancePerLifetimeScope();
			//builder.RegisterType<SuiteManager>()
			//	.AsSelf()
			//	.InstancePerLifetimeScope();
			//builder.RegisterType<TestCaseManager>()
			//	.AsSelf()
			//	.InstancePerLifetimeScope();
			//builder.RegisterType<ConfigManager>()
			//	.AsSelf()
			//	.InstancePerLifetimeScope();
		}
	}
}