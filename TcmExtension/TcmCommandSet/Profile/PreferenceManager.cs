using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TcmCommandSet.Profile
{
	public class PreferenceManager
	{
		private static PreferenceManager _instance;

		private readonly Preference _preference;

		private readonly string _preferenceFile;

		private PreferenceManager()
		{
			_preferenceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Profile\Preference.xml");
			_preference = Load();
		}

		public string Collection { get { return _preference.Collection; } }

		public string TeamProject { get { return _preference.TeamProject; } }

		public string UserName { get { return _preference.UserName; } }

		public string Password { get { return _preference.Password; } }

		public static PreferenceManager Instance
		{
			get { return _instance ?? (_instance = new PreferenceManager()); }
		}

		public void SetPreference(string collection, string teamProject, string user, string pass, string commandPath)
		{
			var preference = Instance.GetPreference();
			preference.Collection = collection ?? preference.Collection;
			preference.TeamProject = teamProject ?? preference.TeamProject;
			preference.UserName = user ?? preference.UserName;
			preference.Password = pass ?? preference.Password;
            preference.CommandPath = commandPath ?? preference.CommandPath;
            Instance.SetPreference(preference);
		}

		public void ShowPreference(string options)
		{
			if (string.IsNullOrWhiteSpace(options))
				return;
			
			foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(_preference))
			{
				string name = descriptor.Name;
				if (options.Equals("all", StringComparison.InvariantCultureIgnoreCase)
					|| options.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					object value = descriptor.GetValue(_preference);
					Console.WriteLine("{0}:{1}", name.PadRight(20), value);
				}
			}
		}

		public Preference GetPreference()
		{
			return _preference;
		}

        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void SetPreference(Preference preference)
		{
			Save(preference);
		}

		/// <summary>
		/// Loads this instance.
		/// </summary>
		/// <returns>
		/// The <see cref="Preference"/>.
		/// </returns>
		Preference Load()
		{
			Preference store;
			XmlSerializer serializer = new XmlSerializer(typeof(Preference));
			using (XmlReader reader = XmlReader.Create(_preferenceFile))
			{
				store = (Preference)serializer.Deserialize(reader);
			}
			return store;
		}

		/// <summary>
		/// Saves the specified data store.
		/// </summary>
		/// <param name="dataStore">
		/// The data store.
		/// </param>
		void Save(Preference dataStore)
		{
			// File.Delete(@"DataStore\PlatformDataStore.xml");

			// Create a new XmlSerializer instance with the type of the test class
			XmlSerializer serializerObj = new XmlSerializer(typeof(Preference));

			// Create a new file stream to write the serialized object to a file
			TextWriter writeFileStream = new StreamWriter(_preferenceFile);
			serializerObj.Serialize(writeFileStream, dataStore);

			// Cleanup
			writeFileStream.Close();
		}
	}
}
