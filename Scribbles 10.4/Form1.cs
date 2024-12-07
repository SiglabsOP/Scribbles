using System.Diagnostics;
using System.Drawing.Text;
using System.Security.Cryptography;
using System.Text;



namespace WinFormsApp3
{
    public partial class Form1 : Form
    {
        private void ProcessTextWithParagraphs(string text)
        {
            richTextBox1.Clear();
            richTextBox1.RightToLeft = RightToLeft.No;

            string[] paragraphs = text.Split(new[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string paragraph in paragraphs)
            {
                richTextBox1.AppendText(paragraph.Trim() + Environment.NewLine + Environment.NewLine);
                richTextBox1.SelectionStart = richTextBox1.Text.Length; // Reset the SelectionStart to the end
            }
        }
        private void OpenFile(string filePath)
        {
            try
            {
                string fileContent = File.ReadAllText(filePath);

                // Check if the file content contains <p> tags - (c) SIG LABS 2024
                if (fileContent.Contains("<p>") || fileContent.Contains("</p>"))
                {
                    ProcessTextWithParagraphs(fileContent);
                }
                else
                {
                    // If no <p> tags, load the file content without modification
                    richTextBox1.Text = fileContent;
                }
            }
            catch (Exception ex)
            {
                // Handle file read error
                _ = MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string filePath = "C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\richtextbox_text.txt";
        private readonly string configFilePath = "C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\checkbox_value.txt";
        private readonly string radioFilePath = "C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt";

        private Button? myButton; // Mark myButton as nullable
        private void myButton_Click(object? sender, EventArgs e)
        {
            _ = MessageBox.Show("Please select an RTF file for corruption check. The result does *not* mean the file is corrupted, or broken.  The result is merely an indicator of how the RTF code is constructed, and helps you maintain your RTF library.  It is possible the result indicates corruption, however this does not need to be the case, in most cases, it will not be the case.  For example if an anomaly is detected, it does not mean essentially the file is corrupt (because the way RTF works).  Since RTF files are fragile, this code helps you maintain your RTF library in the long term.", "Instructions", MessageBoxButtons.OK, MessageBoxIcon.Information);

            OpenFileDialog openFileDialog = new()
            {
                Filter = "Rich Text Format|*.rtf",
                Title = "Select RTF File For Corruption Risk Analysis" // Set the title here
            };


            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    // Read the entire content of the RTF file
                    string rtfContent = File.ReadAllText(filePath);

                    // Check for specific patterns or anomalies that may indicate corruption
                    string corruptionIssues = GetCorruptionIssues(rtfContent);

                    if (!string.IsNullOrEmpty(corruptionIssues))
                    {
                        _ = MessageBox.Show($"File appears to have the following issues:\n{corruptionIssues}", "Corruption Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // File is not corrupted, proceed with loading
                        richTextBox1.LoadFile(filePath, RichTextBoxStreamType.RichText);
                        _ = MessageBox.Show("File loaded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    // An exception occurred during the file read, indicating potential corruption
                    _ = MessageBox.Show($"Error reading file: {ex.Message}", "Corruption Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Custom method to check for corruption and return a string describing the issues
        private string GetCorruptionIssues(string rtfContent)
        {
            StringBuilder issues = new();

            // Check 1: Ensure the file starts with the RTF header ({\rtf1) and ends with the RTF footer (})
            if (!rtfContent.StartsWith("{\\rtf1") || !rtfContent.EndsWith("}"))
            {
                _ = issues.AppendLine("Missing or incorrect RTF header or footer.");
            }

            // Check 2: Verify that the control words (commands) are correctly formatted
            // You may want to add more specific checks based on your requirements

            // Check 3: Font and Style Information - Check for proper formatting of font tables and style definitions
            if (!rtfContent.Contains("{\\fonttbl") || !rtfContent.Contains("{\\stylesheet"))
            {
                _ = issues.AppendLine("Missing font tables or style definitions.");
            }

            // Check 4: Color Information - Verify the color table and color-related commands
            if (!rtfContent.Contains("{\\colortbl"))
            {
                _ = issues.AppendLine("Missing color table or color-related commands.");
            }

            // Check 5: Object Embedding - Check for anomalies in embedded objects, such as images or OLE objects
            if (!rtfContent.Contains("{\\object"))
            {
                _ = issues.AppendLine("Anomalies in embedded objects.");
            }

            // Check 6: Special Characters - Examine the presence of special characters, escape sequences, and control symbols
            // You may want to add specific checks for these elements

            // Check 7: Grouping and Nesting - Ensure that groups and nested structures are balanced and properly closed
            if (!IsBalancedAndProperlyClosed(rtfContent))
            {
                _ = issues.AppendLine("Unbalanced or improperly closed groups.");
            }

            // Check 8: Control Words - Check for unexpected or unknown control words that may indicate non-standard content
            // You may want to add specific checks for known valid control words

            // Check 9: Encoding - Verify that character encoding is consistent and matches the declared encoding
            if (!rtfContent.Contains("\\ansicpg"))
            {
                _ = issues.AppendLine("Missing or inconsistent character encoding.");
            }

            // Check 10: Unicode Characters - Ensure proper handling of Unicode characters if applicable
            if (!rtfContent.Contains("\\uc"))
            {
                _ = issues.AppendLine("Issues with handling Unicode characters.");
            }

            // Check 11: File Size - Check for unusually large or small file sizes, which may indicate corruption
            if (rtfContent.Length is < 100 or > 1048576) // Adjust the size thresholds as needed
            {
                _ = issues.AppendLine("Unusual file size detected.");
            }

            // Add more checks based on your specific requirements

            // For demonstration purposes, let's check if the content contains the word "corrupted"
            if (rtfContent.Contains("corrupted", StringComparison.OrdinalIgnoreCase))
            {
                _ = issues.AppendLine("The word 'corrupted' is present.");
            }

            return issues.ToString();
        }

        // Helper method to check if groups and nested structures are balanced and properly closed
        private bool IsBalancedAndProperlyClosed(string rtfContent)
        {
            // Use a stack to keep track of opening and closing braces
            Stack<char> braceStack = new();

            foreach (char character in rtfContent)
            {
                if (character == '{')
                {
                    // Push opening brace onto the stack
                    braceStack.Push('{');
                }
                else if (character == '}')
                {
                    // Check if there's a matching opening brace on the stack
                    if (braceStack.Count == 0 || braceStack.Pop() != '{')
                    {
                        // Unbalanced or improperly closed braces
                        return false;
                    }
                }
            }

            // Check if all opening braces have matching closing braces
            return braceStack.Count == 0;
        }

        private async Task<bool> IsRTFFileCorruptAsync(string filePath)
        {
            try
            {
                // Read the content of the RTF file asynchronously
                string rtfContent = await Task.Run(() => File.ReadAllText(filePath));

                // Check if groups and nested structures are balanced and properly closed
                if (!IsBalancedAndProperlyClosed(rtfContent))
                {
                    _ = MessageBox.Show("Unbalanced or improperly closed groups detected.", "Corruption Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }

                // You can add additional checks or validation logic here based on your requirements
                // For a basic example, you can check if the content starts with '{\rtf' (a common RTF header)

                if (rtfContent.StartsWith("{\\rtf"))
                {
                    // The file is not corrupt
                    return false;
                }
                else
                {
                    // The file is corrupt
                    _ = MessageBox.Show("Invalid RTF file format.", "Corruption Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the self-check
                _ = MessageBox.Show($"Error during self-check: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // Treat any exception as corruption for simplicity
            }
        }

        public Form1()
        {
            InitializeUI();
            selectedOption = ""; // assign a default value

            InitializeComponent();
            richTextBox1.TextChanged += new EventHandler(wcounter_Click);

            // Check if myButton is not null before subscribing to the Click event
            if (myButton != null)
            {
                myButton.Click += myButton_Click;
            }

            selectedOption = ""; // assign a default value

            if (File.Exists(filePath))
            {
                using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[stream.Length];
                _ = stream.Read(buffer, 0, buffer.Length);

                // Check if richTextBox1 is not null before setting its Text property
                if (richTextBox1 != null)
                {
                    richTextBox1.Text = Encoding.UTF8.GetString(buffer);
                }
            }

            toolStripComboBox2.SelectedIndexChanged += new EventHandler(toolStripComboBox2_SelectedIndexChanged!);
            toolStripComboBox1.SelectedIndexChanged += new EventHandler(toolStripComboBox1_SelectedIndexChanged!);

            FormClosing += new FormClosingEventHandler(Form1_FormClosing!);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new()
            {
                Filter = "Text Files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All Files (*.*)|*.*"
            };

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFile.FileName;

                if (Path.GetExtension(filePath).Equals(".rtf", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle RTF file
                    try
                    {
                        richTextBox1.LoadFile(filePath, RichTextBoxStreamType.RichText);
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Error loading RTF file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Handle other text files
                    OpenFile(filePath);
                }
            }
        }

        private void InitializeUI()
        {
            // Create a button YEP THIS IS THE ONE the legendary one
            myButton = new Button
            {
                Text = "self-check",
                Size = new Size(250, 70),
                Location = new Point(10, 190) // should be 10 190
            };

            // Add the button to the form
            Controls.Add(myButton);

            // Subscribe to the form's Load event
            Load += Form1_Load;
        }
        private async Task FadeInControl(Control control)
        {
            control.Visible = false;
            control.Refresh();

            TaskCompletionSource<bool> fadeInTask = new();

            // Animation logic for growing
            for (double opacity = 0; opacity <= 1; opacity += 0.1)
            {
                _ = control.Invoke((MethodInvoker)delegate { control.Visible = true; });

                // Check if the control has an Opacity property before setting it
                if (control.GetType().GetProperty("Opacity") != null)
                {
                    _ = control.Invoke((MethodInvoker)delegate { control.GetType().GetProperty("Opacity")?.SetValue(control, opacity); });
                }

                // Increase the size of the control
                _ = control.Invoke((MethodInvoker)delegate
                {
                    control.Size = new Size((int)(control.Size.Width * 1.1), (int)(control.Size.Height * 1.1));
                });

                await Task.Delay(1);
            }

            // Animation logic for shrinking back to original size
            for (double opacity = 1; opacity >= 0; opacity -= 0.1)
            {
                // Check if the control has an Opacity property before setting it
                if (control.GetType().GetProperty("Opacity") != null)
                {
                    _ = control.Invoke((MethodInvoker)delegate { control.GetType().GetProperty("Opacity")?.SetValue(control, opacity); });
                }

                // Decrease the size of the control
                _ = control.Invoke((MethodInvoker)delegate
                {
                    control.Size = new Size((int)(control.Size.Width / 1.1), (int)(control.Size.Height / 1.1));
                });

                await Task.Delay(50);
            }

            fadeInTask.SetResult(true);
            _ = await fadeInTask.Task;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new()
            {
                Filter = "Text Files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All Files (*.*)|*.*"
            };

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFile.FileName;

                if (Path.GetExtension(filePath).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    // Save as plain text curenlty works with rtf tags for now
                    using StreamWriter writer = new(filePath, false, Encoding.UTF8);
                    writer.Write(richTextBox1.Text);
                }
                else
                {
                    // Save as RTF
                    richTextBox1.SaveFile(filePath, RichTextBoxStreamType.RichText);
                }
            }
        }



        private void LoadFontSizes()
        {
            for (int i = 8; i <= 72; i += 2)
            {
                _ = toolStripComboBox1.Items.Add(i);
            }
        }



        private async void Form1_Load(object? sender, EventArgs e)
        {

            try
            {
                // Read the radio button value from the file
                if (File.Exists(radioFilePath))
                {
                    using FileStream stream = new(radioFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] buffer = new byte[stream.Length];
                    _ = stream.Read(buffer, 0, buffer.Length);
                    string radioButtonValue = Encoding.UTF8.GetString(buffer).Trim(); if (!string.IsNullOrEmpty(radioButtonValue))
                    {
                        // Set the radio button value in the GUI
                        switch (radioButtonValue)
                        {
                            case "Option 1":
                                radioButton1.Checked = true;
                                break;
                            case "Option 2":
                                radioButton2.Checked = true;
                                break;
                            case "Option 3":
                                radioButton3.Checked = true;
                                break;
                            case "Option 4":
                                radioButton4.Checked = true;
                                break;
                            case "Option 5":
                                radioButton5.Checked = true;
                                break;
                            case "Option 6":
                                radioButton6.Checked = true;
                                break;
                            case "Option 7":
                                radioButton7.Checked = true;
                                break;
                            case "Option 8":
                                radioButton8.Checked = true;
                                break;
                            default:
                                // Handle invalid values
                                _ = MessageBox.Show("Invalid radio button value in file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }
                    }
                }
                // Perform the fade-in animation for the button
                if (myButton != null)
                {
                    await FadeInControl(myButton);
                }

            }
            catch (IOException)
            {
                // Wait for a short time and then retry the file access
                System.Threading.Thread.Sleep(500);
                Form1_Load(sender, e);
            }
            try
            {
                // Read the checkbox values from the file
                if (File.Exists(configFilePath))
                {
                    using FileStream stream = new(configFilePath, FileMode.Open, FileAccess.Read);
                    byte[] buffer = new byte[stream.Length];
                    _ = stream.Read(buffer, 0, buffer.Length);
                    string[] checkboxValues = Encoding.UTF8.GetString(buffer).Split(','); if (checkboxValues.Length >= 16)
                    {
                        // Set the checkbox values in the GUI
                        checkBox1.Checked = Convert.ToBoolean(checkboxValues[0]);
                        checkBox2.Checked = Convert.ToBoolean(checkboxValues[1]);
                        checkBox3.Checked = Convert.ToBoolean(checkboxValues[2]);
                        checkBox4.Checked = Convert.ToBoolean(checkboxValues[3]);
                        checkBox5.Checked = Convert.ToBoolean(checkboxValues[4]);
                        checkBox6.Checked = Convert.ToBoolean(checkboxValues[5]);
                        checkBox7.Checked = Convert.ToBoolean(checkboxValues[6]);
                        checkBox8.Checked = Convert.ToBoolean(checkboxValues[7]);
                        checkBox9.Checked = Convert.ToBoolean(checkboxValues[8]);
                        checkBox10.Checked = Convert.ToBoolean(checkboxValues[9]);
                        checkBox11.Checked = Convert.ToBoolean(checkboxValues[10]);
                        checkBox12.Checked = Convert.ToBoolean(checkboxValues[11]);
                        checkBox13.Checked = Convert.ToBoolean(checkboxValues[12]);
                        checkBox14.Checked = Convert.ToBoolean(checkboxValues[13]);
                        checkBox15.Checked = Convert.ToBoolean(checkboxValues[14]);
                        checkBox16.Checked = Convert.ToBoolean(checkboxValues[15]);
                    }
                    else
                    {
                        // Handle error if the config file does not contain all 16 checkbox values
                        _ = MessageBox.Show("Invalid config file format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                LoadFontTypes();
                LoadFontSizes();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the form load process
                _ = MessageBox.Show($"An error occurred during form load: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Repeat for all 16 checkboxes

        private void SaveConfig()
        {
            using StreamWriter writer = new(configFilePath);
            writer.Write($"{checkBox1.Checked},{checkBox2.Checked},{checkBox3.Checked},{checkBox4.Checked},{checkBox5.Checked},{checkBox6.Checked},{checkBox7.Checked},{checkBox8.Checked},{checkBox9.Checked},{checkBox10.Checked},{checkBox11.Checked},{checkBox12.Checked},{checkBox13.Checked},{checkBox14.Checked},{checkBox15.Checked},{checkBox16.Checked}");
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save checkbox values to configuration file
            using (StreamWriter writer = new(configFilePath))
            {
                writer.Write($"{checkBox1.Checked},{checkBox2.Checked},{checkBox3.Checked},{checkBox4.Checked},{checkBox5.Checked},{checkBox6.Checked},{checkBox7.Checked},{checkBox8.Checked},{checkBox9.Checked},{checkBox10.Checked},{checkBox11.Checked},{checkBox12.Checked},{checkBox13.Checked},{checkBox14.Checked},{checkBox15.Checked},{checkBox16.Checked}");
            }
            using (StreamWriter writer = new(filePath))
            {
                writer.Write(richTextBox1.Text);
            }

        }


        private void WriteSelectedOptionToFile(string filePath)
        {
            using StreamWriter writer = new(filePath);
            writer.WriteLine(selectedOption);
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedItem != null)
            {
                float selectedSize = Convert.ToSingle(toolStripComboBox1.SelectedItem);
                Font currentFont = richTextBox1.SelectionFont;
                currentFont ??= richTextBox1.Font;
                Font newFont = new(currentFont.FontFamily, selectedSize, currentFont.Style);
                richTextBox1.SelectionFont = newFont;
            }
        }


        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {

        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void LoadFontTypes()
        {
            InstalledFontCollection fonts = new();
            foreach (FontFamily family in fonts.Families)
            {
                _ = toolStripComboBox2.Items.Add(family.Name);
                if (richTextBox1.Font.FontFamily.Equals("Cronos Pro"))
                {
                    toolStripComboBox2.SelectedItem = family.Name;
                }
            }
            toolStripComboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox2_Click(object sender, EventArgs e)
        {
            toolStripComboBox2.Focus();


        }
        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox2.SelectedItem != null)
            {
                Font selectedFont = new((string)toolStripComboBox2.SelectedItem, richTextBox1.Font.Size);
                richTextBox1.SelectionFont = selectedFont;
            }
        }


        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }







        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string hashFileName = "";
            using (SaveFileDialog saveFileDialog = new())
            {
                saveFileDialog.Filter = "Rich Text Files (*.rtf)|*.rtf|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Delete hash file if it exists
                    hashFileName = $"{Path.GetFileNameWithoutExtension(saveFileDialog.FileName)}.hash";
                    if (File.Exists(hashFileName))
                    {
                        try
                        {
                            File.Delete(hashFileName);
                        }
                        catch (Exception ex)
                        {
                            _ = MessageBox.Show($"Failed to delete previous hash file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    string text = richTextBox1.Rtf;
                    int fileSize = Encoding.UTF8.GetByteCount(text);
                    toolStripProgressBar1.Maximum = fileSize;
                    toolStripProgressBar1.Minimum = 0;
                    toolStripProgressBar1.Value = 0;

                    // Determine chunk size based on file size
                    int chunkSize = fileSize < 5000000 ? 1048576 : 5242880; // 1 MB or 5 MB
                    string[] chunks = SplitText(text, chunkSize);

                    // Write each chunk to a separate file in parallel
                    List<string> fileNames = new();
                    _ = Parallel.ForEach(chunks, (chunk, state, index) =>
                    {
                        string fileName = $"{Path.GetFileNameWithoutExtension(saveFileDialog.FileName)}_{index}{Path.GetExtension(saveFileDialog.FileName)}.tmp";
                        try
                        {
                            using (FileStream stream = new(fileName, FileMode.Create, FileAccess.Write))
                            {
                                byte[] data = Encoding.UTF8.GetBytes(chunk);
                                stream.Write(data, 0, data.Length);
                            }
                            fileNames.Add(fileName);
                        }
                        catch (Exception ex)
                        {
                            state.Break();
                            _ = MessageBox.Show($"Failed to write chunk {index} to file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });

                    // Generate hash file
                    try
                    {
                        using FileStream stream = new(hashFileName, FileMode.Create, FileAccess.Write);
                        using StreamWriter writer = new(stream);
                        foreach (string fileName in fileNames.OrderBy(f => f))
                        {
                            string hash = GetFileHash(fileName);
                            writer.WriteLine($"{Path.GetFileName(fileName)},{hash}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show($"Failed to write hash file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // Merge files together
                    using (FileStream mergedStream = new(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        foreach (string fileName in fileNames.OrderBy(f => f))
                        {
                            byte[] data = File.ReadAllBytes(fileName);
                            mergedStream.Write(data, 0, data.Length);
                        }
                    }

                    // Delete temporary chunk files
                    foreach (string fileName in fileNames)
                    {
                        File.Delete(fileName);
                    }

                    toolStripProgressBar1.Value = fileSize;
                }
            }

            // Delete hash file
            if (File.Exists(hashFileName))
            {
                try
                {
                    File.Delete(hashFileName);
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Failed to delete hash file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }


        private static string[] SplitText(string text, int chunkSize)
        {
            string[] lines = text.Split(new string[] { "\\line" }, StringSplitOptions.None);
            List<string> chunks = new();
            string currentChunk = string.Empty;
            foreach (string line in lines)
            {
                string tempChunk = currentChunk + line + "\\line";
                if (Encoding.UTF8.GetByteCount(tempChunk) > chunkSize)
                {
                    chunks.Add(currentChunk.TrimEnd('\\'));
                    currentChunk = line + "\\line";
                }
                else
                {
                    currentChunk = tempChunk;
                }
            }
            if (!string.IsNullOrEmpty(currentChunk))
            {
                chunks.Add(currentChunk.TrimEnd('\\'));
            }

            return chunks.ToArray();
        }

        private static string GetFileHash(string fileName)
        {
            using MD5 md5 = MD5.Create();
            using FileStream stream = File.OpenRead(fileName);
            byte[] hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // Display a simple message box
            _ = MessageBox.Show("Visit us @ https://peterdeceuster.uk/", "Creator website", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Close();

        }

        private string selectedOption;
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                selectedOption = "Option 1";
                WriteSelectedOptionToFile("C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt");

            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                selectedOption = "Option 2";
                WriteSelectedOptionToFile("C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt");

            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                selectedOption = "Option 3";
                WriteSelectedOptionToFile("C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt");

            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                selectedOption = "Option 4";
                WriteSelectedOptionToFile("C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt");

            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                selectedOption = "Option 5";
                WriteSelectedOptionToFile("C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt");

            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                selectedOption = "Option 6";
                WriteSelectedOptionToFile("C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt");

            }
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked)
            {
                selectedOption = "Option 7";
                WriteSelectedOptionToFile("C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt");

            }
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton8.Checked)
            {
                selectedOption = "Option 8";
                WriteSelectedOptionToFile("C:\\VermeerSuite 10-1\\VermeerSuite Maintenance\\Settings\\Scribbles\\radio.txt");

            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void dsfsdfsdfToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dsfsdfgdsgToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void currentToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\VermeerSuite 10-1\Lab-Tangaroa\content creation\Academic\articles\alpha draft";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void approvedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\VermeerSuite 10-1\Lab-Tangaroa\content creation\Academic\articles\publication";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void currentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\VermeerSuite 10-1\Lab-Tangaroa\content creation\Academic\articles\beta edit";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\VermeerSuite 10-1\Lab-Tangaroa\content creation\Academic\articles\prerelease";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ddddToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void yearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void weekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toBeSourcedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void preliminaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void transformedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void correctionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ideasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void kickstartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void iNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void oUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the folder path you want to open in Explorer
                string folderPath = @"C:\";

                // Use Process.Start to open the folder in Windows Explorer
                _ = Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                _ = MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dsfsdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Display a simple message box with copyright information
            _ = MessageBox.Show("© 2024 Peter De Ceuster. https://peterdeceuster.uk/ Patch 10.4 Search and replace functionality,matches and wordcounter added.All rights reserved. Scribbles is multithreaded, runs in secure memory and can handle large ammounts of text.  You can use scribbles to categorize, and format your personal library. Scribbles 10.4 will now check for corruption while allowing you to track your progress and process <p> tags when opening a file. ", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void quicksaveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // This method will trigger every time the text in textBox2 changes.
            // You can use this to dynamically search as the user types, if desired.
            // For now, it's empty because the search functionality is triggered by the searchButton click.
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            // Get the search keyword from searchTextBox.
            string searchKeyword = searchTextBox.Text;

            // Check if the searchKeyword is not empty or null.
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                // Clear previous highlights
                ClearHighlights();

                // Start the search from the beginning
                int startIndex = 0;
                int keywordLength = searchKeyword.Length;
                int index;
                bool firstFound = false;
                int matchCount = 0;

                // Search and highlight all instances of the search keyword
                while ((index = richTextBox1.Find(searchKeyword, startIndex, RichTextBoxFinds.None)) != -1)
                {
                    // Select the found text in richTextBox1.
                    richTextBox1.Select(index, keywordLength);

                    // Highlight the found text by changing its background color.
                    richTextBox1.SelectionBackColor = Color.Yellow;

                    // Move the start index forward to search for the next instance
                    startIndex = index + keywordLength;

                    // Jump to the first found result
                    if (!firstFound)
                    {
                        richTextBox1.ScrollToCaret();
                        firstFound = true;
                    }

                    // Increment the match count
                    matchCount++;
                }

                // Update the counter label
                counter.Text = $"Matches found: {matchCount}";

                // If no instances were found, show a message box
                if (!firstFound)
                {
                    MessageBox.Show("No instances of the keyword were found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // If the searchKeyword is empty, show a message box.
                MessageBox.Show("Please enter a keyword to search.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearHighlights()
        {
            // Reset the selection start and length to remove any existing selection
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = richTextBox1.BackColor;
            richTextBox1.DeselectAll();
        }

        private void textBoxreplace_TextChanged(object sender, EventArgs e)
        {
            // This method will trigger every time the text in textBoxreplace changes.
            // Currently empty as the replace functionality is triggered by the replaceButton click.
        }

        private void buttonreplace_Click(object sender, EventArgs e)
        {
            // Get the search keyword from searchTextBox.
            string searchKeyword = searchTextBox.Text;

            // Get the replacement text from replaceTextBox.
            string replaceText = textBoxreplace.Text;

            // Check if the searchKeyword is not empty or null.
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                // Replace all instances of the search keyword with the replacement text.
                richTextBox1.Text = richTextBox1.Text.Replace(searchKeyword, replaceText);

                // Optionally, clear the highlights after replace
                ClearHighlights();

                // Optionally, highlight the replaced text again
                searchButton_Click(sender, e);
            }
            else
            {
                // If the searchKeyword is empty, show a message box.
                MessageBox.Show("Please enter a keyword to search for replacement.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void counter_Click(object sender, EventArgs e)
        {
            // Get the search keyword from searchTextBox.
            string searchKeyword = searchTextBox.Text;

            // Check if the searchKeyword is not empty or null.
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                // Start the search from the beginning
                int startIndex = 0;
                int keywordLength = searchKeyword.Length;
                int index;
                int matchCount = 0;

                // Count all instances of the search keyword
                while ((index = richTextBox1.Find(searchKeyword, startIndex, RichTextBoxFinds.None)) != -1)
                {
                    // Move the start index forward to search for the next instance
                    startIndex = index + keywordLength;

                    // Increment the match count
                    matchCount++;
                }

                // Update the counter label
                counter.Text = $"Matches found: {matchCount}";

                // If no instances were found, show a message box
                if (matchCount == 0)
                {
                    MessageBox.Show("No instances of the keyword were found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // If the searchKeyword is empty, show a message box.
                MessageBox.Show("Please enter a keyword to search.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void wcounter_Click(object? sender, EventArgs e)
        {
            // Get all the text in the RichTextBox
            string allText = richTextBox1.Text;

            // Split the text into words
            string[] words = allText.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Get the word count
            int wordCount = words.Length;

            // Display the word count
            wcounter.Text = $"Word Count: {wordCount}";
            // Optionally, you can use MessageBox to display the word count
            // MessageBox.Show($"Word Count: {wordCount}", "Word Count");
        }

        private void launchAIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the path to the executable
                string path = @"C:\Program Files\ChatGPT\ChatGPT.exe";

                // Create a new process to launch the application
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as if the file is not found
                MessageBox.Show($"Error launching application: {ex.Message}");
            }
        }
    }
}