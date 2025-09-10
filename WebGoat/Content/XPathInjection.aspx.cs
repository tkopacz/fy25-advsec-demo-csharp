using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
namespace OWASP.WebGoat.NET
{
    public partial class XPathInjection : System.Web.UI.Page
    {
        // Make into actual lesson
        private string xml = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><sales><salesperson><name>David Palmer</name><city>Portland</city><state>or</state><ssn>123-45-6789</ssn></salesperson><salesperson><name>Jimmy Jones</name><city>San Diego</city><state>ca</state><ssn>555-45-6789</ssn></salesperson><salesperson><name>Tom Anderson</name><city>New York</city><state>ny</state><ssn>444-45-6789</ssn></salesperson><salesperson><name>Billy Moses</name><city>Houston</city><state>tx</state><ssn>333-45-6789</ssn></salesperson></sales>";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["state"] != null)
            {
                FindSalesPerson(Request.QueryString["state"]);
            }
        }

        private void FindSalesPerson(string state)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xml);

            // Create an XPathNavigator for querying with variables
            XPathNavigator nav = xDoc.CreateNavigator();
            string xpath = "//salesperson[state=$state]";
            XPathExpression expr = nav.Compile(xpath);

            // Prepare argument list with the user-supplied value
            XsltArgumentList varList = new XsltArgumentList();
            varList.AddParam("state", string.Empty, state);

            // Set the expression context using our custom context for variable resolution
            expr.SetContext(new VariableContext(varList));

            XPathNodeIterator iterator = nav.Select(expr);
            // Collect nodes in a list for compatibility with previous logic
            List<XPathNavigator> nodes = new List<XPathNavigator>();
            while (iterator.MoveNext())
            {
                nodes.Add(iterator.Current.Clone());
            }

            if (nodes.Count > 0)
            {
                // (processing logic can be added here)
            }
        }
    }

    // Custom XsltContext for variable resolution
    public class VariableContext : XsltContext
    {
        private XsltArgumentList _args;
        public VariableContext(XsltArgumentList args) : base() { _args = args; }
        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            object value = _args.GetParam(name, string.Empty) ?? string.Empty;
            return new XsltContextVariableImpl(value);
        }
        // Unused in this context
        public override bool Whitespace => false;
        public override int CompareDocument(string baseUri, string nextbaseUri) => 0;
        public override bool PreserveWhitespace(XPathNavigator node) => false;
        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes) => null;
    }
    // Helper for returning variable values
    public class XsltContextVariableImpl : IXsltContextVariable
    {
        private object _value;
        public XsltContextVariableImpl(object value) { _value = value; }
        public bool IsLocal => false;
        public bool IsParam => true;
        public XPathResultType VariableType => XPathResultType.Any;
        public object Evaluate(XsltContext xsltContext) => _value;
    }
}
