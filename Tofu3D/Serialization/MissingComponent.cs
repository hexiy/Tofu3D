using System.IO;

namespace Tofu3D;

public class MissingComponent : Component
{
    public string _missingComponentXML;

    public void SetMissingComponentXML(string xml)
    {
        _missingComponentXML = xml;
    }
    public string GetXMLOfThisComponent()
    {
        using var stream = new StringWriter();
        var serializer = new XmlSerializer(typeof(MissingComponent), new Type[]{typeof(MissingComponent)});
        serializer.Serialize(stream, this);
        string xml = stream.ToString();
        var a = xml.IndexOf("<MissingComponent");

        xml = xml.Substring(xml.IndexOf("<MissingComponent"));

        string aaaa =
            "<Component xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xsi:type=\"MissingComponent\">";
        string bbbb = "<Component xsi:type=\"MissingComponent\">\n";
        
        xml = xml.Replace(aaaa, bbbb);
        return xml;
    }
}