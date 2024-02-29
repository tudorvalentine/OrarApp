using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeDataGridViews();
            
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(System.String)))
            {
                Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));
                dataGridView1.Rows[dataGridView1.HitTest(clientPoint.X, clientPoint.Y).RowIndex].Cells[dataGridView1.HitTest(clientPoint.X, clientPoint.Y).ColumnIndex].Value = (string)e.Data.GetData(typeof(System.String));
            }
        }

        /* private void dataGridView1_DragEnter(object sender, DragEventArgs e)
         {
             if (e.Data.GetDataPresent(typeof(System.String)))
                 e.Effect = DragDropEffects.Copy;
             else
                 e.Effect = DragDropEffects.None;
         }*/
        private Timer warningTimer = new Timer();
        private Label warningLabel = new Label();

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(System.String)))
            {
                // Get the dragged data
                string draggedData = (string)e.Data.GetData(typeof(System.String));

                // Extract the group from the dragged data
                string[] lines = draggedData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                string draggedGroup = lines[2]; // Assuming group is in the third column

                // Get the column index of the drop location in dataGridView1
                Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));
                int dropColumnIndex = dataGridView1.HitTest(clientPoint.X, clientPoint.Y).ColumnIndex;

                // Check if the group matches the group of the target column
                if (dropColumnIndex >= 0 && dropColumnIndex < dataGridView1.Columns.Count)
                {
                    string targetGroup = dataGridView1.Columns[dropColumnIndex].HeaderText.Trim(); // Assuming group is the header text
                    if (draggedGroup != targetGroup)
                    {
                        // Cancel the drag operation
                        e.Effect = DragDropEffects.None;

                        // Display warning message
                        ShowWarningMessage("Cannot drag the row to a column with a different group.");
                        return;
                    }
                }

                // Allow the drag operation if the group matches
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ShowWarningMessage(string message)
        {
            // Configure the warning label
            warningLabel.Text = message;
            warningLabel.AutoSize = true;
            warningLabel.BackColor = Color.Yellow;
            warningLabel.Location = new Point(dataGridView1.Location.X, dataGridView1.Location.Y + dataGridView1.Height + 5);

            // Add the warning label to the form
            Controls.Add(warningLabel);

            // Set up a timer to hide the warning message after a delay
            warningTimer.Interval = 3000; // 3 seconds
            warningTimer.Tick += (s, e) =>
            {
                warningTimer.Stop();
                warningLabel.Hide();
            };
            warningTimer.Start();
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                // Get the group of the selected row in dataGridView2
                string selectedGroup = dataGridView2.SelectedRows[0].Cells[2].Value.ToString();

                // Find the corresponding column in dataGridView1 by matching the group
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (column.HeaderText.Trim() == selectedGroup)
                    {
                        // Highlight the column by changing its background color
                        column.DefaultCellStyle.BackColor = Color.Yellow;
                    }
                    else
                    {
                        // Reset background color for other columns
                        column.DefaultCellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                    }
                }
            }
            else
            {
                // If no row is selected in dataGridView2, reset background color for all columns in dataGridView1
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.DefaultCellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                }
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {

            this.WindowState = FormWindowState.Maximized;
            InitializeDataGridViews();
        }

        private void InitializeDataGridViews()
        {
            int eightyPercentWidth = (int)(this.Width * 0.8);

            // Set the width of the DataGridViews
            dataGridView1.Width = eightyPercentWidth;
            dataGridView2.Width = eightyPercentWidth;

            // Set the height of the DataGridViews as desired
            // For example:
            dataGridView1.Height = 800; // Set height to 400 pixels
            dataGridView2.Height = 200; // Set height to 200 pixels

            // Adjust the location of the DataGridViews if needed
            // For example:
            dataGridView1.Location = new Point((this.Width - dataGridView1.Width) / 2, 250);
            dataGridView2.Location = new Point((this.Width - dataGridView2.Width) / 2, 30);
        }

        private void dataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int currentRowIndex = dataGridView2.HitTest(e.X, e.Y).RowIndex;
                //int displayedRowCount = dataGridView2.DisplayedRowCount(true);

                // Get the index of the first visible row
                //int firstDisplayedRowIndex = dataGridView2.FirstDisplayedScrollingRowIndex;

                int actualRowIndex = currentRowIndex;

                Trace.WriteLine("drag:" + actualRowIndex);

                if (actualRowIndex >= 0)
                {
                    DataGridViewRow selectedRow = dataGridView2.Rows[actualRowIndex];
                    StringBuilder rowData = new StringBuilder();

                    foreach (DataGridViewCell cell in selectedRow.Cells)
                    {
                        rowData.Append(cell.Value.ToString());
                        rowData.Append(Environment.NewLine);
                    }

                    dataGridView2.DoDragDrop(rowData.ToString(), DragDropEffects.Copy);
                }
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "csv File|*.csv";
            if (opf.ShowDialog() == DialogResult.OK)
            {
                List<Course> courseList = new List<Course>();

                using (TextFieldParser parser = new TextFieldParser(opf.FileName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        if (fields.Length == 5)
                        {
                            Course course = new Course
                            {
                                Subject = fields[0],
                                Type = fields[1],
                                Group = fields[2],
                                Language = fields[3],
                                Professor = fields[4]
                            };

                            courseList.Add(course);
                        }
                        else
                        {
                            MessageBox.Show("Invalid data format in the CSV file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                        }
                    }
                }
                LoadDataIntoDataGridView2(courseList);
                InitializeDataGridView1(courseList);
            }
        }

        private void InitializeDataGridView1(List<Course> courseList)
        {
            if (courseList == null || courseList.Count == 0)
                return;

            var groupNames = courseList.Select(c => c.Group).Distinct().ToList();

            foreach (var groupName in groupNames)
            {
                dataGridView1.Columns.Add(groupName, groupName);
            }

            string[] timeSlots = { "8:00 AM - 9:30 AM", "9:45 AM - 11:15 AM", "11:30 AM - 1:00 PM", "1:15 PM - 2:45 PM" };
            int rowCount = timeSlots.Length + 1;
            dataGridView1.Rows.Add(rowCount * groupNames.Count);

            for (int i = 0; i < groupNames.Count; i++)
            {
                for (int j = 0; j < timeSlots.Length; j++)
                {
                    int rowIndex = i * rowCount + j;
                    dataGridView1.Rows[rowIndex].HeaderCell.Value = timeSlots[j];

                    if (j == timeSlots.Length - 1)
                    {
                        dataGridView1.Rows[rowIndex + 1].HeaderCell.Value = "";
                        dataGridView1.Rows[rowIndex + 1].DefaultCellStyle.BackColor = Color.Black;
                    }
                }
            }
        }

        private void LoadDataIntoDataGridView2(List<Course> courseList)
        {
            dataGridView2.Rows.Clear();

            foreach (var course in courseList)
            {
                dataGridView2.Rows.Add(course.Subject, course.Type, course.Group, course.Language, course.Professor);
            }
        }
    }
}
