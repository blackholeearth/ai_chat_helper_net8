using ReverseMarkdown;
using System;
using System.Collections.Generic;
using System.ComponentModel; // Required for BindingList
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ai_chat_helper_net8;

public partial class Form1 : Form
{
    // Use BindingList for automatic DataGridView updates when the list changes
    private BindingList<MyFile> filesToProcess = new BindingList<MyFile>();

    public Form1()
    {
        InitializeComponent();
        SetupControls();
        SetupDataGridView(); // Setup DataGridView binding

        this.DoubleBuffered = true;
        dgv_Files.SetControlDoubleBuffered(true);

        tbx_outputDir.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "aiHelper_out");
    }


    private void Form1_Load(object sender, EventArgs e){ }

    private void SetupControls()
    {
        // --- TextBox Setup ---
        if (this.lbl_filepaths == null)
        {
            MessageBox.Show("Error: TextBox 'tbx_filepaths' not found.", "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return; // Or handle differently
        }
        lbl_filepaths.AllowDrop = true;         // **** CRITICAL: Enable dropping ****
        lbl_filepaths.AutoSize = false;         // Ensure AutoSize is false so you can size it
        lbl_filepaths.BorderStyle = BorderStyle.FixedSingle; // Visual cue for the drop area
        lbl_filepaths.FlatStyle = FlatStyle.Flat;     // Optional visual style
        lbl_filepaths.TextAlign = System.Drawing.ContentAlignment.MiddleCenter; // Center the text
        lbl_filepaths.Text = "Drag & Drop Files / Folders Here"; // Initial text

        lbl_filepaths.DragEnter += tbx_filepaths_DragEnter;
        lbl_filepaths.DragDrop += tbx_filepaths_DragDrop;

    }

    private void SetupDataGridView()
    {
        if (this.dgv_Files == null)
        {
            MessageBox.Show("Error: DataGridView 'dgv_Files' not found.", "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return; // Or handle differently
        }

        // --- DataGridView Setup ---
        dgv_Files.AutoGenerateColumns = false; // Recommended for control
        dgv_Files.AllowUserToAddRows = false; // Usually false for display
        dgv_Files.AllowUserToDeleteRows = false; // Usually false
        dgv_Files.ReadOnly = true; // Make grid read-only
        dgv_Files.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // User selection style
        dgv_Files.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // Adjust column sizing

        // --- Define Columns Manually ---
        // File Path Column
        var filePathCol = new DataGridViewTextBoxColumn
        {
            Name = "FilePathColumn",
            HeaderText = "File Path",
            DataPropertyName = nameof(MyFile.FilePath), // Binds to the FilePath property
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None, // Let path fill available space
            FillWeight = 70 // Give more weight to path column
        };
        dgv_Files.Columns.Add(filePathCol);

        // Status Column
        var statusCol = new DataGridViewTextBoxColumn
        {
            Name = "StatusColumn",
            HeaderText = "Status",
            DataPropertyName = nameof(MyFile.Status), // Binds to the Status property
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None, // Size based on content
            FillWeight = 30 // Give less weight to status column
        };
        dgv_Files.Columns.Add(statusCol);

        dgv_Files.AllowUserToResizeColumns = true;

        // --- Set DataSource ---
        // Use the BindingList for two-way updates (list -> grid)
        dgv_Files.DataSource = filesToProcess;

        //do resize 1 time
        dgv_Files.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);

    }


    // --- Drag/Drop Event Handlers ---
    private void tbx_filepaths_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }
    // Located within your Form1 class
    // Required using statements: System, System.Collections.Generic, System.IO, System.Linq, System.Windows.Forms, System.ComponentModel

    private void tbx_filepaths_DragDrop(object sender, DragEventArgs e)
    {
        // Attempt to get the dropped data as file paths
        string[]? droppedItems = e.Data?.GetData(DataFormats.FileDrop) as string[];

        // Exit if no valid file data was dropped
        if (droppedItems == null || droppedItems.Length == 0)
        {
            Console.WriteLine("DragDrop operation detected no file items.");
            return;
        }

        // --- Counters for this specific drop operation ---
        int currentDropDiscoveredCount = 0; // How many top-level items were processed from the drop
        int currentDropAddedCount = 0;      // How many unique files were actually added to the main list
        int currentDropIgnoredCount = 0;    // How many were ignored as duplicates (either within drop or already in list)

        List<MyFile> newlyDiscoveredFiles =
            Get_DragDroppedFiles_toList(
                droppedItems,
            ref currentDropDiscoveredCount,
            ref currentDropIgnoredCount);

        Console.WriteLine($"--- Discovery phase complete. Found {newlyDiscoveredFiles.Count} potential files. Ignored {currentDropIgnoredCount} duplicates within drop. ---");

        // 2. Add items efficiently from the temporary list to the main BindingList
        if (newlyDiscoveredFiles.Any())
        {
            filesToProcess.RaiseListChangedEvents = false; // Pause DataGridView updates
            try
            {
                foreach (var newItem in newlyDiscoveredFiles)
                {
                    // Check against EXISTING items in the main filesToProcess list
                    if (!filesToProcess.Any(existing => existing.FilePath.Equals(newItem.FilePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        filesToProcess.Add(newItem);     // Add the unique item
                        currentDropAddedCount++;         // Increment count of ACTUALLY added files
                    }
                    else
                    {
                        currentDropIgnoredCount++;       // Increment ignored count (already in main list)
                    }
                }
            }
            finally // Ensure events are re-enabled and grid is refreshed
            {
                filesToProcess.RaiseListChangedEvents = true;  // Resume DataGridView updates
                filesToProcess.ResetBindings();               // Force DataGridView to refresh from the updated list
                Console.WriteLine($"--- BindingList updated. Triggered ResetBindings. ---");
            }

            // 3. Update UI elements after the list has been fully updated
            //UpdateTextBoxDisplay(); // Optional: Update textbox display with current paths
            UpdateFormTitle(currentDropAddedCount, currentDropIgnoredCount); // Use the new detailed update method

            // Log summary of the add operation
            if (currentDropAddedCount > 0)
            {
                Console.WriteLine($"--- Drop Operation Summary: Added {currentDropAddedCount} new unique files. Ignored {currentDropIgnoredCount} duplicates (within drop or pre-existing). ---");
                // ProcessFiles(); // Decide whether to trigger processing automatically here
            }
            else
            {
                Console.WriteLine($"--- Drop Operation Summary: No new files added. Ignored {currentDropIgnoredCount} duplicates (within drop or pre-existing). ---");
            }
        }
        else // Case where newlyDiscoveredFiles is empty after the loop
        {
            // This means the drop contained items, but none resulted in valid, unique file paths
            // (e.g., only inaccessible folders, empty folders, or only duplicates within the drop itself)
            Console.WriteLine("--- Drop Operation Summary: No new valid files discovered or all were duplicates within the drop itself. ---");
            // Update title to reflect only the total count and potentially the ignored count from this drop
            UpdateFormTitle(0, currentDropIgnoredCount);
        }
        Console.WriteLine($"--- DragDrop operation finished. ---");


        //do resize 1 time
        dgv_Files.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        dgv_Files.Columns[0].Width += 60;

        //tbx out path 
        if (filesToProcess.Count <= 1) // single file -> default to desktop
            tbx_outputDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        else
            tbx_outputDir.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "aiHelper_out");
    }



    private static List<MyFile> Get_DragDroppedFiles_toList(string[] droppedItems, ref int currentDropDiscoveredCount, ref int currentDropIgnoredCount)
    {
        // ------------------------------------------------

        // Temporary list to hold discovered files before adding to BindingList
        List<MyFile> newlyDiscoveredFiles = new List<MyFile>();

        // 1. Discover files and populate the temporary list
        Console.WriteLine($"--- Processing {droppedItems.Length} dropped items ---");
        foreach (string itemPath in droppedItems)
        {
            currentDropDiscoveredCount++; // Count each top-level item being processed
            try
            {
                // Check if the dropped item is a file
                if (File.Exists(itemPath))
                {
                    var newFile = new MyFile(itemPath) { Status = "draggedViaMouse" };
                    // Check for duplicates within this single drop batch before adding to temp list
                    if (!newlyDiscoveredFiles.Any(f => f.FilePath.Equals(itemPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        newlyDiscoveredFiles.Add(newFile);
                        // Console.WriteLine($"Discovered file: {itemPath}"); // Verbose logging
                    }
                    else
                    {
                        // This path was already found earlier in *this same drop operation* (e.g., dragging file twice)
                        currentDropIgnoredCount++;
                        Console.WriteLine($"Duplicate within drop ignored: {itemPath}");
                    }
                }
                // Check if the dropped item is a directory
                else if (Directory.Exists(itemPath))
                {
                    Console.WriteLine($"Enumerating directory: {itemPath}");
                    // Use EnumerateFiles for potentially better memory usage on huge folders
                    var filesInFolder = Directory.EnumerateFiles(itemPath, "*.*", SearchOption.AllDirectories);
                    foreach (string filePath in filesInFolder)
                    {
                        var newFile = new MyFile(filePath) { Status = "draggedViaMouse" };
                        // Check for duplicates within this single drop batch before adding to temp list
                        if (!newlyDiscoveredFiles.Any(f => f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                        {
                            newlyDiscoveredFiles.Add(newFile);
                            // Console.WriteLine($"  Discovered file in folder: {filePath}"); // Verbose logging
                        }
                        else
                        {
                            // This path was already found earlier in *this same drop operation*
                            currentDropIgnoredCount++;
                            Console.WriteLine($"Duplicate within drop ignored: {filePath}");
                        }
                    }
                    Console.WriteLine($"Finished enumerating directory: {itemPath}");
                }
                else
                {
                    Console.WriteLine($"Dropped item is neither a file nor a directory (or inaccessible): {itemPath}");
                    // Could potentially increment an 'invalidItemCount' if needed
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Log access denied errors without interrupting the user excessively
                Console.WriteLine($"Access denied processing '{itemPath}'. Skipping item/folder. Details: {ex.Message}");
                // Optionally increment an 'errorCount'
            }
            catch (Exception ex)
            {
                // Log other errors during file discovery
                Console.WriteLine($"Error processing '{itemPath}'. Skipping item/folder. Details: {ex.Message}");
                // Optionally increment an 'errorCount'
            }
        } // End foreach dropped item

        return newlyDiscoveredFiles;
    }


    // Inside your Form1 class

    /// <summary>
    /// Updates the form's title bar with total file count and details from the last drag-drop operation.
    /// </summary>
    /// <param name="addedLastDrop">Number of unique files added in the most recent drop.</param>
    /// <param name="ignoredLastDrop">Number of duplicate files ignored in the most recent drop.</param>
    private void UpdateFormTitle(int addedLastDrop = 0, int ignoredLastDrop = 0)
    {
        int totalCount = filesToProcess.Count; // Get the current total count

        // Start building the title string
        string title = $"File Collector --- Total: {totalCount} files";

        // Append details about the last drop operation if relevant counts were provided
        if (addedLastDrop > 0 || ignoredLastDrop > 0)
        {
            title += $" - Last Drop: {addedLastDrop} added";
            if (ignoredLastDrop > 0) // Only show ignored count if > 0
            {
                title += $", {ignoredLastDrop} duplicates ignored";
            }
        }

        // Set the form's text property
        // Use Invoke if called from a non-UI thread, but DragDrop is usually on the UI thread.
        if (this.InvokeRequired)
        {
            this.Invoke(() => this.Text = title);
        }
        else
        {
            this.Text = title;
        }
    }


   
    // --- Helper Function to Find Common Base Directory ---
    private string? FindCommonBaseDirectory(IEnumerable<string> filePaths)
    {
        var paths = filePaths?.Where(p => !string.IsNullOrEmpty(p)).ToList();

        if (paths == null || !paths.Any())
        {
            return null; // No paths provided
        }

        // Get directory names, handling potential errors
        List<string> directories = new List<string>();
        foreach (var path in paths)
        {
            try
            {
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    directories.Add(dir);
                }
            }
            catch (ArgumentException argEx)
            { // Handle invalid path chars etc.
                Console.WriteLine($"Warning: Invalid path skipped for base directory calculation: {path} ({argEx.Message})");
            }
            catch (PathTooLongException ptle)
            {
                Console.WriteLine($"Warning: Path too long skipped for base directory calculation: {path} ({ptle.Message})");
            }
        }


        if (!directories.Any())
        {
            return null; // No valid directories found
        }


        // Start with the first directory
        string commonPrefix = directories[0];

        // Compare with other directories
        for (int i = 1; i < directories.Count; i++)
        {
            string currentDir = directories[i];

            // Find the length of the shortest path to limit comparison
            int minLength = Math.Min(commonPrefix.Length, currentDir.Length);
            int lastSeparatorIndex = -1;

            // Find the last common character index
            int commonLength = 0;
            for (commonLength = 0; commonLength < minLength; commonLength++)
            {
                // Use OrdinalIgnoreCase for case-insensitive comparison on Windows
                if (!commonPrefix[commonLength].ToString().Equals(currentDir[commonLength].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                // Keep track of the last directory separator encountered in the common part
                if (commonPrefix[commonLength] == Path.DirectorySeparatorChar || commonPrefix[commonLength] == Path.AltDirectorySeparatorChar)
                {
                    lastSeparatorIndex = commonLength;
                }
            }

            // If commonLength is 0, there's no common prefix (e.g., different drives)
            if (commonLength == 0)
            {
                Console.WriteLine($"No common prefix found between '{commonPrefix}' and '{currentDir}'.");
                return null; // Or string.Empty depending on desired behavior
            }

            // Trim the common prefix to the last common directory separator
            // If lastSeparatorIndex remained -1 (e.g., "C:" vs "C:\"), adjust commonLength if paths partially match root
            if (lastSeparatorIndex >= 0)
            {
                commonPrefix = commonPrefix.Substring(0, lastSeparatorIndex + 1);
            }
            else
            {
                // Handle cases like C:\ vs C:\folder, common prefix should be C:\
                // If paths diverge immediately after root, commonLength would point after root (e.g., 3 for C:\)
                if (commonLength >= 2 && commonPrefix.IndexOf(Path.VolumeSeparatorChar) == 1 && (commonPrefix.Length == commonLength || commonPrefix[commonLength] == Path.DirectorySeparatorChar))
                {
                    commonPrefix = commonPrefix.Substring(0, commonLength);
                    // Ensure it ends with a separator if it's just the root (like "C:\")
                    if (!commonPrefix.EndsWith(Path.DirectorySeparatorChar.ToString()) && !commonPrefix.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                    {
                        if (commonPrefix.Length == 2 && commonPrefix[1] == Path.VolumeSeparatorChar)
                        {
                            commonPrefix += Path.DirectorySeparatorChar;
                        }
                    }
                }
                else
                {
                    // No common directory structure found after initial comparison
                    Console.WriteLine($"No common directory separator found between '{commonPrefix}' and '{currentDir}'. Divergence at index {commonLength}.");
                    return null; // Or string.Empty
                }
            }


            // If the prefix becomes empty, stop early
            if (string.IsNullOrEmpty(commonPrefix))
            {
                Console.WriteLine("Common prefix calculation resulted in empty string.");
                return null;
            }
        }
        Console.WriteLine($"Calculated Common Base Directory: {commonPrefix}");
        return commonPrefix;
    }


    // --- Modified Conversion Button Click Handler ---
    private void bt_convertToMarkdown_Click(object sender, EventArgs e)
    {
        // Get the selected output directory
        string outputDirectory = tbx_outputDir.Text ;

        // Basic validation for output directory
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            MessageBox.Show("Please select a valid output directory.", "Output Directory Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            tbx_outputDir?.Focus(); // Focus the textbox
            return;
        }
        // Optional: Check if directory exists, though CreateDirectory handles non-existence
        // if (!Directory.Exists(outputDirectory)) { ... }

        // Filter for HTML files to be processed
        //var htmlFilesToProcess = filesToProcess
        var htmlFilesToProcess = 
            GetFiles_matching_includeFilter()
            .Where(f => Path.GetExtension(f.FilePath).ToLowerInvariant() == ".htm" || Path.GetExtension(f.FilePath).ToLowerInvariant() == ".html")
            .ToList(); // Process a snapshot

        if (!htmlFilesToProcess.Any())
        {
            MessageBox.Show("No HTML (.htm, .html) files found in the list to convert.", "No HTML Files", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // --- Find the Common Base Directory for Structure Preservation ---
        string? commonBaseDirectory = FindCommonBaseDirectory(htmlFilesToProcess.Select(f => f.FilePath));
        if (commonBaseDirectory == null)
        {
            Console.WriteLine("Could not determine a common base directory. Outputting files flat.");
            // Decide behavior: error out, or output flat? Let's output flat for this example.
            // Alternatively:
            // MessageBox.Show("Could not determine a common base directory for the input files (e.g., files from different drives).\nCannot preserve folder structure.", "Structure Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            // return;
        }
        // --------------------------------------------------------------------

        bool mergeOutput = chk_mergeAll_intoSingleFile.Checked ;

        // UI Feedback & Processing Logic
        bt_convertToMarkdown.Enabled = false;
        bt_convertToMarkdown.Text = "Processing...";
        this.Cursor = Cursors.WaitCursor;

        List<string> successfulMarkdownFiles = new List<string>(); // Stores FULL paths in output dir
        int processedCount = 0;
        int successCount = 0;
        int errorCount = 0;

        try
        {
            Console.WriteLine($"--- Starting Markdown Conversion ({htmlFilesToProcess.Count} HTML items) ---");
            Console.WriteLine($"Output Directory: {outputDirectory}");
            Console.WriteLine($"Common Base Input Directory: {commonBaseDirectory ?? "N/A (Outputting Flat)"}");
            Console.WriteLine($"Merge Output: {mergeOutput}");

            foreach (var fileInfo in htmlFilesToProcess)
            {
                MyFile originalFileInfo = filesToProcess.FirstOrDefault(f => f.FilePath == fileInfo.FilePath);
                if (originalFileInfo == null) continue;

                processedCount++;
                originalFileInfo.Status = "Processing...";
                Application.DoEvents(); // Allow UI update (consider async/await)

                string inputFilePath = originalFileInfo.FilePath;
                string finalOutputMarkdownPath; // This will be the full path in the output structure

                try
                {
                    // --- Calculate Output Path with Structure Preservation ---
                    string markdownFileName = Path.ChangeExtension(Path.GetFileName(inputFilePath), ".md");
                    string relativePath = ""; // default, treat as flat

                    if (commonBaseDirectory != null)
                    {
                        try
                        {
                            // Get the directory of the input file relative to the common base
                            string? inputFileDir = Path.GetDirectoryName(inputFilePath);
                            if (inputFileDir != null)
                            {
                                relativePath = Path.GetRelativePath(commonBaseDirectory, inputFileDir);
                            }
                        }
                        catch (ArgumentException argEx)
                        {
                            Console.WriteLine($"Warning: Could not get relative path for {inputFilePath} from {commonBaseDirectory}. Outputting flat. Error: {argEx.Message}");
                            relativePath = ""; // Fallback to flat structure on error
                        }
                    }

                    // If relative path is "." (meaning file is in the base dir), make it empty
                    if (relativePath == ".") relativePath = "";

                    // Combine output directory, relative path, and new filename
                    string targetSubDir = Path.Combine(outputDirectory, relativePath);
                    finalOutputMarkdownPath = Path.Combine(targetSubDir, markdownFileName);

                    // Ensure the target subdirectory exists
                    Directory.CreateDirectory(targetSubDir); // Creates intermediate directories if needed
                    // --- End Calculate Output Path ---


                    // --- Perform Conversion ---
                    ////bool conversionSuccess = ConvertHtmlToMarkdown_notgood(inputFilePath, finalOutputMarkdownPath); // Use the actual implementation
                    bool conversionSuccess =
                        ConvertHtmlToMarkdown(
                            inputFilePath,
                            finalOutputMarkdownPath
                            , tbx_CssSelector_for_Root.Text);

                    if (conversionSuccess)
                    {
                        originalFileInfo.Status = "Converted";
                        successCount++;
                        if (mergeOutput)
                        {
                            successfulMarkdownFiles.Add(finalOutputMarkdownPath); // Add the *output* path
                        }
                        // Console.WriteLine($" -> Successfully converted: {finalOutputMarkdownPath}");
                    }
                    else
                    {
                        originalFileInfo.Status = "Error: Conversion Failed";
                        errorCount++;
                        Console.WriteLine($" -> Failed conversion (handled): {inputFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    originalFileInfo.Status = $"Error: {ex.GetType().Name}"; errorCount++;
                    Console.WriteLine($" FAILED (Unexpected) converting {inputFilePath}: {ex.Message}");
                }
            } // End foreach loop

            // Update status for non-HTML files in the main list
            foreach (var fileInfo in filesToProcess.Except(htmlFilesToProcess))
            {
                if (fileInfo.Status == "draggedViaMouse" || fileInfo.Status == "Initialized" || string.IsNullOrEmpty(fileInfo.Status))
                {
                    fileInfo.Status = "Skipped (Type)";
                }
            }


            Console.WriteLine($"--- Conversion Phase Complete. Processed: {processedCount}, Success: {successCount}, Errors: {errorCount} ---");

            // Merging Logic (uses outputDirectory)
            string mergeResult = "";
            if (mergeOutput && successfulMarkdownFiles.Any())
            {
                mergeResult = MergeMarkdownFiles(successfulMarkdownFiles, outputDirectory); // Pass output dir
            }
            else if (mergeOutput)
            {
                mergeResult = "No successful markdown files were created to merge.";
            }

            string summary = $"Conversion finished.\n\nProcessed HTML Files: {processedCount}\nSuccessful: {successCount}\nErrors: {errorCount}";
            if (mergeOutput)
                summary += $"\n\nMerge Result: {mergeResult}";

            // MessageBox.Show(summary, "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Text = " Done. at " + DateTime.Now.ToLongTimeString() + " --> " + summary.ReplaceLineEndings(" ; ");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred during the conversion process:\n\n{ex.Message}", "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            bt_convertToMarkdown.Enabled = true;
            bt_convertToMarkdown.Text = "Convert to Markdown";
            this.Cursor = Cursors.Default;
        }
    }

 
    private bool ConvertHtmlToMarkdown_notgood(string filePath, string outputMarkdownPath)
    {
        try
        {
            var config = new ReverseMarkdown.Config
            {
                // Include the unknown tag completely in the result (default as well)
                UnknownTags = Config.UnknownTagsOption.PassThrough,
                // generate GitHub flavoured markdown, supported for BR, PRE and table tags
                GithubFlavored = false,
                // will ignore all comments
                RemoveComments = true,
                // remove markdown output for links where appropriate
                SmartHrefHandling = true
            };
            var converter = new ReverseMarkdown.Converter(config);

            //string html = "This a sample <strong>paragraph</strong> from <a href=\"http://test.com\">my site</a>";
            string html = File.ReadAllText(filePath);

            string result = converter.Convert(html);

            File.WriteAllText(outputMarkdownPath, result);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("error at ConvertHtmlToMarkdown_Placeholder:" + ex.Message);
            return false;
        }


    }

    /// <summary>
    /// this one actually convert html to Text.. clears the htmltags , fixes indents and spaces.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="outputMarkdownPath"></param>
    /// <returns></returns>
    private bool ConvertHtmlToMarkdown(string filePath, string outputMarkdownPath, string cssSelector_Root = "")
    {
        try
        {
            string html = File.ReadAllText(filePath);

            string result = HtmlToText.ConvertHtml_toText(html, cssSelector_Root);
            //reduce empty line count.
            result = Regex.Replace(result, "((\r\n)|(\n)){3,}", @"\r\n\r\n", RegexOptions.Multiline);


            File.WriteAllText(outputMarkdownPath, result);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("error at ConvertHtmlToMarkdown:" + ex.Message);
            return false;
        }


    }


    private string MergeMarkdownFiles(List<string> markdownFilePaths, string outputDirectory)
    {
        if (markdownFilePaths == null || !markdownFilePaths.Any())
        {
            return "No files provided to merge.";
        }

        // Use the provided output directory
        string mergedFileName = "output_merged.md";
        string mergedFilePath = Path.Combine(outputDirectory, mergedFileName);

        try
        {
            Console.WriteLine($"Attempting to merge {markdownFilePaths.Count} files into: {mergedFilePath}");
            StringBuilder mergedContent = new StringBuilder();

            foreach (string filePath in markdownFilePaths) // These paths are already in the output structure
            {
                if (File.Exists(filePath))
                {
                    if (mergedContent.Length > 0)
                    {
                        mergedContent.AppendLine();
                        mergedContent.AppendLine("---"); // Separator
                        mergedContent.AppendLine();
                    }
                    // Use relative path from outputDirectory for header if desired, otherwise just filename
                    string relativeHeaderPath = Path.GetRelativePath(outputDirectory, filePath);
                    mergedContent.AppendLine($"<!-- Merged from: {relativeHeaderPath} -->");
                    // Or: mergedContent.AppendLine($"## Merged from: {Path.GetFileName(filePath)}");
                    mergedContent.AppendLine();
                    mergedContent.Append(File.ReadAllText(filePath));
                    Console.WriteLine($"  Appended content from: {filePath}");
                }
                else
                {
                    Console.WriteLine($"Merge Warning: File not found, skipping: {filePath}");
                    mergedContent.AppendLine();
                    mergedContent.AppendLine($"<!-- File not found during merge: {Path.GetFileName(filePath)} -->");
                    mergedContent.AppendLine();
                }
                mergedContent.AppendLine();
            }

            File.WriteAllText(mergedFilePath, mergedContent.ToString());
            return $"Successfully merged into: {mergedFilePath}";
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"Merge IO Error writing to {mergedFilePath}: {ioEx.Message}");
            return $"Error writing merged file: {ioEx.Message}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error during merge: {ex.Message}");
            return $"An unexpected error occurred during merging: {ex.GetType().Name}";
        }
    }



    // --- Delete Button Logic ---
    private void bt_removeSelectedRows_Click(object sender, EventArgs e)
    {
        //no row selected
        if (dgv_Files.SelectedRows.Count == 0)
            return;

        // Get the list of selected MyFile objects for removal
        var selectedFiles = dgv_Files.SelectedRows
            .Cast<DataGridViewRow>()
            .Select(row => row.DataBoundItem as MyFile)
            .Where(file => file != null) // Filter out any nulls.
            .ToList();

        // select rows doesnt have fileName
        if (!selectedFiles.Any())
            return;


        // Pause updates, remove the items, then resume updates
        filesToProcess.RaiseListChangedEvents = false; // Pause notifications
        try
        {
            foreach (var file in selectedFiles)
            {
                filesToProcess.Remove(file); // remove from the list.
            }
        }
        finally
        {
            filesToProcess.RaiseListChangedEvents = true;  // Resume DataGridView updates
            filesToProcess.ResetBindings(); // Force DataGridView to refresh from the updated list
        }

        UpdateFormTitle(); // Update the form title
        Console.WriteLine($"--- Files deleted. Triggered ResetBindings. {selectedFiles.Count} files deleted ---");
    }

    private void bt_clear_Click(object sender, EventArgs e) => filesToProcess.Clear();



    //------------ filter logic

    private List<string> GetFilterTerms(string filterText)
    {
        List<string> terms = new List<string>();
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return terms; // Return empty list if no filter text
        }

        string[] lines = filterText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] lineTerms = line.Split(new[] { ';' ,',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string term in lineTerms)
            {
                if (string.IsNullOrWhiteSpace(term.Trim()))
                    continue;

                terms.Add(term.Trim()); // Trim whitespace from individual terms
            }
        }
        return terms;
    }


    private List<MyFile> GetFiles_matching_includeFilter()
    {
        List<string> filterTerms = GetFilterTerms(tbx_includeFilter.Text); // Renamed here

        if (filterTerms.Count == 0)
            return filesToProcess.ToList();

        List<MyFile> files_filtered = new List<MyFile>();

        foreach (DataGridViewRow row in dgv_Files.Rows)
        {
            if (row.DataBoundItem == null) // Ensure row is bound to data
                continue;

            var itemx = (row.DataBoundItem as MyFile);
            if (itemx == null) // no item - skip Next row
                continue;

            if (is_myFile_Matches_Filter(filterTerms, itemx))
                files_filtered.Add(itemx);
        }
        return files_filtered;
    }
    private void ApplyFilenameFilter()
    {
        List<string> filterTerms = GetFilterTerms(tbx_includeFilter.Text); // Renamed here

        if (filterTerms.Count == 0)
        {
            // No filter terms, clear highlighting
            foreach (DataGridViewRow row in dgv_Files.Rows)
            {
                row.DefaultCellStyle.BackColor = SystemColors.Window; // Reset to default background
            }
            return; // Exit if no filter terms
        }

        foreach (DataGridViewRow row in dgv_Files.Rows)
        {
            row.DefaultCellStyle.BackColor = SystemColors.Window; // Reset to default background for all rows initially

            if (row.DataBoundItem == null) // Ensure row is bound to data
                continue;
            
            var itemx = (row.DataBoundItem as MyFile);
            if (itemx == null) // no item - skip Next row
                continue;

            if(is_myFile_Matches_Filter(filterTerms, itemx) )
                row.DefaultCellStyle.BackColor = Color.Yellow; // Highlight row
            
        }
    }

    private static bool is_myFile_Matches_Filter(List<string> filterTerms, MyFile itemx)
    {
        if (!string.IsNullOrWhiteSpace(itemx.FilePath))
        {
            foreach (string filterTerm in filterTerms)
            {
                if (itemx.FilePath.ToLower().Contains(filterTerm.ToLower())) // Case-insensitive contains
                {
                    return true;
                    break; // Found a match, no need to check other filter terms for this row
                }
            }
        }
        return false;
    }

    private void tbx_includeFilter_TextChanged(object sender, EventArgs e) // Renamed here
    {
        ApplyFilenameFilter();
    }


    private void bt_filterExample_Click(object sender, EventArgs e)
    {
        tbx_includeFilter.Text = "example\r\nv2;report\r\nini";
    }



}