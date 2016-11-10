using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TcmCommandSet;
using System.Linq;

namespace Tcmx
{
	public static class TcmOptionHelper
	{
		public static Option ToTcmOption(this string[] args)
		{
			StringBuilder builder = new StringBuilder();
			foreach (var s in args)
			{
				string pattern = @"/(\w+):";
				if (s.StartsWith("/"))
				{
					try
					{
						string opt = Regex.Matches(s, pattern, RegexOptions.IgnoreCase)[0].Groups[1].Value;
						string value = s.Substring(opt.Length + 2);
						string pair = string.Format("\"{0}\":\"{1}\",", opt, value);
						builder.Append(pair);
					}
					catch (ArgumentOutOfRangeException)
					{
						throw new ArgumentException();
					}
				}
			}
			var options = builder.ToString();

			if (options.EndsWith(","))
			{
				options = options.Remove(options.Length - 1);
			}

            options = options.Replace(@"\", @"\\");

			string optionJson = string.Format("{{ {0} }}", options);
			Option option = JsonConvert.DeserializeObject<Option>(optionJson);
			if (!string.IsNullOrEmpty(option.Login))
			{
				option.UserName = option.Login.ToUserName();
				option.Password = option.Login.ToPassword();
			}

			return option;
		}

        public static string ToUserName(this string login)
        {
            var credential = login.Split(',');
            return credential[0];
        }

        public static string ToPassword(this string login)
        {
            var credential = login.Split(',');
            if (credential.Count() > 1)
            {
                return credential[1];
            }
            return null;
        }
    }
}
