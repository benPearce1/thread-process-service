using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using javax.xml.transform;
using Saxon.Api;

namespace WindowsService1
{
    public class SchematronProcess
    {
        private ILogger logger;
        private string file;
        private string outputDirectory;

        public SchematronProcess(string file, string outputDirectory, ILogger logger)
        {
            this.file = file;
            this.logger = logger;
            this.outputDirectory = outputDirectory;
        }

        public void Start()
        {
            try
            {
                logger.Log("Starting processing of " + file);
                string tempFile = Path.Combine(@"c:\xbrl\temp",Path.GetFileName(Path.GetTempFileName()));
                FileInfo f = new FileInfo(file);
                string xsltFile =
                    @"\Schematron\SSCCT\PreLodge\SSCCTPreLodge Validation RulesAll.xsl";
                transform(file, xsltFile, tempFile);
                var destFileName = Path.Combine(outputDirectory, f.Name);
                File.Copy(tempFile, destFileName, true);
                logger.Log("Completed processing of " + file);
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message);
                logger.Log(ex.StackTrace);
            }
        }

        public static void transform(String src, String stylesheet, String tempFile)
        {
            XsltExecutable compiled = compileStylesheet(stylesheet);
            XsltTransformer transformer = compiled.Load();

            XdmNode doc = getSourceRoot(src);
            if (doc == null)
            {
                //Console.WriteLine("error: null transform source root node");
                Environment.Exit(-1);
            }

            transformer.InitialContextNode = doc;
            Serializer serializer = new Serializer();
            serializer.SetOutputFile(tempFile);
            transformer.Run(serializer);
            serializer.Close();
        }

        /**
         * Runs an XSLT transform, serializing the result to standard output.
         * 
         * @param name="src" the document to be transformed
         * @param name="stylesheet" the stylesheet to use
         */
        public static void transform(String src, String stylesheet)
        {
            XsltExecutable compiled = compileStylesheet(stylesheet);
            XsltTransformer transformer = compiled.Load();

            XdmNode doc = getSourceRoot(src);
            if (doc == null)
            {
                //Console.WriteLine("error: null transform source root node");
                Environment.Exit(-1);
            }

            transformer.InitialContextNode = doc;
            Serializer serializer = new Serializer();
            serializer.SetOutputStream(Console.OpenStandardOutput());
            transformer.Run(serializer);
            serializer.Close();
        }

        public static XsltExecutable compileStylesheet(String s)
        {
            //Debug.WriteLine("compiling stylesheet " + s);
            XsltExecutable stylesheet = null;
            Uri uri = null;

            try
            {
                uri = new Uri(s);
            }
            catch (UriFormatException e)
            {
                //Console.WriteLine("error compiling stylesheet '" + s + "': " + e.Message);
                Environment.Exit(0);
            }

            try
            {
                stylesheet = Vars.compiler.Compile(uri);
            }
            catch (TransformerConfigurationException e)
            {
                //Console.WriteLine("error compiling stylesheet '" + s + "': " + e.Message);
                Environment.Exit(0);
            }
            catch (FileNotFoundException e)
            {
                //Console.WriteLine("error compiling stylesheet: " + e.Message);
                Environment.Exit(0);
            }
            return stylesheet;
        }

        private static XdmNode getSourceRoot(String src)
        {
            //Debug.WriteLine("getting source root node for " + src);

            Uri uri = null;
            try
            {
                XmlUrlResolver resolver = new XmlUrlResolver();
                uri = resolver.ResolveUri(null, src);   //null sets the current location as the resolution base
            }
            catch (UriFormatException e)
            {
                //Console.WriteLine("error getting transformation source '" + uri + "': " + e.Message);
            }

            //Debug.WriteLine("resolved uri=" + uri.AbsolutePath);

            XdmNode doc = null;
            try
            {
                doc = Vars.builder.Build(uri);
            }
            catch (FileNotFoundException e)
            {
                //Console.WriteLine("error getting transformation source: " + e.Message);
            }
            catch (XPathException e)
            {
                //Console.WriteLine("error getting transformation source: " + e.Message);
            }
            return doc;
        }
    }

    public static class Vars
    {
        public static Processor processor = new Processor();
        public static XsltCompiler compiler = processor.NewXsltCompiler();
        public static DocumentBuilder builder = processor.NewDocumentBuilder();
    }
}