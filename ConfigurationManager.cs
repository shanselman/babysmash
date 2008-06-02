using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;

namespace IsolatedStorage
{
    /// <summary>
    /// IsolatedStorageConfigurationManager
    /// ===================================
    /// Read and Write application and formsettings to isolated storage
    /// Setting are saved as XML in a file named <application-name>.config 
    /// in folder C:\Documents and Settings\<user>\Local Settings\Application Data\IsolatedStorage\<...>\<...>\<...>\AssemFiles\
    ///
    /// Class is implemented as a Singleton.
    /// Example for use: 
    /// IsolatedStorageConfigurationManager configManager = IsolatedStorageConfigurationManager.ConfigurationManager(Application.ProductName)
    /// string databaseName = configManager.Read("Database")
    /// configManager.Write("Database", DatabaseName)
    /// configManager.Persist()
    ///
    /// Edwin Roetman, January 2004
    /// </summary>
    public sealed class ConfigurationManager
    {
        #region Singleton

        //The ConfigurationManager singleton instance
        private static ConfigurationManager _singleton;

        //The ConfigurationManager singleton instance
        public static ConfigurationManager GetConfigurationManager(string applicationName)
        {
            if (_singleton == null)
            {
                _singleton = new ConfigurationManager(applicationName);
            }

            return _singleton;
        }

        /// <summary>
        /// Singleton, do not allow this class to be instantiated by making the contructor private
        /// </summary>
        /// <param name="applicationName"></param>
        private ConfigurationManager(string applicationName)
        {
            this.InitializeConfiguration(applicationName);
        }

        #endregion

        #region Private members
        private XmlDocument _xml;
        private XmlDocument _xmlOriginal;
        private string _fileName;
        private IsolatedStorageFile _isoStore;
        #endregion

        #region public implementation

        public string Read(string section)
        {
            return Read(section, String.Empty);
        }

        public string Read(string section, string defaultValue)
        {
            try
            {
                if (this._xml == null)
                {
                    //return String.Empty;
                    return defaultValue;
                }
                //Select the item node(s)
                XmlNode sectionNode = this._xml.SelectSingleNode((@"/configuration/" + section));
                if (sectionNode == null)
                {
                    return defaultValue;
                }
                return sectionNode.FirstChild.Value;
            }
            catch
            {
                return defaultValue;
            }
        }

        public int ReadInteger(string section)
        {
            return ReadInteger(section, 0);
        }

        public int ReadInteger(string section, int defaultValue)
        {
            string valuestring = Read(section);
            if (valuestring.Length <= 0)
            {
                return defaultValue;
            }
            try
            {
                int value = Convert.ToInt32(valuestring);
                return value;
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool ReadBoolean(string section)
        {
            return ReadBoolean(section, false);
        }

        public bool ReadBoolean(string section, bool defaultValue)
        {
            string value = this.Read(section);
            if (value.Length <= 0)
            {
                return defaultValue;
            }
            try
            {
                return Boolean.Parse(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public void Write(string section, string value)
        {
            try
            {
                if (this._xml == null)
                {
                    this._xml = new XmlDocument();
                    XmlNode configurationRootNode = this._xml.CreateElement(@"configuration");
                    this._xml.AppendChild(configurationRootNode);
                }

                //Select the item node(s)
                XmlNode sectionNode = this._xml.SelectSingleNode((@"/configuration/" + section));
                if (sectionNode == null)
                {
                    //if the node does not exist create it
                    sectionNode = this._xml.CreateElement(section);
                    XmlNode configurationRootNode = this._xml.SelectSingleNode(@"/configuration");
                    configurationRootNode.AppendChild(sectionNode);
                }
                sectionNode.InnerText = value;
            }
            catch { }
        }

        ///Read form state, size and position
        public void ReadFormSettings(System.Windows.Forms.Form form)
        {
            string windowStateString = this.Read(form.Name + "WindowState");
            System.Windows.Forms.FormWindowState windowState = System.Windows.Forms.FormWindowState.Normal;
            if (windowStateString.Length > 0)
            {
                windowState = (System.Windows.Forms.FormWindowState)Convert.ToInt32(windowStateString);
            }

            if (windowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                form.WindowState = windowState;
            }
            else
            {
                string valuesString = this.Read(form.Name);
                if (valuesString.Length > 0)
                {
                    string[] values = valuesString.Split(Convert.ToChar(","));
                    form.Top = Convert.ToInt16(values[0]);
                    form.Left = Convert.ToInt16(values[1]);
                    int width = Convert.ToInt16(values[2]);
                    if (width > 0) form.Width = width;
                    int height = Convert.ToInt16(values[3]);
                    if (height > 0) form.Height = height;
                }
            }
        }

        ///Write form state, size and position
        public void WriteFormSettings(System.Windows.Forms.Form form)
        {
            this.Write(form.Name + "WindowState", ((int)form.WindowState).ToString());

            //Me.Write(form.Name & "WindowState", (CType(form.WindowState, Integer)).ToString())

            if (form.WindowState == System.Windows.Forms.FormWindowState.Normal)
            {
                string valuesstring = form.Top.ToString() + "," + form.Left.ToString() + "," + form.Width.ToString() + "," + form.Height.ToString();
                this.Write(form.Name, valuesstring);
            }
        }

        public void Persist()
        {
            try
            {
                this.WriteBackConfiguration();
            }
            catch
            {
            }
            finally
            {
                _singleton = null;
            }
        }

        #endregion

        #region private methods and functions

        private void InitializeConfiguration(string applicationName)
        {
            this._fileName = applicationName + ".config";
            this._isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            //Check to see if the settings file exists, if so load xml from it
            string[] storeFileNames;
            storeFileNames = this._isoStore.GetFileNames(this._fileName);

            foreach (string storeFile in storeFileNames)
            {
                if (storeFile == this._fileName)
                {
                    //Create isoStorage StreamReader
                    StreamReader streamReader = new StreamReader(new IsolatedStorageFileStream(this._fileName, FileMode.Open, this._isoStore));
                    this._xml = new XmlDocument();
                    this._xml.Load(streamReader);
                    this._xmlOriginal = new XmlDocument();
                    this._xmlOriginal.LoadXml(this._xml.OuterXml);
                    streamReader.Close();
                }
            }
        }

        private void WriteBackConfiguration()
        {
            //if no config information is present write null
            if (this._xml == null) return;

            //if config is unchanged write null
            if (this._xmlOriginal != null)
            {
                if (this._xml.OuterXml == this._xmlOriginal.OuterXml) return;
            }

            //Save the document
            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(new IsolatedStorageFileStream(this._fileName, FileMode.Create, this._isoStore));
                this._xml.Save(streamWriter);
                streamWriter.Flush();
                streamWriter.Close();

                if (this._xmlOriginal == null) this._xmlOriginal = new XmlDocument();
                this._xmlOriginal.LoadXml(this._xml.OuterXml);
            }
            catch
            {
                //throw;
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
        }

        #endregion
    }
}
