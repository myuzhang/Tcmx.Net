using System.Xml.Serialization;

namespace TcmCommandSet.Profile
{
	[XmlRoot("tcmxParameter")]
	public class Preference
	{
		[XmlElement("collection")]
		public string Collection { get; set; }

		[XmlElement("teamproject")]
		public string TeamProject { get; set; }

		[XmlElement("username")]
		public string UserName { get; set; }

		[XmlElement("password")]
		public string Password { get; set; }

        [XmlElement("commandpath")]
        public string CommandPath { get; set; }
    }
}
