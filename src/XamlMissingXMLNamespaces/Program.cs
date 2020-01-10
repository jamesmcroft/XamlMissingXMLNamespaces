namespace XamlMissingXMLNamespaces
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Xml;
    using Logging;

    public class Program
    {
        private const string XamlPresentationNamespace = "def";

        private static readonly string StyleTargetTypeXPath = $"//{XamlPresentationNamespace}:Style[@TargetType]";

        public static void Main(string[] args)
        {
            EventLog.StartFileLogging();

            var referenceXamlFiles = new List<FileInfo>();

            string referenceDirectoryPath = Environment.CurrentDirectory + "\\Reference";

            if (args != null && args.Length >= 1)
            {
                referenceDirectoryPath = args[0];
            }

            GetDirectoryXamlFiles(referenceDirectoryPath, referenceXamlFiles);

            EventLog.Info($"Found {referenceXamlFiles.Count} XAML reference files to examine missing namespaces for");

            var filesAndNamespaces = new Dictionary<string, List<string>>();

            foreach (FileInfo referenceXamlFile in referenceXamlFiles)
            {
                var fileMissingNamespaces = new List<string>();
                filesAndNamespaces.Add(referenceXamlFile.ToString(), fileMissingNamespaces);

                try
                {
                    XmlDocument xmlDocument = LoadXmlDocument(referenceXamlFile, out XmlNamespaceManager manager);

                    RetrieveMissingNamespaceElements(fileMissingNamespaces, xmlDocument, manager);
                }
                catch (XmlException xe) when (xe.Message.Contains("is an undeclared prefix"))
                {
                    string[] ns = xe.Message.Split('\'');
                    if (ns.Length >= 2)
                    {
                        fileMissingNamespaces.Add(ns[1]);
                    }
                }
            }

            var missingNamespaces = filesAndNamespaces.Where(x => x.Value.Any()).ToList();

            EventLog.Info($"Found {missingNamespaces.Count} missing namespaces");

            foreach (KeyValuePair<string, List<string>> missingNamespace in missingNamespaces)
            {
                foreach (string name in missingNamespace.Value)
                {
                    EventLog.Error($"Namespace missing from {missingNamespace.Key} - {name}");
                }
            }

            EventLog.Info("Completed");
            EventLog.StopFileLogging();

            Console.ReadLine();
        }

        private static void GetDirectoryXamlFiles(string directoryPath, List<FileInfo> fileInfos)
        {
            try
            {
                var referenceDirectory = new DirectoryInfo(directoryPath);
                GetResourcesFromDirectory(referenceDirectory, fileInfos);
            }
            catch (Exception ex)
            {
                EventLog.Error(ex);
            }
        }

        private static void RetrieveMissingNamespaceElements(
            List<string> missingNamespaces,
            XmlDocument xmlDocument,
            XmlNamespaceManager manager)
        {
            try
            {
                IDictionary<string, string> namespacesInScope = manager.GetNamespacesInScope(XmlNamespaceScope.All);
                var fileNamespaces = namespacesInScope.Select(namespaceInScope => namespaceInScope.Key).ToList();

                XmlNodeList nodes = xmlDocument.ChildNodes;
                if (nodes == null)
                {
                    return;
                }

                var foundNamespaces = new List<string>();

                // Retrieves nodes which use the namespace (i.e. declaring a control, <control:MyControl... />)
                RetrieveFileNodeNamespaces(nodes, foundNamespaces);

                // Retrieves Style nodes with TargetType which use the namespace (i.e. <Style TargetType="control:MyControl" />)
                RetrieveTargetTypeNamespaces(StyleTargetTypeXPath, "TargetType", foundNamespaces, xmlDocument, manager);

                // Retrieves nodes which use the namespace as an attribute (i.e. <TextBox control:MyControl.Text="Hello" />)
                RetrieveAttributeNamespaces(nodes, foundNamespaces);

                missingNamespaces.AddRange(foundNamespaces.Where(x => !fileNamespaces.Contains(x)));
            }
            catch (Exception ex)
            {
                EventLog.Error(ex);
            }
        }

        private static XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDocument)
        {
            XmlNodeList _xmlNameSpaceList = xmlDocument.SelectNodes(@"//namespace::*[not(. = ../../namespace::*)]");

            var manager = new XmlNamespaceManager(xmlDocument.NameTable);

            manager.AddNamespace(
                XamlPresentationNamespace,
                "http://schemas.microsoft.com/winfx/2006/xaml/presentation");

            foreach (XmlNode nsNode in _xmlNameSpaceList)
            {
                try
                {
                    manager.AddNamespace(nsNode.LocalName, nsNode.Value);
                }
                catch (Exception)
                {
                    // Ignored
                }
            }

            return manager;
        }

        private static void RetrieveFileNodeNamespaces(XmlNodeList nodes, List<string> namespaces)
        {
            foreach (XmlNode node in nodes)
            {
                // Get to the deepest node and work back
                RetrieveFileNodeNamespaces(node.ChildNodes, namespaces);

                if (!node.Name.Contains(":"))
                {
                    continue;
                }

                try
                {
                    string nodeNamespace = node.Name.Split(":")[0];
                    if (!string.IsNullOrWhiteSpace(nodeNamespace) && !namespaces.Contains(nodeNamespace))
                    {
                        namespaces.Add(nodeNamespace);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Error(ex.ToString());
                }
            }
        }

        private static void RetrieveAttributeNamespaces(XmlNodeList nodes, List<string> namespaces)
        {
            foreach (XmlNode node in nodes)
            {
                // Get to the deepest node and work back
                RetrieveAttributeNamespaces(node.ChildNodes, namespaces);

                try
                {
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (attribute.Name == null || !attribute.Name.Contains(":") || attribute.Name.Contains("xmlns"))
                        {
                            continue;
                        }

                        try
                        {
                            string attributeNamespace = attribute.Name.Split(":")[0];
                            if (!string.IsNullOrWhiteSpace(attributeNamespace) &&
                                !namespaces.Contains(attributeNamespace))
                            {
                                namespaces.Add(attributeNamespace);
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLog.Error(ex.ToString());
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignored.
                }
            }
        }

        private static void RetrieveTargetTypeNamespaces(
            string xpath,
            string attribute,
            List<string> namespaces,
            XmlDocument xmlDocument,
            XmlNamespaceManager manager)
        {
            // Finds all XAML elements and extracts their attributes.
            XmlNodeList nodes = xmlDocument.SelectNodes(xpath, manager);
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                try
                {
                    if (node.Attributes == null)
                    {
                        Console.WriteLine($"Ignoring {node.Name} because it has no attributes");
                        continue;
                    }

                    string targetType = node.Attributes[attribute].Value;

                    if (!targetType.Contains(":"))
                    {
                        continue;
                    }

                    string nodeNamespace = targetType.Split(":")[0];
                    if (!string.IsNullOrWhiteSpace(nodeNamespace) && !namespaces.Contains(nodeNamespace))
                    {
                        namespaces.Add(nodeNamespace);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private static XmlDocument LoadXmlDocument(FileInfo fileInfo, out XmlNamespaceManager manager)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(fileInfo.FullName);

            manager = GetNamespaceManager(xmlDocument);

            return xmlDocument;
        }

        private static void GetResourcesFromDirectory(DirectoryInfo directoryInfo, List<FileInfo> resourceFiles)
        {
            if (resourceFiles == null)
            {
                resourceFiles = new List<FileInfo>();
            }

            try
            {
                var childDirectories = directoryInfo.GetDirectories().ToList();

                if (childDirectories.Count > 0)
                {
                    foreach (DirectoryInfo directory in childDirectories)
                    {
                        GetResourcesFromDirectory(directory, resourceFiles);
                    }
                }

                resourceFiles.AddRange(directoryInfo.GetFiles("*.xaml"));
            }
            catch (Exception ex)
            {
                EventLog.Error(ex.ToString());
            }
        }
    }
}