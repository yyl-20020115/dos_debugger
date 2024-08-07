namespace Disassembler;

public static class XMLUtils
{
    public static string EscapeXml(this string xmlString)
    {
        xmlString = xmlString.Replace("&amp;", "&");
        xmlString = xmlString.Replace("&lt;", "<");
        xmlString = xmlString.Replace("&gt;", ">");
        xmlString = xmlString.Replace("&quot;", "\"");
        xmlString = xmlString.Replace("&apos;", "'");
        xmlString = xmlString.Replace("&#39;", "'");
        return xmlString;
    }
    public static string UnescapeXml(this string xmlString)
    {
        xmlString = xmlString.Replace("&", "&amp;");
        xmlString = xmlString.Replace("<", "&lt;");
        xmlString = xmlString.Replace(">", "&gt;");
        xmlString = xmlString.Replace("\"", "&quot;");
        xmlString = xmlString.Replace("'", "&apos;");
        xmlString = xmlString.Replace("'", "&#39;");
        return xmlString;
    }

}
