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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

		//trigger event
		chk_mergeAll_intoSingleFile.Checked = true;
		chk_mergeAll_intoSingleFile.Checked = false;
	}


	private void Form1_Load(object sender, EventArgs e) { }

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
		dgv_Files.RowHeadersWidth = 22;
		dgv_Files.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

		// --- Define Columns Manually ---
		var statusCol = new DataGridViewTextBoxColumn
		{
			Name = "StatusColumn",
			HeaderText = "Status",
			DataPropertyName = nameof(MyFile.Status), // Binds to the Status property
			AutoSizeMode = DataGridViewAutoSizeColumnMode.None, // Size based on content
			FillWeight = 30 // Give less weight to status column
		};
		dgv_Files.Columns.Add(statusCol);

		var isfilterhitCol = new DataGridViewCheckBoxColumn
		{
			Name = "isfilterhitColumn",
			HeaderText = "is filter hit",
			DataPropertyName = nameof(MyFile.isFilterHit), // Binds to the Status property
			AutoSizeMode = DataGridViewAutoSizeColumnMode.None, // Size based on content
			//FillWeight = 30 // Give less weight to status column
			Width = 50,
		};
		dgv_Files.Columns.Add(isfilterhitCol);

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

		HashSet<MyFile> newlyDiscoveredFiles =
			Get_DragDroppedFiles_toList(
				droppedItems,
			ref currentDropDiscoveredCount,
			ref currentDropIgnoredCount).ToHashSet();

		Console.WriteLine($"--- Discovery phase complete. Found {newlyDiscoveredFiles.Count} potential files. Ignored {currentDropIgnoredCount} duplicates within drop. ---");

		// 2. Add items efficiently from the temporary list to the main BindingList
		if (newlyDiscoveredFiles.Any())
		{
			filesToProcess.RaiseListChangedEvents = false; // Pause DataGridView updates

			var tmp_hashset_filesToProcess = filesToProcess.ToHashSet();
			try
			{
				foreach (var newItem in newlyDiscoveredFiles)
				{
					// Check against EXISTING items in the main filesToProcess list
					//if (!filesToProcess.Any(existing => existing.FilePath.Equals(newItem.FilePath, StringComparison.OrdinalIgnoreCase)))
					bool isAdded = tmp_hashset_filesToProcess.Add(newItem);  // Add the unique item
					if (isAdded)  
					{
						currentDropAddedCount++;         // Increment count of ACTUALLY added files
					}
					else
					{
						currentDropIgnoredCount++;       // Increment ignored count (already in main list)
					}
				}

				//now all distinct . add them all.
				filesToProcess.Clear();
				foreach (var item in tmp_hashset_filesToProcess)
				{
					filesToProcess.Add(item);
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

		//  maybe MOVE This into -- binding changed /datasorce changed events..
		tbx_includeFilter_TextChanged(null, null);// reColor DGV backColor (according to Filter)
	}



	private static List<MyFile> Get_DragDroppedFiles_toList(string[] droppedItems, ref int currentDropDiscoveredCount, ref int currentDropIgnoredCount)
	{
		// ------------------------------------------------

		// Temporary list to hold discovered files before adding to BindingList
		HashSet<MyFile> newlyDiscoveredFiles = new();

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

					//---hashset doesnt need Duplicate check its already Distinct.
					// Check for duplicates within this single drop batch before adding to temp list
					//if (!newlyDiscoveredFiles.Any(f => f.FilePath.Equals(itemPath, StringComparison.OrdinalIgnoreCase)))
					//{
					//	newlyDiscoveredFiles.Add(newFile);
					//	// Console.WriteLine($"Discovered file: {itemPath}"); // Verbose logging
					//}
					//else
					//{
					//	// This path was already found earlier in *this same drop operation* (e.g., dragging file twice)
					//	currentDropIgnoredCount++;
					//	Console.WriteLine($"Duplicate within drop ignored: {itemPath}");
					//}

					bool isAdded = newlyDiscoveredFiles.Add(newFile);
					if (isAdded)
					{
						
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
					var filesInFolder = Directory.EnumerateFiles(itemPath, "*.*", SearchOption.AllDirectories).ToList();
					foreach (string filePath in filesInFolder)
					{
						var newFile = new MyFile(filePath) { Status = "draggedViaMouse" };
						
						bool isAdded = newlyDiscoveredFiles.Add(newFile);
						if (isAdded)
					    // Check for duplicates within this single drop batch before adding to temp list
						//	if (!newlyDiscoveredFiles.Any(f => f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
						{
							//newlyDiscoveredFiles.Add(newFile);
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

		return newlyDiscoveredFiles.ToList();
	}


	/// <summary>
	/// Updates the form's title bar with total file count and details from the last drag-drop operation.
	/// </summary>
	/// <param name="addedLastDrop">Number of unique files added in the most recent drop.</param>
	/// <param name="ignoredLastDrop">Number of duplicate files ignored in the most recent drop.</param>
	private void UpdateFormTitle(int addedLastDrop = 0, int ignoredLastDrop = 0)
	{
		int totalCount = filesToProcess.Count; // Get the current total count

		// Start building the title string
		string title = $"AI CHAT HELPER ( Merge Files  / Convert .html to .md )  --- Total: {totalCount} files";

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

	// --- MERGE---
	private void chk_mergeAll_intoSingleFile_CheckedChanged(object sender, EventArgs e)
	{
		var chksnd = (sender as CheckBox);
		chk_SplitBy_Type.Enabled = chksnd.Checked;
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

	private async Task MergeFilesAsync(string outputDirectory, StringBuilder content, string outputFileName)
	{
		if (content == null || content.Length == 0) return; // Nothing to merge

		string mergedFilePath = Path.Combine(outputDirectory, outputFileName);
		try
		{
			Directory.CreateDirectory(outputDirectory); // Ensure output dir exists
			await File.WriteAllTextAsync(mergedFilePath, content.ToString());
			Console.WriteLine($"Successfully merged content into: {mergedFilePath}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error writing merged file '{mergedFilePath}': {ex.Message}");
			// Optionally show a message box or update status
			MessageBox.Show($"Failed to write merged file:\n{mergedFilePath}\n\nError: {ex.Message}",
							"Merge Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
	private async Task MergeFilesByTypeAsync(string outputDirectory, Dictionary<string, StringBuilder> contentByType)
	{
		if (contentByType == null || contentByType.Count == 0) return; // Nothing to merge

		Directory.CreateDirectory(outputDirectory); // Ensure output dir exists

		foreach (var kvp in contentByType)
		{
			string fileExtension = kvp.Key; // e.g., ".cs", ".json", ".md"
			StringBuilder content = kvp.Value;
			if (content.Length > 0)
			{
				// Construct filename like "merged.cs", "merged.json", "merged.md"
				string mergedFileName = $"merged{fileExtension}";
				string mergedFilePath = Path.Combine(outputDirectory, mergedFileName);
				try
				{
					await File.WriteAllTextAsync(mergedFilePath, content.ToString());
					Console.WriteLine($"Successfully merged {fileExtension} files into: {mergedFilePath}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error writing merged file '{mergedFilePath}': {ex.Message}");
					MessageBox.Show($"Failed to write merged file for type '{fileExtension}':\n{mergedFilePath}\n\nError: {ex.Message}",
								   "Merge Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
	private async void bt_convertToMarkdown_Click(object sender, EventArgs e) // Made async
	{
		string outputDirectory = tbx_outputDir.Text;
		if (string.IsNullOrWhiteSpace(outputDirectory))
		{
			MessageBox.Show("Please select a valid output directory.", "Output Directory Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			tbx_outputDir?.Focus();
			return;
		}

		var allFilesToProcess = GetFiles_matching_includeFilter();

		var tbx_filter_hasText = !string.IsNullOrWhiteSpace(tbx_includeFilter.Text);
		var yourFilter_Chose_Oresult = allFilesToProcess.Count == 0 && tbx_filter_hasText;
		if (yourFilter_Chose_Oresult)
		{
			if (DialogResult.Yes == MessageBox.Show("Your Filter Matches 0 Rows.  Do you want to Select All Rows Instead ?", "Filter Matches No Rows.", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
				allFilesToProcess = GetFiles_ALL();
			else
				return;
		}


		if (!allFilesToProcess.Any())
		{
			MessageBox.Show("No files found in the list to process.  ,", "No Files", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		// --- Determine Active Merge Mode ---
		bool mergeAllChecked = chk_mergeAll_intoSingleFile.Checked;
		bool mergeByTypeChecked = chk_SplitBy_Type.Checked;

		bool performSingleMerge = false;
		bool performTypeMerge = false;
		string activeMergeDescription = "No merge operation selected.";

		if (mergeByTypeChecked) // "Merge by Type" takes precedence if checked (alone or with "Merge All")
		{
			performTypeMerge = true;
			activeMergeDescription = "Merge By Type active.";
		}
		else if (mergeAllChecked) // Only active if "Merge by Type" is NOT checked
		{
			performSingleMerge = true;
			activeMergeDescription = "Merge All (Single File) active.";
		}
		// else: both false, no merge

		// --- Find Common Base Directory ---
		string? commonBaseDirectory = FindCommonBaseDirectory(allFilesToProcess.Select(f => f.FilePath));

		// --- UI Feedback & Initialization ---
		bt_convertToMarkdown.Enabled = false;
		bt_convertToMarkdown.Text = "Processing...";
		this.Cursor = Cursors.WaitCursor;

		// --- Initialize Data Structures based on ACTIVE merge mode ---
		StringBuilder mergedContentAll = performSingleMerge ? new StringBuilder() : null;
		Dictionary<string, StringBuilder> mergedContentByType = performTypeMerge ? new Dictionary<string, StringBuilder>() : null;

		int processedCount = 0;
		int htmlConvertedCount = 0;
		int nonHtmlMergedCount = 0;
		int errorCount = 0;
		string cssSelector = tbx_CssSelector_for_Root.Text;

		// --- Main Processing Loop ---
		try
		{
			Console.WriteLine($"--- Starting Processing ({allFilesToProcess.Count} items) ---");
			Console.WriteLine($"Output Directory: {outputDirectory}");
			Console.WriteLine($"Common Base Input Directory: {commonBaseDirectory ?? "N/A (Outputting Flat)"}");
			Console.WriteLine($"Active Merge Mode: {activeMergeDescription}"); // Log the effective mode

			foreach (var fileInfo in allFilesToProcess.ToList())
			{
				processedCount++;
				fileInfo.Status = "Processing...";

				string inputFilePath = fileInfo.FilePath;
				string fileExtension = Path.GetExtension(inputFilePath).ToLowerInvariant();
				string fileName = Path.GetFileName(inputFilePath);

				try
				{
					string contentToMerge = null;
					string mergeKeyExtension = fileExtension; // Default to original extension for merging
					bool isHtml = (fileExtension == ".htm" || fileExtension == ".html");

					// --- HTML File Processing ---
					if (isHtml)
					{
						string markdownFileName = Path.ChangeExtension(fileName, ".md");
						string finalOutputMarkdownPath;
						string targetSubDir = outputDirectory;

						if (commonBaseDirectory != null)
						{ /* ... (calculate targetSubDir based on relative path as before) ... */
							try
							{
								string? inputFileDir = Path.GetDirectoryName(inputFilePath);
								if (inputFileDir != null)
								{
									string relativePath = Path.GetRelativePath(commonBaseDirectory, inputFileDir);
									if (relativePath != "." && !string.IsNullOrEmpty(relativePath))
									{
										targetSubDir = Path.Combine(outputDirectory, relativePath);
									}
								}
							}
							catch (ArgumentException argEx)
							{
								Console.WriteLine($"Warning: Could not get relative path for {inputFilePath} from {commonBaseDirectory}. Outputting flat. Error: {argEx.Message}");
							}
						}

						finalOutputMarkdownPath = Path.Combine(targetSubDir, markdownFileName);
						Directory.CreateDirectory(targetSubDir);

						bool conversionSuccess = ConvertHtmlToMarkdown(inputFilePath, finalOutputMarkdownPath, cssSelector);

						if (conversionSuccess)
						{
							htmlConvertedCount++;
							fileInfo.Status = "Converted";
							Console.WriteLine($" -> Converted HTML: {inputFilePath} to {finalOutputMarkdownPath}");

							// --- Read CONVERTED Markdown for Merging (if active) ---
							if (performSingleMerge || performTypeMerge)
							{
								try
								{
									contentToMerge = await File.ReadAllTextAsync(finalOutputMarkdownPath);
									mergeKeyExtension = ".md"; // Use .md extension for the merge key/grouping
								}
								catch (IOException readEx)
								{
									Console.WriteLine($" -> Error reading converted file for merging '{finalOutputMarkdownPath}': {readEx.Message}");
									fileInfo.Status = "Converted (Merge Read Error)";
									errorCount++; // Count this as an error
									contentToMerge = null; // Ensure it's not added to merge data
								}
							}
						}
						else
						{
							errorCount++;
							fileInfo.Status = "Error: Conversion Failed";
							Console.WriteLine($" -> Failed conversion (handled): {inputFilePath}");
						}
					}
					// --- Non-HTML File Processing ---
					else
					{
						fileInfo.Status = "Skipped (Type)"; // Default status
															// --- Read ORIGINAL content for Merging (if active) ---
						if (performSingleMerge || performTypeMerge)
						{
							try
							{
								contentToMerge = await File.ReadAllTextAsync(inputFilePath);
								fileInfo.Status = "Read for Merging"; // Update status
								nonHtmlMergedCount++;
								// mergeKeyExtension remains the original fileExtension
							}
							catch (IOException readEx)
							{
								Console.WriteLine($" -> Error reading non-HTML file for merging '{inputFilePath}': {readEx.Message}");
								fileInfo.Status = "Error: Merge Read Failed";
								errorCount++;
								contentToMerge = null; // Ensure it's not added
							}
						}
						else // Not merging
						{
							Console.WriteLine($" -> Skipped (Non-HTML, No Merge): {inputFilePath}");
						}
					}

					// --- Append Content to Merge Data (if applicable and content exists) ---
					if (contentToMerge != null) // Only add if read successfully
					{
						string header;
						if (isHtml && mergeKeyExtension == ".md")
						{
							header = $"--- File: {fileName} (Converted to Markdown) ---";
						}
						else
						{
							header = $"--- File: {fileName} ---";
						}


						if (performSingleMerge && mergedContentAll != null)
						{
							mergedContentAll.AppendLine(header);
							mergedContentAll.AppendLine(contentToMerge);
							mergedContentAll.AppendLine();
						}
						else if (performTypeMerge && mergedContentByType != null) // Use 'else if' as modes are exclusive
						{
							if (!mergedContentByType.TryGetValue(mergeKeyExtension, out var sb))
							{
								sb = new StringBuilder();
								mergedContentByType[mergeKeyExtension] = sb;
							}
							sb.AppendLine(header);
							sb.AppendLine(contentToMerge);
							sb.AppendLine();
						}
					}
				}
				catch (Exception ex) // Catch errors during processing of a single file
				{
					errorCount++;
					fileInfo.Status = $"Error: {ex.GetType().Name}";
					Console.WriteLine($" FAILED (Unexpected) processing {inputFilePath}: {ex.Message}");
				}

			} // End foreach loop

			Console.WriteLine($"--- File Processing Phase Complete. Processed: {processedCount}, HTML Converted: {htmlConvertedCount}, Non-HTML Read for Merge: {nonHtmlMergedCount}, Errors: {errorCount} ---");

			// --- Perform Merging AFTER loop based on ACTIVE flags ---
			string mergeSummary = "No merge operation performed.";
			if (performSingleMerge && mergedContentAll != null && mergedContentAll.Length > 0)
			{
				await MergeFilesAsync(outputDirectory, mergedContentAll, "merged_all_files.txt");
				mergeSummary = $"Merged all processed files into 'merged_all_files.txt'.";
			}
			else if (performTypeMerge && mergedContentByType != null && mergedContentByType.Values.Any(sb => sb.Length > 0))
			{
				await MergeFilesByTypeAsync(outputDirectory, mergedContentByType);
				mergeSummary = $"Merged files by type (e.g., 'merged.cs', 'merged.md', ...).";
			}
			else if (performSingleMerge || performTypeMerge) // A merge was selected but resulted in no content
			{
				mergeSummary = "Merge operation selected, but no content was available or read successfully to merge.";
				Console.WriteLine(mergeSummary);
			}


			// --- Final Summary ---
			string summary = $"Processing finished.\n\n" +
							 $"Total Files Processed: {processedCount}\n" +
							 $"HTML Files Converted: {htmlConvertedCount}\n" +
							 $"Non-HTML Files Read (for merge): {nonHtmlMergedCount}\n" +
							 $"Errors: {errorCount}\n\n" +
							 $"{mergeSummary}";

			this.Text = $" {this.Text.Split(" --> ")[0]} --> Done. at {DateTime.Now.ToLongTimeString()} --> {summary.ReplaceLineEndings(" ; ")}";
		}
		catch (Exception ex) // Catch unexpected errors during the overall process
		{
			MessageBox.Show($"An unexpected error occurred during the process:\n\n{ex.Message}", "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Console.WriteLine($"FATAL ERROR during processing: {ex.ToString()}");
		}
		finally
		{
			bt_convertToMarkdown.Enabled = true;
			bt_convertToMarkdown.Text = "Convert / Process";
			this.Cursor = Cursors.Default;
			// Consider refreshing your file list UI here
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





	// --- Delete Button Logic ---

	private void bt_RemoveFilterHitRows_Click(object sender, EventArgs e)
	{
		////no row selected
		//if (dgv_Files.SelectedRows.Count == 0)
		//	return;

		// Get the list of selected MyFile objects for removal
		var selectedFiles = dgv_Files.Rows
			.Cast<DataGridViewRow>()
			.Select(row => row.DataBoundItem as MyFile)
			.Where(file => file != null) // Filter out any nulls.
			.Where(file => (file?.isFilterHit ?? false)  == true  ) // text filter matched..
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

		//update filter 
		tbx_includeFilter_TextChanged(null, null);// reColor DGV backColor (according to Filter)
	}


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

		//update filter 
		tbx_includeFilter_TextChanged(null, null);// reColor DGV backColor (according to Filter)

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
			string[] lineTerms = line.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
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
	private List<MyFile> GetFiles_ALL() => filesToProcess.ToList();


	private void ApplyFilenameFilter_OLD()
	{
		List<string> filterTerms = GetFilterTerms(tbx_includeFilter.Text); // Renamed here

		if (filterTerms.Count == 0)
		{
			// No filter terms, clear highlighting
			foreach (DataGridViewRow row in dgv_Files.Rows)
			{
				row.DefaultCellStyle.BackColor = SystemColors.Window; // Reset to default background
				(row.DataBoundItem as MyFile).isFilterHit = false;
			}
			return; // Exit if no filter terms
		}

		foreach (DataGridViewRow row in dgv_Files.Rows)
		{
			row.DefaultCellStyle.BackColor = SystemColors.Window; // Reset to default background for all rows initially
			(row.DataBoundItem as MyFile).isFilterHit = false;

			if (row.DataBoundItem == null) // Ensure row is bound to data
				continue;

			var itemx = (row.DataBoundItem as MyFile);
			if (itemx == null) // no item - skip Next row
				continue;

			if (is_myFile_Matches_Filter(filterTerms, itemx))
			{
				row.DefaultCellStyle.BackColor = Color.Yellow; // Highlight row
			   (row.DataBoundItem as MyFile).isFilterHit = true;

			}


		}
	}
	private void ApplyFilenameFilter()
	{
		// clear highlighting
		foreach (DataGridViewRow row in dgv_Files.Rows)
		{
			row.DefaultCellStyle.BackColor = SystemColors.Window; // Reset to default background
			(row.DataBoundItem as MyFile).isFilterHit = false;
		}

		List<string> filterTerms = GetFilterTerms(tbx_includeFilter.Text); // Renamed here
		if (filterTerms.Count == 0)
			return; // Exit if no filter terms


		foreach (DataGridViewRow row in dgv_Files.Rows)
		{
			if (row.DataBoundItem == null) // Ensure row is bound to data
				continue;

			var itemx = (row.DataBoundItem as MyFile);
			if (itemx == null) // no item - skip Next row
				continue;

			if (is_myFile_Matches_Filter(filterTerms, itemx))
			{
				row.DefaultCellStyle.BackColor = Color.Yellow; // Highlight row
				(row.DataBoundItem as MyFile).isFilterHit = true;
			}


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

	private void tbx_includeFilter_TextChanged(object sender, EventArgs e) => ApplyFilenameFilter();
	private void bt_ClearFilterTbx_Click(object sender, EventArgs e) => tbx_includeFilter.Text = "";

	

}