using System.Reflection;

namespace ai_chat_helper_net8
{
    internal static class Form11Helpers
    {







        //_---------------------ddgv helper 
        /// <summary>
        /// Enables or disables double buffering on a Control using reflection.
        /// Works for controls where the DoubleBuffered property is protected (like DataGridView).
        /// </summary>
        /// <param name="control">The control to modify.</param>
        /// <param name="enabled">True to enable double buffering, false to disable.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public static bool SetControlDoubleBuffered(this Control control, bool enabled)
        {
            if (control == null)
            {
                Console.WriteLine("SetControlDoubleBuffered: Input control is null.");
                return false; // Indicate failure
            }

            try
            {
                // Get the type of the Control
                Type type = control.GetType();

                // Get the protected DoubleBuffered property using Reflection
                PropertyInfo? propInfo = type.GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                // Set the DoubleBuffered property to the desired value
                if (propInfo != null)
                {
                    propInfo.SetValue(control, enabled, null);
                    Console.WriteLine($"SetControlDoubleBuffered: Double buffering {(enabled ? "enabled" : "disabled")} for '{control.Name ?? type.Name}'.");
                    return true; // Indicate success
                }
                else
                {
                    Console.WriteLine($"SetControlDoubleBuffered: Could not find DoubleBuffered property for '{control.Name ?? type.Name}'.");
                    return false; // Indicate failure: Property not found
                }
            }
            catch (Exception ex)
            {
                // Log detailed error information
                Console.WriteLine($"SetControlDoubleBuffered: Error setting DoubleBuffered property for '{control.Name ?? control.GetType().Name}': {ex.Message}");
                // Consider more robust logging (e.g., Debug.WriteLine, logging framework)
                // Optionally re-throw or handle specific exceptions if needed
                return false; // Indicate failure due to exception
            }
        }
    }
}