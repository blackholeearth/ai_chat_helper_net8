//using System;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;
using ReverseMarkdown.Converters;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace ai_chat_helper_net8;

// 2025.04 - handles <table>
//           NOTE: "currently it call innerText of TableCells" // so it strips out images and urls...
//                   if you want the links use older implemementation at the link below.
// 2025.04 - outputs <img> <a>, as markdown 
//small but important modification to class https://github.com/zzzprojects/html-agility-pack/blob/master/src/Samples/Html2Txt/HtmlConvert.cs
public static class HtmlToText
{

    public static Func<string,string> process_img_url;
    public static Func<string, string> process_a_href;

    static HtmlToText()
    {
        var func = (string input) =>
        {
            // expected url:  https://static.wikia.nocookie.net/omniheroes-game/images/b/b5/Avengers.png/revision/latest?cb=20230517095152
            var match = Regex.Match(input, @"/([^/]{1,}?\.(png|jpg|webp|gif|bmp))/");
            input = match.Groups[1].Value.ToString();
            return input;
        };

        process_img_url = func;
        process_a_href = func;
    }

    //public static string ConvertHtmlFile_toText(string FilePath)
    //{
    //    HtmlDocument doc = new HtmlDocument();
    //    doc.Load(FilePath);
    //    return ConvertDoc(doc);
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="html"></param>
    /// <param name="CssSelector_forRoot"> ".page" --> class='page' for example.  ***selects first occurance of the Match. </param>
    /// <returns></returns>
    public static string ConvertHtml_toText(string html ,string CssSelector_forRoot = "")
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);
        return ConvertDoc(doc,CssSelector_forRoot);
    }


    public static string ConvertDoc(HtmlDocument doc, string CssSelector_forRoot = "")
    {
        using (StringWriter sw = new StringWriter())
        {

            if (CssSelector_forRoot != null)
                ConvertTo(doc.QuerySelector(CssSelector_forRoot), sw);
            else
                ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            
            var result = sw.ToString();
            //return result;

            //reduce empty line count.
            result = Regex.Replace(result, "((\r\n)|(\n)){3,}", "\r\n\r\n", RegexOptions.Multiline);
            
            return result;
        }
    }

    public static void ConvertTo(HtmlNode node, TextWriter outText)
    {
        var dominfo =  new PreceedingDomTextInfo(false);



        ConvertTo(node, outText, dominfo) ;
    }

    internal static void ConvertContentTo(HtmlNode node, TextWriter outText, PreceedingDomTextInfo textInfo)
    {
        foreach (HtmlNode subnode in node.ChildNodes)
        {
            ConvertTo(subnode, outText, textInfo);
        }
    }
 
    internal static void ConvertTo(HtmlNode node, TextWriter outText, PreceedingDomTextInfo textInfo)
    {
        string html;
        switch (node.NodeType)
        {
            case HtmlNodeType.Comment:
                // don't output comments
                break;
            case HtmlNodeType.Document:
                ConvertContentTo(node, outText, textInfo);
                break;
            case HtmlNodeType.Text:
                // script and style must not be output
                string parentName = node.ParentNode.Name;
                if ((parentName == "script") || (parentName == "style"))
                {
                    break;
                }
                // get text
                html = ((HtmlTextNode)node).Text;
                // is it in fact a special closing node output as text?
                if (HtmlNode.IsOverlappedClosingElement(html))
                {
                    break;
                }
                // check the text is meaningful and not a bunch of whitespaces
                if (html.Length == 0)
                {
                    break;
                }
                if (!textInfo.WritePrecedingWhiteSpace || textInfo.LastCharWasSpace)
                {
                    html = html.TrimStart();
                    if (html.Length == 0) { break; }
                    textInfo.IsFirstTextOfDocWritten.Value = textInfo.WritePrecedingWhiteSpace = true;
                }
                outText.Write(HtmlEntity.DeEntitize(Regex.Replace(html.TrimEnd(), @"\s{2,}", " ")));
                if (textInfo.LastCharWasSpace = char.IsWhiteSpace(html[html.Length - 1]))
                {
                    outText.Write(' ');
                }
                break;
            case HtmlNodeType.Element:
                string endElementString = null;
                bool isInline;
                bool skip = false;
                int listIndex = 0;
                switch (node.Name)
                {
                    case "nav":
                        skip = true;
                        isInline = false;
                        break;
                    case "body":
                    case "section":
                    case "article":
                    case "aside":
                    case "h1":
                    case "h2":
                    case "header":
                    case "footer":
                    case "address":
                    case "main":
                    case "div":
                    case "p": // stylistic - adjust as you tend to use
                        if (textInfo.IsFirstTextOfDocWritten)
                        {
                            outText.Write("\r\n");
                        }
                        endElementString = "\r\n";
                        isInline = false;
                        break;
                    case "br":
                        outText.Write("\r\n");
                        skip = true;
                        textInfo.WritePrecedingWhiteSpace = false;
                        isInline = true;
                        break;
                    case "li":
                        if (textInfo.ListIndex > 0)
                        {
                            outText.Write("\r\n{0}.\t", textInfo.ListIndex++);
                        }
                        else
                        {
                            outText.Write("\r\n*\t"); //using '*' as bullet char, with tab after, but whatever you want eg "\t->", if utf-8 0x2022
                        }
                        isInline = false;
                        break;
                    case "ol":
                        listIndex = 1;
                        goto case "ul";
                    case "ul": //not handling nested lists any differently at this stage - that is getting close to rendering problems
                        endElementString = "\r\n";
                        isInline = false;
                        break;
                    case "a":

                        // [altText](pictures/image.png)
                        // [altText](url)
                        {
                            var Attrs = node.Attributes;
                            string alt = Attrs.Contains("alt") ? Attrs["alt"].Value.Trim() : "link text";
                            string src = Attrs.Contains("href") ? Attrs["href"].Value.Trim() : "";

                            if (process_a_href != null)
                                src = process_a_href.Invoke(src);

                            var aResult = @$"[{alt}]({src})";

                            outText.Write(aResult);
                        }
                        isInline = true; //true
                        break;
                    case "img": //inline-block in reality

                        // ![altText](pictures/image.png)
                        // ![altText](url)
                        {
                            var Attrs = node.Attributes;
                            string alt = Attrs.Contains("alt") ? Attrs["alt"].Value.Trim() : "img name";
                            string src = Attrs.Contains("src") ? Attrs["src"].Value.Trim() : "";
                            
                            if (process_img_url != null)
                                src = process_img_url.Invoke(src);

                            //try data-src
                            if (string.IsNullOrWhiteSpace(src))
                            {
                                src = Attrs.Contains("data-src") ? Attrs["data-src"].Value.Trim() : "";
                                if (process_img_url != null)
                                    src = process_img_url.Invoke(src);
                            }
                            

                            var imgResult ="\r\n" +@$"![{alt}]({src})";

                            outText.WriteLine(imgResult);
                        }
                        //isInline = true;
                        isInline = false;
                        break;

                    case "table":
                        skip = Handle_Table(node, outText);

                        isInline = false;
                        break;

                    default:
                        isInline = true;
                        break;
                }

                if (!skip && node.HasChildNodes)
                {
                    ConvertContentTo(node, outText, isInline ? textInfo : new PreceedingDomTextInfo(textInfo.IsFirstTextOfDocWritten) { ListIndex = listIndex });
                }
                if (endElementString != null)
                {
                    outText.Write(endElementString);
                }
                break;
        }
    }

    private static bool Handle_Table(HtmlNode node, TextWriter outText)
    {
        //dont dive into children of Table.. 
        //i will handle Table itself Here.
        bool skip = true;

        //outText.WriteLine("--------------Table START  -----------");
        outText.WriteLine("\r\n\r\n");
        outText.WriteLine();


        var table = node;

        var tableRows = table.QuerySelectorAll("tr").ToList();
        var columns = tableRows[0].QuerySelectorAll("th , td");

        var colName_AlignCount = columns.Max(x => x.InnerText.Trim().Length);
        //var colName_AlignCount = 20;
        var seperator = new string('-', 50);

        HandleTables_NonStandartChilds(outText, table);


        
        outText.WriteLine("|" + seperator);
        for (int i = 1; i < tableRows.Count; i++)
        {
            for (int e = 0; e < columns.Count; e++)
            {
                var value = tableRows[i].SelectSingleNode($"td[{e + 1}]");
                outText.WriteLine(
                    "  "
                    + columns[e].InnerText.Trim()
                    .ReplaceLineEndings(" ")
                    .PadRight(colName_AlignCount)
                    + " : "
                    + (value?.InnerText.Trim() ?? "")
                    );
            }
            outText.WriteLine(seperator);
            //outText.WriteLine();
        }

        //outText.WriteLine("\r\n--------------TABLE END  -----------");
        outText.WriteLine("\r\n\r\n");
        return skip;
    }

    private static void HandleTables_NonStandartChilds(TextWriter outText, HtmlNode table)
    {

        //handle nonStandart direct childs.
        var tableNonStandarChilds =
            table.ChildNodes
            .AsEnumerable()
            .Where(x => !new[] { "tr", "th", "td", "tbody" }.Contains(x.Name))
            .ToList();

        var tableNonStandarChilds_clean =
            tableNonStandarChilds
            .Where(nodex => !string.IsNullOrWhiteSpace(nodex.InnerText))
            .ToList();

        if (tableNonStandarChilds_clean.Count == 0)
            return;

        outText.WriteLine("### table --non-stardart-childs---start---");
        foreach (var nodex in tableNonStandarChilds_clean)
        {
            if (string.IsNullOrWhiteSpace(nodex.InnerText))
                continue;

            outText.WriteLine(
                "  " +  nodex.InnerText.Trim().TrimEnd( '\r', '\n' ) 
                );
        }
        outText.WriteLine("### table --non-stardart-childs---end---");
    }
}
internal class PreceedingDomTextInfo
{
    public PreceedingDomTextInfo(BoolWrapper isFirstTextOfDocWritten)
    {
        IsFirstTextOfDocWritten = isFirstTextOfDocWritten;
    }
    public bool WritePrecedingWhiteSpace { get; set; }
    public bool LastCharWasSpace { get; set; }
    public readonly BoolWrapper IsFirstTextOfDocWritten;
    public int ListIndex { get; set; }

    //public Func<string, string> process_img_url;
    //public Func<string, string> process_a_href;
}
internal class BoolWrapper
{
    public BoolWrapper() { }
    public bool Value { get; set; }
    public static implicit operator bool(BoolWrapper boolWrapper)
    {
        return boolWrapper.Value;
    }
    public static implicit operator BoolWrapper(bool boolWrapper)
    {
        return new BoolWrapper { Value = boolWrapper };
    }
}