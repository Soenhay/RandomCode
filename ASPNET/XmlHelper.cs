using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;

namespace Common
{
    public static class XmlHelper
    {
        ///// <summary>
        ///// Need .net 4.0
        ///// http://stackoverflow.com/questions/1123718/format-xml-string-to-print-friendly-xml-string
        ///// </summary>
        ///// <param name="Xml"></param>
        ///// <returns></returns>
        //public static string PrettyXml(String Xml)
        //{
        //    try
        //    {
        //        XDocument doc = XDocument.Parse(Xml);
        //        return doc.ToString();
        //    }
        //    catch (Exception)
        //    {
        //        return Xml;
        //    }
        //}

        /// <summary>
        /// http://stackoverflow.com/questions/1123718/format-xml-string-to-print-friendly-xml-string
        /// </summary>
        /// <param name="XML"></param>
        /// <returns></returns>
        public static String PrettyXml(string strXml)
        {
            String Result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(strXml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                String FormattedXML = sReader.ReadToEnd();

                Result = FormattedXML;
            }
            catch (XmlException ex)
            {
                Result = "[ERROR] " + ex.Message;
            }

            writer.Close();
            mStream.Close();//Can't close memory stream first b/c it will also dispose of the writer.

            return Result;
        }

        /// <summary>
        /// Try to parse an XML string into a DataSet. If it fails or the string is empty then an empty DataSet will be returned.
        /// </summary>
        /// <param name="strSource"></param>
        /// <returns></returns>
        public static DataSet ParseXmlToDataSet(string strXml)
        {
            DataSet ds = new DataSet();

            if (!String.IsNullOrEmpty(strXml))
            {
                try
                {
                    ds.Clear();
                    ds.ReadXml(new System.IO.StringReader(strXml));
                }
                catch //(Exception ex)
                {
                    ds = new DataSet();
                }
            }

            return ds;
        }
    }
}
