using System;

namespace ai_chat_helper_net8;

public class MyFile // Renamed from FilesToBeProcessed
{
    // Properties are often preferred for DataGridView binding
    public string FilePath { get; set; }
    public string Status { get; set; } // Added Status property

    // Constructor updated to reflect the class name
    public MyFile(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        Status = "Initialized"; // Default status (will be overwritten on drag)
    }

    // Optional: Override ToString for debugging
    public override string ToString()
    {
        return $"{FilePath} ({Status})";
    }
}