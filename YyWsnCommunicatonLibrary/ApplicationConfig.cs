using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace YyWsnCommunicatonLibrary
{
    /// <summary>
    /// TODO
    /// </summary>
    public class ApplicationConfig
    {
        public void OpenXMLFile(string FileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"..\..\Book.xml");
            XmlNode xn = doc.SelectSingleNode("bookstore");
            foreach (XmlNode xn1 in xn)
            {
                XmlElement xe = (XmlElement)xn1;
                //bookModel.BookISBN = xe.GetAttribute("ISBN").ToString();
            }
        }

    }
}
