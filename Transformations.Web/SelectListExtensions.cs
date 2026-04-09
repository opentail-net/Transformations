using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace Transformations
{
    /// <summary>
    /// Web-specific DataTable extensions that depend on Microsoft.AspNetCore.App.
    /// </summary>
    public static class SelectListExtensions
    {
        /// <summary>
        /// Converts a DataTable to a list of SelectListItems for modern UI dropdowns.
        /// </summary>
        public static List<SelectListItem> ToSelectList(
            this DataTable? dataTable,
            string nameColumn,
            string valueColumn,
            string? selectedValue = null,
            string? selectedName = null)
        {
            if (dataTable == null) return new List<SelectListItem>();

            bool itemAlreadySelected = false;

            // Use LINQ for a cleaner, high-density transformation
            return dataTable.AsEnumerable().Select(row =>
            {
                var text = row[nameColumn]?.ToString() ?? string.Empty;
                var val = row[valueColumn]?.ToString() ?? string.Empty;

                bool isSelected = false;

                // Logical check for selection (Case-insensitive)
                if (!itemAlreadySelected)
                {
                    bool matchesValue = selectedValue != null &&
                                        string.Equals(val, selectedValue, StringComparison.OrdinalIgnoreCase);

                    bool matchesName = selectedName != null &&
                                       string.Equals(text, selectedName, StringComparison.OrdinalIgnoreCase);

                    if (matchesValue || matchesName)
                    {
                        isSelected = true;
                        itemAlreadySelected = true;
                    }
                }

                return new SelectListItem
                {
                    Text = text,
                    Value = val,
                    Selected = isSelected
                };
            }).ToList();
        }
    }
}
