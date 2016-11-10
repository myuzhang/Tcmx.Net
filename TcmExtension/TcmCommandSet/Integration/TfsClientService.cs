namespace TcmCommandSet.Integration
{
	public class TfsClientService
	{
		private static TfsClientService _instance = null;

		public static ServiceManagement ServiceManagement { get; set; }

		private TfsClientService()
		{
		}

		public static TfsClientService Instance
		{
			get { return _instance ?? (_instance = new TfsClientService()); }
		}

		public void LoadTfsClientService(Option option)
		{
			ServiceManagement = new ServiceManagement(option);
		}
	}
}
