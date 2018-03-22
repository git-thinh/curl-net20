using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace curl
{
    public class Paragraph
    {
        public int id { set; get; }
        public SENTENCE type { set; get; }
        public string text { set; get; }
        public string html { set; get; }

        public Paragraph() { }

        public Paragraph(int _id, string _s)
        {
            if (!string.IsNullOrEmpty(_s) && _s.Trim().Length > 0)
            {
                text = _s;
                if (_id == 0)
                {
                    type = SENTENCE.TITLE;
                    html = text.generalHTML(type);
                }
                else
                {
                    id = _id;
                    string si = _s.ToLower();
                    if (si.IndexOf("http") == 0)
                    {
                        type = SENTENCE.LINK;
                        html = text.generalHTML(type);
                    }
                    else if (si.IndexOf("note") == 0)
                    {
                        type = SENTENCE.NOTE;
                        html = text.generalHTML(type);
                    }
                    else
                    {
                        bool _isParagraph = false;
                        string _sen = string.Empty;
                        string[] aSen = _s.Split(EL._SPLIT_PARAGRAPH_TO_SENTENCE, StringSplitOptions.None).Where(x => x.Trim() != string.Empty).ToArray();
                        if (aSen.Length == 1)
                        {
                            int len = _s.Split(EL._SPLIT_PARAGRAPH_TO_CLAUSE, StringSplitOptions.None).Where(x => x.Trim() != string.Empty).Count();
                            if (len == 1)
                            {
                                type = SENTENCE.HEADING;
                                html = text.generalHTML(type);
                            }
                            else
                                _isParagraph = true;
                        }
                        if(_isParagraph)
                        {
                            type = SENTENCE.PARAGRAPH;
                            foreach (string se in aSen)
                            {
                                string[] ap = se.Split(EL._SPLIT_PARAGRAPH_TO_CLAUSE, StringSplitOptions.None).Where(x => x.Trim() != string.Empty).ToArray();
                                string hi = se;
                                for (int ki = 0; ki < ap.Length; ki++)
                                {
                                    hi = hi.Replace(ap[ki], string.Format("<{0}>{1}</{0}>", EL.TAG_WORD, ap[ki]));
                                }
                                _sen += string.Format("<{0}{1}{2}>{3}</{0}>", EL.TAG_SENTENCE, string.Empty, string.Empty, hi) + ".";
                            }
                            html = string.Format("<{0}{1}{2}>{3}</{0}>", EL.TAG_PARAGRAPH, string.Empty, string.Empty, _sen);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}: {2}", id, type.ToString(), text);
        }
    }

    public enum SENTENCE
    {
        TITLE,
        PARAGRAPH,
        HEADING,
        NOTE,
        CODE,
        UL_LI,
        LINK,
    }

    public static class EnglishExt
    {
        public static string generalHTMLAttr(this string text, SENTENCE type, string func = null, string attrClass = null)
        {
            Dictionary<string, string> dic = null;
            if (!string.IsNullOrEmpty(attrClass))
            {
                dic = new Dictionary<string, string>();
                dic.Add("class", attrClass);
            }
            return generalHTML(text, type, func, dic);
        }

        public static string generalHTML(this string text, SENTENCE type, string func = null, Dictionary<string, string> attributes = null)
        {
            string attr = string.Empty, f = string.Empty, tag = string.Empty;
            switch (type)
            {
                case SENTENCE.TITLE:
                    tag = EL.TAG_TITLE;
                    break;
                case SENTENCE.LINK:
                    tag = EL.TAG_LINK;
                    break;
                case SENTENCE.HEADING:
                    tag = EL.TAG_HEADING;
                    break;
                case SENTENCE.NOTE:
                    tag = EL.TAG_NOTE;
                    break;
                case SENTENCE.CODE:
                    tag = EL.TAG_CODE;
                    break;
                case SENTENCE.PARAGRAPH:
                    tag = EL.TAG_PARAGRAPH;
                    break;
            }

            if (attributes != null && attributes.Count > 0)
                attr = " " + string.Join(" ", attributes.Select(kv => string.Format(@"""{0}""=""{1}""", kv.Key, kv.Value)).ToArray()) + " ";
            if (!string.IsNullOrEmpty(func))
                f = string.Format(@" do={0} ", func);
            return string.Format("<{0}{1}{2}>{3}</{0}>", tag, f, attr, text);
        }
    }
}
