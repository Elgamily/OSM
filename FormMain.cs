using OsmSharp.Osm.PBF.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace OSM
{
    public partial class FormOSM : Form
    {
        private List<string> postalCodeList = new List<string>();

        public FormOSM()
        {
            InitializeComponent();
        }

        private string DownloadFile(string address)
        {
            WebClient client = new WebClient();
            string filePath = Path.GetTempFileName() + ".osm.pbf";
            client.DownloadFile(address, filePath);

            return filePath;
        }

        private void WriteToTextFile(string filePath)
        {
            File.WriteAllLines(filePath, postalCodeList.ToArray());
        }

        private void Export()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "OSM";
            saveFileDialog.Filter = "txt files (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //download OSM.PBF file
                    string filePath = DownloadFile(textBoxInput.Text);

                    //read OSM.PBF file
                    using (var fileStream = new FileInfo(@filePath).OpenRead())
                    {
                        var source = new PBFOsmStreamSource(fileStream);

                        progressBarOSM.Visible = true;

                        int i = 0;
                        foreach (var element in source)
                        {
                            //get Postal_Code(s)
                            string s_element = element.ToString();
                            string[] sa_element = s_element.Split(',');
                            string s_postalCode = Array.Find(sa_element, e => e.StartsWith("postal_code", StringComparison.Ordinal));

                            if (s_postalCode != null)
                            {
                                string[] sa_postalCode = s_postalCode.Split('=');

                                if (sa_postalCode.Length == 2 && !postalCodeList.Contains(sa_postalCode[1]))
                                {
                                    //fill postalCodeList
                                    postalCodeList.Add(sa_postalCode[1]);
                                }
                            }

                            //refresh progressbar
                            if (i == 1000)
                            {
                                progressBarOSM.Refresh();
                                i = 0;
                            }

                            i++;
                        }

                        progressBarOSM.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    progressBarOSM.Visible = false;
                }

                //write to .txt file
                WriteToTextFile(saveFileDialog.FileName);

                Close();
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            Export();
        }
    }
}
