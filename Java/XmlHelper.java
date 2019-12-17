package lib;

import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.xml.sax.SAXException;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;
import java.io.File;
import java.io.IOException;

public class XmlHelper {
    public static boolean SaveNewNodeValueToFile(String xmlFilePath, String xpathExpression, String newNodeValue){
        boolean success = false;

        DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
        DocumentBuilder docBuilder = null;
        try {
            docBuilder = docFactory.newDocumentBuilder();
        } catch (ParserConfigurationException e) {
            e.printStackTrace();
        }
        try {
            //Get the document
            Document doc = docBuilder.parse(new File(xmlFilePath));

            //Update the node
            UpdateDocumentNode(doc, xpathExpression, newNodeValue);

            //Save the document back to the file
            SaveDocument(doc, xmlFilePath);

        } catch (SAXException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }

        return success;
    }

    public static boolean UpdateDocumentNode(Document doc, String xpathExpression, String newNodeValue){
        boolean success = false;

        XPath xPath = XPathFactory.newInstance().newXPath();
        Node theNode = null;
        try {
            theNode = (Node) xPath.compile(xpathExpression).evaluate(doc, XPathConstants.NODE);
            theNode.setTextContent(newNodeValue);
            success = true;
        } catch (XPathExpressionException e) {
            e.printStackTrace();
        }

        return success;
    }

    public static boolean SaveDocument(Document doc, String xmlFilePath) {
        boolean success = false;
        Transformer tf = null;
        try {
            tf = TransformerFactory.newInstance().newTransformer();
        } catch (TransformerConfigurationException e) {
            e.printStackTrace();
        }
        if (tf != null) {
            tf.setOutputProperty(OutputKeys.INDENT, "yes");
            tf.setOutputProperty(OutputKeys.METHOD, "xml");
            tf.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", "4");

            DOMSource domSource = new DOMSource(doc);
            StreamResult sr = new StreamResult(new File(xmlFilePath));
            try {
                tf.transform(domSource, sr);
                success = true;
            } catch (TransformerException e) {
                e.printStackTrace();
            }
        }
        return success;
    }
}
