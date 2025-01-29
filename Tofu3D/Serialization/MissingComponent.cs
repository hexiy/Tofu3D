using System.IO;

namespace Scripts;

public class MissingComponent : Component
{
    public string _oldComponentTypeName;
    public string _oldComponentXMLString;

    public void SetMissingComponentXML(string oldComponentTypeName, string xml)
    {
        _oldComponentTypeName = oldComponentTypeName;
        _oldComponentXMLString = xml;
    }

    public string GetXMLOfThisComponent()
    {
        using StringWriter stream = new StringWriter();
        XmlSerializer serializer = new XmlSerializer(typeof(MissingComponent), new[] { typeof(MissingComponent) });
        serializer.Serialize(stream, this);
        string xml = stream.ToString();
        int a = xml.IndexOf("<MissingComponent");

        xml = xml.Substring(xml.IndexOf("<MissingComponent"));

        // string aaaa =
        //     "<MissingComponent xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";
        // string bbbb = "<Component xsi:type=\"MissingComponent\">\n";  
        string aaaa =
            "<MissingComponent";
        string bbbb =
            "<Component xsi:type=\"MissingComponent\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"";
        // string bbbb = "<Component xsi:type=\"MissingComponent\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"";

        if (xml.Contains("xmlns:xsi") == false)
        {
            xml = xml.Replace(aaaa, bbbb);
        }
        else
        {
            bbbb = "<Component xsi:type=\"MissingComponent\"";
            xml = xml.Replace(aaaa, bbbb);
        }

        string x = "</MissingComponent>";
        string y = "</Component>";
        xml = xml.Replace(x, y);
        return xml;
    }
}