/*
*   SMXML 
*       by AHathon
*       
*   Parses xml files produced from "SMS Backup".
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SMXML {

    public partial class Form1 : Form {

        public Form1(){
            InitializeComponent();
        }

        //Parse single SMS tag
        private string parseSMS(XmlTextReader xread, string node) {
            string str = "";
            switch(node){
                case "date":
                    str = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(long.Parse(xread.GetAttribute(node)))).ToString("G", CultureInfo.CreateSpecificCulture("en-US"));
                    break;
                case "contact_name":
                    str = (xread.GetAttribute("type").Equals("1") ? xread.GetAttribute(node) : "Me") + ":";
                    break;
                case "body":
                    str = WebUtility.HtmlDecode(xread.GetAttribute(node));
                    break;
                default:
                    str = null;
                    break;
            }
            return str;
        }

        //Parse SMS Dump
        private void parseXML(XmlTextReader xread) {
            new Thread(() => {

                int cnt = 0;
                int totalCnt = 0;

                //Read each element
                while (xread.Read()) {

                    if (xread.NodeType == XmlNodeType.Element) {

                        //Get total SMS count first
                        while (xread.Name.Equals("smses") && totalCnt == 0) {
                            totalCnt = (int)Int32.Parse(xread.GetAttribute("count"));
                        }

                        //Parse each SMS
                        if (xread.Name.Equals("sms")) {
                            this.Invoke((MethodInvoker)delegate {
                                string name = parseSMS(xread, "contact_name"),
                                       date = parseSMS(xread, "date"),
                                       body = parseSMS(xread, "body");

                                textOut.AppendText("[" + date + "]" + Environment.NewLine, Color.Gray, FontStyle.Italic);
                                textOut.AppendText(name + " ", (name.Equals("Me:") ? Color.Blue : Color.Red));
                                textOut.AppendText(body + Environment.NewLine + Environment.NewLine);
                                label1.Text = ++cnt + "/" + totalCnt;
                            });
                        }
                    }
                    //Break loop after closing element is reached
                    else if(xread.NodeType == XmlNodeType.EndElement) {
                        if (xread.Name.Equals("smses")) break;
                    }
                }
                //State that parsing is done.
                this.Invoke((MethodInvoker)delegate { label1.Text = "Done!"; });
            }).Start();
        }

        //Menu 'Open'
        private void menuOpen_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SMS Backup (.xml)|*.xml|All Files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                textOut.Text = "";
                parseXML(new XmlTextReader(ofd.FileName));
            }
        }

        //Menu 'Close'
        private void menuClose_Click(object sender, EventArgs e) {
            textOut.Text = "";
            label1.Text = "";
        }

        //About SMXML
        private void menuAbout_Click(object sender, EventArgs e) {
            MessageBox.Show(
                "SMXML " + ProductVersion + Environment.NewLine + 
                "by AHathon" + Environment.NewLine + 
                "Licensed under GPLv3."
            );
        }

        //Exit Application
        private void menuExit_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        //Menu 'Export'
        private void menuExport_Click(object sender, EventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "Text File (*.txt)|*.txt|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == DialogResult.OK) File.WriteAllText(sfd.FileName, textOut.Text, Encoding.UTF8);
        }
    }
}

//Overrides for 'AppendText'
public static class RichTextBoxExtensions {
    public static void AppendText(this RichTextBox box, string text, Color color, FontStyle fntstyl = 0) {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;

        box.SelectionColor = color;
        box.SelectionFont = new Font(box.SelectionFont, fntstyl);
        box.AppendText(text);
        box.SelectionColor = box.ForeColor;
        box.SelectionFont = new Font(box.SelectionFont, FontStyle.Regular);
    }
}