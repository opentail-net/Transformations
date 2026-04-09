using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

/// <summary>
/// Enumerator Convertor Class.
/// </summary>
public static class CollectionConvertor
{
    #region Enumerations

    /// <summary>
    /// The object types.
    /// </summary>
    internal enum ObjectTypes
    {
        /// <summary>
        /// The string type.
        /// </summary>
        StringType,

        /// <summary>
        /// The DateTime type.
        /// </summary>
        DateTimeType,

        /// <summary>
        /// The integer 16 type.
        /// </summary>
        Int16Type,

        /// <summary>
        /// the integer 32 type.
        /// </summary>
        Int32Type,

        /// <summary>
        /// The integer 64 type.
        /// </summary>
        Int64Type,

        /// <summary>
        /// The double type.
        /// </summary>
        DoubleType,

        /// <summary>
        /// The byte type.
        /// </summary>
        ByteType,

        /// <summary>
        /// The s byte type.
        /// </summary>
        SByteType,

        /// <summary>
        /// The boolean type.
        /// </summary>
        BooleanType,

        /// <summary>
        /// The char type.
        /// </summary>
        CharType,

        /// <summary>
        /// The GUID type.
        /// </summary>
        GuidType,

        /// <summary>
        /// The decimal type.
        /// </summary>
        DecimalType
    }

    #endregion Enumerations

    #region Methods

    /// <summary>
    /// Converts a collection to an array.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <param name="ignoreNullElements">if set to <c>true</c> [ignore null elements].</param>
    /// <param name="ignoreEmptyElements">if set to <c>true</c> [ignore empty elements].</param>
    /// <returns>
    /// The array of the destination type.
    /// </returns>
    public static T[] ConvertToArray<T>(this string valueList, string stringSplitter = ",", bool ignoreNullElements = true, bool ignoreEmptyElements = true)
        where T : struct, IComparable<T>
    {
        return valueList.ConvertToList<T>(stringSplitter, ignoreNullElements, ignoreEmptyElements).ToArray();
    }

    // Split the string into collection...

    /// <summary>
    /// Converts to string array.
    /// </summary>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <returns>The string array.</returns>
    public static string[] ConvertToArrayOfString(this string valueList, string stringSplitter = ",")
    {
        if (string.IsNullOrEmpty(valueList))
        {
            return new string[] { };
        }
        else if (string.IsNullOrEmpty(stringSplitter))
        {
            return new string[] { valueList };
        }
        else if (valueList.Contains(stringSplitter, StringComparison.Ordinal))
        {
            return valueList.Split(new[] { stringSplitter }, StringSplitOptions.None);
        }
        else
        {
            return new string[] { valueList };
        }
    }

    /// <summary>
    /// Converts a DataTable into a Bootstrap-formatted HTML list of anchor links.
    /// </summary>
    /// <param name="dataTable">The source data table.</param>
    /// <param name="nameOfNameColumn">The column used for the link text.</param>
    /// <param name="nameOfValueColumn">The column used for the href attribute.</param>
    /// <returns>An HTML string containing list items.</returns>
    public static string ConvertToBootstrapOptionList(this DataTable dataTable, string nameOfNameColumn, string nameOfValueColumn)
    {
        // Null Guard
        if (dataTable == null || string.IsNullOrEmpty(nameOfNameColumn) || string.IsNullOrEmpty(nameOfValueColumn))
        {
            return string.Empty;
        }

        StringBuilder stringBuilder = new StringBuilder();

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            DataRow dataRow = dataTable.Rows[i];

            // Accessing DataRow by column name can return DBNull.Value. 
            // We use '!' to suppress null warnings because we handle the empty check immediately after.
            string? name = dataRow[nameOfNameColumn]?.ToString();
            string? value = dataRow[nameOfValueColumn]?.ToString();

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            {
                // WebUtility.HtmlEncode prevents XSS if the DB contains malicious strings.
                string safeName = WebUtility.HtmlEncode(name!);
                string safeValue = WebUtility.HtmlEncode(value!);

                stringBuilder.AppendLine($"<li><a href='{safeValue}'>{safeName}</a></li>");
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Converts a collection of name-value pairs into a Bootstrap-formatted HTML list of anchor links.
    /// </summary>
    /// <param name="options">An enumerable collection of tuples containing the display Name and the link Value.</param>
    /// <returns>
    /// A string containing HTML list items (li) with nested anchor tags (a). 
    /// Returns an empty string if the input collection is null or empty.
    /// </returns>
    /// <remarks>
    /// This method performs HTML encoding on both Name and Value to prevent Cross-Site Scripting (XSS) attacks.
    /// It is a high-performance alternative to DataTable-based HTML generation.
    /// </remarks>
    public static string ToBootstrapOptionList(this IEnumerable<(string Name, string Value)> options)
    {
        // High-Visibility Guard
        if (options == null) return string.Empty;

        var sb = new StringBuilder();

        foreach (var (name, value) in options)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                continue;

            // Security: HTML Encode to prevent XSS
            string safeName = WebUtility.HtmlEncode(name);
            string safeValue = WebUtility.HtmlEncode(value);

            // Efficiency: Direct buffer writing via interpolation
            sb.AppendLine($"<li><a href='{safeValue}'>{safeName}</a></li>");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts data table to the data set.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="dataSetName">Name of the data set.</param>
    /// <returns>The data set.</returns>
    public static DataSet ConvertToDataSet(this DataTable dataTable, string? dataSetName = null)
    {
        if (string.IsNullOrEmpty(dataSetName))
        {
            if (!string.IsNullOrEmpty(dataTable.TableName))
            {
                dataSetName = dataTable.TableName;
            }
            else
            {
                dataSetName = "DataSet";
            }
        }

        DataSet dataSet = new DataSet(dataSetName);
        dataSet.Tables.Add(dataTable);
        return dataSet;
    }

    /// <summary>
    /// Convert data set to the data table.
    /// </summary>
    /// <param name="dataSet">The data set.</param>
    /// <param name="dataTableId">The data table identifier.</param>
    /// <returns>The data table.</returns>
    public static DataTable ConvertToDataTable(this DataSet dataSet, int? dataTableId = null)
    {
        DataTable result;
        dataSet.TryToDataTable(out result, dataTableId);
        return result;
    }

    /// <summary>
    /// Convert data set to the data table.
    /// </summary>
    /// <param name="dataSet">The data set.</param>
    /// <param name="dataTableName">Name of the data table.</param>
    /// <returns>The data table.</returns>
    public static DataTable ConvertToDataTable(this DataSet dataSet, string? dataTableName = null)
    {
        DataTable result;
        dataSet.TryToDataTable(out result, dataTableName);
        return result;
    }

    /// <summary>
    /// The list of generic data objects to data table.
    /// </summary>
    /// <typeparam name="T">
    /// The generic type T.
    /// </typeparam>
    /// <param name="data">
    /// The set of data to convert.
    /// </param>
    /// <param name="groupGuidName">
    /// Name of the external GUID.
    /// </param>
    /// <param name="groupGuid">
    /// The header GUID.
    /// </param>
    /// <returns>
    /// The <see cref="DataTable"/>.
    /// </returns>
    public static DataTable ConvertToDataTable<T>(this ISet<T> data, string? groupGuidName = null, Guid? groupGuid = null)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
        DataTable table = new DataTable();
        foreach (PropertyDescriptor prop in properties)
        {
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }

        if (groupGuid != null && !string.IsNullOrEmpty(groupGuidName))
        {
            if (!table.Columns.Contains(groupGuidName))
            {
                table.Columns.Add(groupGuidName, groupGuid.GetType());
            }

            if (data != null)
            {
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }

                    row[groupGuidName] = groupGuid;
                    table.Rows.Add(row);
                }
            }
        }
        else
        {
            if (data != null)
            {
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }

                    table.Rows.Add(row);
                }
            }
        }

        return table;
    }

    /// <summary>
    /// Converts string to IEnumerable collection.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <param name="ignoreNullElements">if set to <c>true</c> [ignore null elements].</param>
    /// <param name="ignoreEmptyElements">if set to <c>true</c> [ignore empty elements].</param>
    /// <returns>
    /// An IEnumerable collection of the destination type.
    /// </returns>
    public static IEnumerable<T> ConvertToEnumerable<T>(this string valueList, string stringSplitter = ",", bool ignoreNullElements = true, bool ignoreEmptyElements = true)
        where T : struct, IComparable<T>
    {
        return valueList.ConvertToList<T>(stringSplitter, ignoreNullElements, ignoreEmptyElements);
    }

    /// <summary>
    /// Converts to enumerable.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TU">The destination type.</typeparam>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <param name="ignoreNullElements">if set to <c>true</c> [ignore null elements].</param>
    /// <returns>The converted collection.</returns>
    public static IEnumerable<TU> ConvertToEnumerable<T, TU>(this IList<T> valueList, string stringSplitter = ",", bool ignoreNullElements = true)
        where T : struct, IComparable<T>
        where TU : struct, IComparable<TU>
    {
        IEnumerable<TU> result = valueList.ConvertToList<T, TU>(stringSplitter, ignoreNullElements);
        return result;
    }

    //// ********************************************************************************************************
    //// Data Table Conversions
    //// ********************************************************************************************************

    /// <summary>
    /// Converts DataTable into a Hash Set.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="dataTable">The data table.</param>
    /// <returns>The hash set.</returns>
    /// <remarks>
    /// ****************************************************************************************************************
    /// TODO: This particular method does NOT use the conversions defined in BasicTypeConverter at the moment.
    /// ****************************************************************************************************************
    /// The assumption is that the data table has the same column names as the object type you are converting to.
    /// For more complex conversions, use XSLT.
    /// </remarks>
    public static HashSet<T> ConvertToHashSet<T>(this DataTable dataTable)
        where T : new()
    {
        HashSet<T> returnHashset = new HashSet<T>();
        if (dataTable.Rows.Count == 0)
        {
            return returnHashset;
        }

        Type componentType = typeof(T);
        string propName = string.Empty;
        //// componentType.BaseType
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(componentType);

        //// get this list of properties
        List<PropertyDescriptor> propertyDescriptorList = properties.Cast<PropertyDescriptor>().ToList();

        //// WAS:
        //// foreach (PropertyDescriptor prop in properties)
        //// {
        ////    propertyDescriptorList.Add(prop);
        //// }

        // get this list of properties of base object, if it's there
        if (componentType.BaseType?.FullName?.EndsWith("Base", StringComparison.Ordinal) == true)
        {
            PropertyDescriptorCollection baseProperties = TypeDescriptor.GetProperties(componentType.BaseType);
            propertyDescriptorList.AddRange(baseProperties.Cast<PropertyDescriptor>());

            //// WAS:
            //// foreach (PropertyDescriptor prop in baseProperties)
            //// {
            ////    propertyDescriptorList.Add(prop);
            //// }
        }

        // filter to only contain the properties which are in the table.
        List<PropertyDescriptor> propertyDescriptorList2 = new List<PropertyDescriptor>();
        List<string> propertyDescriptorNameList2 = new List<string>();

        foreach (PropertyDescriptor prop in propertyDescriptorList)
        {
            propName = prop.Name;
            if (dataTable.Columns[propName] != null)
            {
                if (!propertyDescriptorNameList2.Contains(propName.ToUpper(CultureInfo.GetCultureInfo("en-GB"))))
                {
                    propertyDescriptorNameList2.Add(propName.ToUpper(CultureInfo.GetCultureInfo("en-GB")));
                    propertyDescriptorList2.Add(prop);
                }
            }
        }

        // combine into PropertyDescriptorCollection
        PropertyDescriptorCollection jointProperties = new PropertyDescriptorCollection(propertyDescriptorList2.ToArray());

        // populate the hashset
        if (jointProperties.Count > 0)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                T obj = new T();
                foreach (PropertyDescriptor prop in jointProperties)
                {
                    propName = prop.Name;

                    var value = row[propName];
                    if (value != DBNull.Value)
                    {
                        prop.SetValue(obj, value);
                    }
                }

                returnHashset.Add(obj);
            }
        }

        return returnHashset;
    }

    /// <summary>
    /// Converts to list of the destination type.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <param name="ignoreNullElements">if set to <c>true</c> [ignore null elements].</param>
    /// <param name="ignoreEmptyElements">if set to <c>true</c> [ignore empty elements].</param>
    /// <returns>
    /// A list of the destination type.
    /// </returns>
    /// <remarks>
    /// Ideally should convert this to a TRY method, that can return exception is needed.
    /// </remarks>
    public static List<T> ConvertToList<T>(this string valueList, string stringSplitter = ",", bool ignoreNullElements = true, bool ignoreEmptyElements = true)
        where T : struct, IComparable<T>
    {
        List<T> result = new List<T>();
        foreach (string item in valueList.ConvertToArrayOfString(stringSplitter))
        {
            if (ignoreNullElements && item == null)
            {
                continue;
            }

            if (ignoreEmptyElements && item == string.Empty)
            {
                continue;
            }

            result.Add(item.ConvertTo<T>());
        }

        return result;
    }

    /// <summary>
    /// The convert to list.
    /// </summary>
    /// <param name="originalCollection">
    /// The dictionary.
    /// </param>
    /// <typeparam name="TK">
    /// The type of the key.
    /// </typeparam>
    /// <typeparam name="TV">
    /// The type to convert to.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IList"/>.
    /// </returns>
    public static IList<TV> ConvertToList<TK, TV>(this IDictionary<TK, TV> originalCollection)
        where TK : struct, IComparable<TK>
        where TV : struct, IComparable<TV>
    {
        return originalCollection.Values.ToList();
    }

    /// <summary>
    /// The convert to list.
    /// </summary>
    /// <param name="originalCollection">
    /// The original collection.
    /// </param>
    /// <typeparam name="T">
    /// The type to convert.
    /// </typeparam>
    /// <returns>
    /// The <see cref="IList"/>.
    /// </returns>
    public static IList<T> ConvertToList<T>(this IEnumerable<T> originalCollection)
        where T : struct, IComparable<T>
    {
        return originalCollection.ToList();
    }

    /// <summary>
    /// Converts a list of the source type to a list of the destination type.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TU">The destination type.</typeparam>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <param name="ignoreNullElements">if set to <c>true</c> [ignore null elements].</param>
    /// <returns>The converted list of destination type.</returns>
    public static List<TU> ConvertToList<T, TU>(this IEnumerable<T> valueList, string stringSplitter = ",", bool ignoreNullElements = true)
        where T : struct, IComparable<T>
        where TU : struct, IComparable<TU>
    {
        List<TU> result = new List<TU>();

        if (typeof(T) == typeof(bool))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((bool)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(byte))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((byte)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(char))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((char)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(DateTime))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((DateTime)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(decimal))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((decimal)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(double))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((double)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(short))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((short)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(ushort))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((ushort)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(int))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((int)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(uint))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((uint)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(long))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((long)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(ulong))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((ulong)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(float))
        {
            foreach (object item in valueList)
            {
                TU availableValue = ((float)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(bool?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((bool?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(byte?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((byte?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(char?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((char?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(DateTime?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((DateTime?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(decimal?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((decimal?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(double?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((double?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(short?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((short?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(ushort?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((ushort?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(int?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((int?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(uint?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((uint?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(long?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((long?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(ulong?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((ulong?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }
        else if (typeof(T) == typeof(float?))
        {
            foreach (object item in valueList)
            {
                if (item == null && ignoreNullElements)
                {
                    continue;
                }

                TU availableValue = ((float?)item).ConvertTo<TU>();
                result.Add(availableValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Converts DataTable into a List.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="dataTable">The data table.</param>
    /// <param name="typeTestAndCorrection">if set to <c>true</c> [type test and correction].</param>
    /// <returns>
    /// The list.
    /// </returns>
    /// <remarks>
    /// ****************************************************************************************************************
    /// TODO: This particular method does NOT use the conversions defined in BasicTypeConverter at the moment.
    /// ****************************************************************************************************************
    /// The assumption is that the data table has the same column names as the object type you are converting to.
    /// For more complex conversions, use XSLT.
    /// </remarks>
    public static List<T> ConvertToList<T>(this DataTable dataTable, bool typeTestAndCorrection = true)
        where T : new()
    {
        List<T> returnList = new List<T>();
        if (dataTable.Rows.Count == 0)
        {
            return returnList;
        }

        Type componentType = typeof(T);
        string propName = string.Empty;
        //// componentType.BaseType
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(componentType);

        //// get this list of properties
        List<PropertyDescriptor> propertyDescriptorList = properties.Cast<PropertyDescriptor>().Where(prop => dataTable.Columns[prop.Name] != null).ToList();

        //// Note: Above LINQ previously WAS foreach, as below:
        //// foreach (PropertyDescriptor prop in properties)
        //// {
        ////    if (dataTable.Columns[prop.Name] != null)
        ////    {
        ////        propertyDescriptorList.Add(prop);
        ////    }
        //// }

        // get this list of properties of base object, if it's there
        if (componentType.BaseType?.FullName?.EndsWith("Base", StringComparison.Ordinal) == true)
        {
            PropertyDescriptorCollection baseProperties = TypeDescriptor.GetProperties(componentType.BaseType);
            propertyDescriptorList.AddRange(baseProperties.Cast<PropertyDescriptor>().Where(prop => dataTable.Columns[prop.Name] != null));

            //// Note: Above LINQ previously WAS foreach, as below:
            //// foreach (PropertyDescriptor prop in baseProperties)
            //// {
            ////    if (dataTable.Columns[prop.Name] != null)
            ////    {
            ////        propertyDescriptorList.Add(prop);
            ////    }
            //// }
        }

        // filter to only contain the properties which are in the table.
        List<PropertyDescriptor> propertyDescriptorList2 = new List<PropertyDescriptor>();
        List<string> propertyDescriptorNameList2 = new List<string>();

        foreach (PropertyDescriptor prop in propertyDescriptorList)
        {
            propName = prop.Name;
            if (dataTable.Columns[propName] != null)
            {
                if (!propertyDescriptorNameList2.Contains(propName.ToUpper(CultureInfo.GetCultureInfo("en-GB"))))
                {
                    propertyDescriptorNameList2.Add(propName.ToUpper(CultureInfo.GetCultureInfo("en-GB")));
                    propertyDescriptorList2.Add(prop);
                }
            }
        }

        // *******************************************************************
        // If required, do a quick test that the column values can be mapped.
        // If they can't be mapped, add them to an "exclusion" list;
        // This is so that fields with erraneous values did not keep failing over and over (which will slow us down)
        // We only want to process the columns and values that can be converted.
        // *******************************************************************
        List<string> propertyToRemove = new List<string>();

        if (typeTestAndCorrection)
        {
            if (propertyDescriptorList2.Count > 0)
            {
                int maxTest = 10;

                if (dataTable.Rows.Count - 1 < maxTest)
                {
                    maxTest = dataTable.Rows.Count - 1;
                }

                for (int rowi = 0; rowi < maxTest; rowi++)
                {
                    DataRow row = dataTable.Rows[rowi];

                    foreach (PropertyDescriptor prop in propertyDescriptorList2)
                    {
                        object value = row[prop.Name];

                        T obj = new T();

                        try
                        {
                            // Check if it's a nullable type first.
                            Type? nullableType = Nullable.GetUnderlyingType(prop.PropertyType);

                            if (nullableType != null)
                            {
                                prop.SetValue(obj, Convert.ChangeType(value, nullableType));
                            }
                            else
                            {
                                prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                            }
                        }
                        catch
                        {
                            if (!propertyToRemove.Contains(prop.Name))
                            {
                                propertyToRemove.Add(prop.Name);
                            }
                        }
                    }
                }
            }
        }

        // ************************************************************************
        // Apply the exclusion list, keeping only the properties we can handle.
        // ************************************************************************
        List<PropertyDescriptor> propertyDescriptorList3 = new List<PropertyDescriptor>();

        foreach (PropertyDescriptor prop in propertyDescriptorList2)
        {
            if (!propertyToRemove.Contains(prop.Name))
            {
                propertyDescriptorList3.Add(prop);
            }
        }

        // **********************************************************
        // Create PropertyDescriptorCollection from the list.
        // **********************************************************
        PropertyDescriptorCollection jointProperties = new PropertyDescriptorCollection(propertyDescriptorList3.ToArray());

        // **********************************************************
        // Create the object list.
        // **********************************************************
        foreach (DataRow row in dataTable.Rows)
        {
            T obj = new T();
            for (int i = 0; i < jointProperties.Count; i++)
            {
                PropertyDescriptor prop = jointProperties[i];
                object value = row[prop.Name];

                try
                {
                    if (!Convert.IsDBNull(value))
                    {
                        Type? nullableType = Nullable.GetUnderlyingType(prop.PropertyType);

                        if (nullableType != null)
                        {
                            prop.SetValue(obj, Convert.ChangeType(value, nullableType));
                        }
                        else
                        {
                            prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                        }
                    }
                }
                catch
                {
                }
            }

            returnList.Add(obj);
        }

        return returnList;
    }

    /// <summary>
    /// Converts to string list.
    /// </summary>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <returns>
    /// A list of the destination type.
    /// </returns>
    public static List<string> ConvertToListOfString(this string valueList, string stringSplitter = ",")
    {
        return new List<string>(valueList.ConvertToArrayOfString(stringSplitter));
    }

    /// <summary>
    /// Converts data table to list of string.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="includeColumnNamesAsHeader">if set to <c>true</c> [include column names as a header].</param>
    /// <returns>The list of strings.</returns>
    public static List<string> ConvertToListOfString(this DataTable dataTable, bool includeColumnNamesAsHeader = false)
    {
        List<string> stringList = new List<string>();

        if (dataTable == null)
        {
            return stringList;
        }

        if (includeColumnNamesAsHeader)
        {
            string header = string.Empty;

            foreach (DataColumn column in dataTable.Columns)
            {
                if (!string.IsNullOrEmpty(header))
                {
                    header += ",";
                }

                header += column.ColumnName;
            }

            stringList.Add(header);
        }

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            stringList.Add(dataTable.Rows[i].ItemArray.ConvertToString(","));
        }

        return stringList;
    }

    //// ********************************************************************************************************
    //// List And Array Conversions
    //// ********************************************************************************************************

    // Join the collection into a string...

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <returns>The concatenated string.</returns>
    public static string ConvertToString<T>(this IEnumerable<T> valueList, string stringSplitter = ",")
    {
        string result = string.Empty;

        foreach (var item in valueList)
        {
            if (item == null)
                continue;
            else if (item.ToString() == string.Empty)
                continue;

            if (result.Length > 0)
                result += stringSplitter;

            result += item!.ToString();
        }

        return result;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <param name="valueList">
    /// The value list.
    /// </param>
    /// <param name="stringSplitter">
    /// The string splitter.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string ConvertToString(this object?[] valueList, string stringSplitter = ",")
    {
        string result = string.Empty;

        foreach (var item in valueList)
        {
            if (item == null)
                continue;
            else if (item.ToString() == string.Empty)
                continue;

            if (result.Length > 0)
                result += stringSplitter;

            result += item!.ToString()!;
        }

        return result;
    }

    /*
    /// <summary>
    /// Converts to separated string.
    /// </summary>
    /// <param name="itemCollection">The item connection.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <returns>The separated string.</returns>
    /// <remarks>todo: add ignoreNullElements & ignoreEmptyElements</remarks>
    public static string ConvertToString(this object[] itemCollection, string stringSplitter = ",")
    {
        if (itemCollection == null || itemCollection.Length == 0)
        {
            return string.Empty;
        }
        else
        {
            return string.Join(stringSplitter, itemCollection);
        }
    }
    */

    /// <summary>
    /// Converts to separated string.
    /// </summary>
    /// <param name="itemCollection">The item connection.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <returns>The separated string.</returns>
    /// <remarks>todo: add ignoreNullElements and ignoreEmptyElements</remarks>
    public static string ConvertToString(this IEnumerable<IEnumerable> itemCollection, string stringSplitter = ",")
    {
        if (itemCollection == null || !itemCollection.Any())
        {
            return string.Empty;
        }
        else
        {
            return string.Join(stringSplitter, itemCollection);
        }
    }

    /// <summary>
    /// Converts to separated string.
    /// </summary>
    /// <param name="itemCollection">The item connection.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <returns>
    /// The separated string.
    /// </returns>
    public static string? ConvertToString(this IList itemCollection, string stringSplitter = ",")
    {
        if (itemCollection == null || itemCollection.Count == 0)
        {
            return string.Empty;
        }
        else
        {
            string result = string.Empty;
            foreach (var item in itemCollection)
            {
                if (item == null)
                    continue;
                else if (item.ToString() == string.Empty)
                    continue;

                if (result.Length > 0)
                    result += stringSplitter;

                result += item.ToString() ?? string.Empty;
            }

            return result;
        }
    }

    /// <summary>
    /// Converts to separated string.
    /// </summary>
    /// <param name="itemCollection">The item connection.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static string ConvertToString(this IDictionary itemCollection, string stringSplitter = ",")
    {
        if (itemCollection == null || itemCollection.Count == 0)
        {
            return string.Empty;
        }
        else
        {
            string result = string.Empty;
            foreach (var item in itemCollection)
            {
                if (item == null)
                    continue;
                else if (item.ToString() == string.Empty)
                    continue;

                if (result.Length > 0)
                    result += stringSplitter;

                result += item.ToString() ?? string.Empty;
            }

            return result;
        }
    }

    /// <summary>
    /// Converts a list of source type to a list of string type.
    /// </summary>
    /// <typeparam name="T">The type to use.</typeparam>
    /// <param name="valueList">The value list.</param>
    /// <param name="stringSplitter">The string splitter.</param>
    /// <param name="ignoreNullElements">if set to <c>true</c> [ignore null elements].</param>
    /// <returns>A list of string type.</returns>
    /// <remarks>Can't be done as part of ConvertToList.</remarks>
    public static List<string> ConvertToStringList<T>(this IEnumerable<T> valueList, string stringSplitter = ",", bool ignoreNullElements = true)
        where T : struct, IComparable<T>
    {
        List<string> result = new List<string>();

        foreach (object item in valueList)
        {
            if (ignoreNullElements)
            {
                if ((item is string) || (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    if (item == null)
                    {
                        continue;
                    }
                }
            }

            string? availableValue = item.ToString();
            if (availableValue != null)
                result.Add((string)availableValue);
        }

        return result;
    }

    /// <summary>
    /// Converts double to double array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static double[] ToArray(this double value, params double[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts char to char array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static char[] ToArray(this char value, params char[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts <see cref="sbyte"/> to <see cref="sbyte"/> array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static sbyte[] ToArray(this sbyte value, params sbyte[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts byte to byte array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static byte[] ToArray(this byte value, params byte[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts <see cref="bool"/> to <see cref="bool"/> array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static bool[] ToArray(this bool value, params bool[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts DateTimeOffset to DateTimeOffset array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static DateTimeOffset[] ToArray(this DateTimeOffset value, params DateTimeOffset[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts decimal to decimal array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static decimal[] ToArray(this decimal value, params decimal[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts DateTime to DateTime array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static DateTime[] ToArray(this DateTime value, params DateTime[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts float to float array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static float[] ToArray(this float value, params float[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts <see cref="ushort"/> to <see cref="ushort"/> array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static ushort[] ToArray(this ushort value, params ushort[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts <see cref="short"/> to <see cref="short"/> array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static short[] ToArray(this short value, params short[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts long to long array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static long[] ToArray(this long value, params long[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts <see cref="ulong"/> to <see cref="ulong"/> array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static ulong[] ToArray(this ulong value, params ulong[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts <see cref="uint"/> to <see cref="uint"/> array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static uint[] ToArray(this uint value, params uint[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts <see cref="int"/> to <see cref="int"/> array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static int[] ToArray(this int value, params int[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts Uri to Uri array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Uri[] ToArray(this Uri value, params Uri[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts <see cref="Guid"/> to <see cref="Guid"/> array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Guid[] ToArray(this Guid value, params Guid[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts TimeSpan to TimeSpan array.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static TimeSpan[] ToArray(this TimeSpan value, params TimeSpan[] additionalValues)
    {
        return value.ToList(additionalValues).ToArray();
    }

    /// <summary>
    /// Converts double to double list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<double> ToList(this double value, params double[] additionalValues)
    {
        List<double> valueList = new List<double>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (double newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts char to char list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<char> ToList(this char value, params char[] additionalValues)
    {
        List<char> valueList = new List<char>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (char newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts <see cref="sbyte"/> to <see cref="sbyte"/> list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<sbyte> ToList(this sbyte value, params sbyte[] additionalValues)
    {
        List<sbyte> valueList = new List<sbyte>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (sbyte newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts byte to byte list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<byte> ToList(this byte value, params byte[] additionalValues)
    {
        List<byte> valueList = new List<byte>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (byte newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts <see cref="bool"/> to <see cref="bool"/> list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<bool> ToList(this bool value, params bool[] additionalValues)
    {
        List<bool> valueList = new List<bool>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (bool newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts DateTimeOffset to DateTimeOffset list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<DateTimeOffset> ToList(this DateTimeOffset value, params DateTimeOffset[] additionalValues)
    {
        List<DateTimeOffset> valueList = new List<DateTimeOffset>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (DateTimeOffset newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts decimal to decimal list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<decimal> ToList(this decimal value, params decimal[] additionalValues)
    {
        List<decimal> valueList = new List<decimal>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (decimal newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts decimal to decimal list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<DateTime> ToList(this DateTime value, params DateTime[] additionalValues)
    {
        List<DateTime> valueList = new List<DateTime>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (DateTime newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts float to float list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<float> ToList(this float value, params float[] additionalValues)
    {
        List<float> valueList = new List<float>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (float newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts <see cref="ushort"/> to <see cref="ushort"/> list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<ushort> ToList(this ushort value, params ushort[] additionalValues)
    {
        List<ushort> valueList = new List<ushort>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (ushort newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts short to short list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<short> ToList(this short value, params short[] additionalValues)
    {
        List<short> valueList = new List<short>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (short newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts <see cref="ulong"/> to <see cref="ulong"/> list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<long> ToList(this long value, params long[] additionalValues)
    {
        List<long> valueList = new List<long>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (long newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts <see cref="ulong"/> to <see cref="ulong"/> list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<ulong> ToList(this ulong value, params ulong[] additionalValues)
    {
        List<ulong> valueList = new List<ulong>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (ulong newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts <see cref="uint"/> to <see cref="uint"/> list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<uint> ToList(this uint value, params uint[] additionalValues)
    {
        List<uint> valueList = new List<uint>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (uint newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts <see cref="int"/> to <see cref="int"/> list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<int> ToList(this int value, params int[] additionalValues)
    {
        List<int> valueList = new List<int>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (int newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts Uri to Uri list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<Uri> ToList(this Uri value, params Uri[] additionalValues)
    {
        List<Uri> valueList = new List<Uri>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (Uri newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts <see cref="Guid"/> to <see cref="Guid"/> list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<Guid> ToList(this Guid value, params Guid[] additionalValues)
    {
        List<Guid> valueList = new List<Guid>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (Guid newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts TimeSpan to TimeSpan list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static List<TimeSpan> ToList(this TimeSpan value, params TimeSpan[] additionalValues)
    {
        List<TimeSpan> valueList = new List<TimeSpan>();
        valueList.Add(value);

        if (additionalValues != null)
        {
            foreach (TimeSpan newValue in additionalValues)
            {
                valueList.Add(newValue);
            }
        }

        return valueList;
    }

    /// <summary>
    /// Converts double to double Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<double> ToQueue(this double value, params double[] additionalValues)
    {
        return new Queue<double>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts char to char Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<char> ToQueue(this char value, params char[] additionalValues)
    {
        return new Queue<char>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="sbyte"/> to <see cref="sbyte"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<sbyte> ToQueue(this sbyte value, params sbyte[] additionalValues)
    {
        return new Queue<sbyte>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="sbyte"/> to <see cref="sbyte"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<byte> ToQueue(this byte value, params byte[] additionalValues)
    {
        return new Queue<byte>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="bool"/> to <see cref="bool"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<bool> ToQueue(this bool value, params bool[] additionalValues)
    {
        return new Queue<bool>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts DateTimeOffset to DateTimeOffset Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<DateTimeOffset> ToQueue(this DateTimeOffset value, params DateTimeOffset[] additionalValues)
    {
        return new Queue<DateTimeOffset>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts decimal to decimal Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<decimal> ToQueue(this decimal value, params decimal[] additionalValues)
    {
        return new Queue<decimal>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts DateTime to DateTime Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<DateTime> ToQueue(this DateTime value, params DateTime[] additionalValues)
    {
        return new Queue<DateTime>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts float to float Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<float> ToQueue(this float value, params float[] additionalValues)
    {
        return new Queue<float>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="ushort"/> to <see cref="ushort"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<ushort> ToQueue(this ushort value, params ushort[] additionalValues)
    {
        return new Queue<ushort>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts short to short Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<short> ToQueue(this short value, params short[] additionalValues)
    {
        return new Queue<short>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="ulong"/> to <see cref="ulong"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<long> ToQueue(this long value, params long[] additionalValues)
    {
        return new Queue<long>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="ulong"/> to <see cref="ulong"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<ulong> ToQueue(this ulong value, params ulong[] additionalValues)
    {
        return new Queue<ulong>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="uint"/> to <see cref="uint"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<uint> ToQueue(this uint value, params uint[] additionalValues)
    {
        return new Queue<uint>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="int"/> to <see cref="int"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<int> ToQueue(this int value, params int[] additionalValues)
    {
        return new Queue<int>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="int"/> to <see cref="int"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<Uri> ToQueue(this Uri value, params Uri[] additionalValues)
    {
        return new Queue<Uri>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="Guid"/> to <see cref="Guid"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<Guid> ToQueue(this Guid value, params Guid[] additionalValues)
    {
        return new Queue<Guid>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Converts <see cref="Guid"/> to <see cref="Guid"/> Queue.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>The result.</returns>
    public static Queue<TimeSpan> ToQueue(this TimeSpan value, params TimeSpan[] additionalValues)
    {
        return new Queue<TimeSpan>(value.ToList(additionalValues));
    }

    /// <summary>
    /// Tries to convert to  data table.
    /// </summary>
    /// <param name="dataSet">The data set.</param>
    /// <param name="result">The result.</param>
    /// <param name="dataTableName">Name of the data table.</param>
    /// <returns>The success of the conversion.</returns>
    public static bool TryToDataTable(this DataSet dataSet, out DataTable result, string? dataTableName = null)
    {
        if (dataSet.Tables.Count == 0)
        {
            result = new DataTable();
            return false;
        }

        if (!string.IsNullOrEmpty(dataTableName))
        {
            if (dataSet.Tables.Contains(dataTableName))
            {
                result = dataSet.Tables[dataTableName]!;
                return true;
            }

            result = new DataTable();
            return false;
        }
        else
        {
            if (dataSet.Tables.Count == 1)
            {
                result = dataSet.Tables[0];
                return true;
            }
        }

        result = new DataTable();
        return false;
    }

    /// <summary>
    /// Tries to convert to data table.
    /// </summary>
    /// <param name="dataSet">The data set.</param>
    /// <param name="result">The result.</param>
    /// <param name="dataTableId">The data table identifier.</param>
    /// <returns>The success of the conversion.</returns>
    public static bool TryToDataTable(this DataSet dataSet, out DataTable result, int? dataTableId = null)
    {
        if (dataSet.Tables.Count == 0)
        {
            result = new DataTable();
            return false;
        }

        if (dataTableId != null)
        {
            if (dataTableId >= 0 && dataTableId < dataSet.Tables.Count)
            {
                result = dataSet.Tables[(int)dataTableId];
                return true;
            }

            result = new DataTable();
            return false;
        }
        else
        {
            if (dataSet.Tables.Count == 1)
            {
                result = dataSet.Tables[0];
                return true;
            }
        }

        result = new DataTable();
        return false;
    }

    /*
    /// <summary>
    /// Converts the data table to a generic List.
    /// </summary>
    /// <typeparam name="T">
    /// The generic type to convert to.
    /// </typeparam>
    /// <param name="dataTable">
    /// The data table.
    /// </param>
    /// <param name="typeTestAndCorrection">
    /// If set to <c>true</c> [type test and correction].
    /// </param>
    /// <returns>
    /// The List of generic type.
    /// </returns>
    internal static List<T> ToList<T>(this DataTable dataTable, bool typeTestAndCorrection = true)
        where T : new()
    {
        // ***************************************************************************************************************
        // Remove any spaces from the Table Name, as there can be no spaces in the object name;
        // ***************************************************************************************************************
        foreach (DataColumn dataColumn in dataTable.Columns)
        {
            if (dataColumn.ColumnName.Contains(" "))
            {
                dataColumn.ColumnName = dataColumn.ColumnName.RemoveEx(" ");
            }
        }

        List<T> returnList = new List<T>();
        if (dataTable.Rows.Count == 0)
        {
            return returnList;
        }

        Type componentType = typeof(T);
        string propName = string.Empty;

        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(componentType);

        // ****************************************
        // Get this list of properties (LINQ):
        // ****************************************
        List<PropertyDescriptor> propertyDescriptorList = properties.Cast<PropertyDescriptor>().Where(prop => dataTable.Columns[prop.Name] != null).ToList();

        //// Note: Above LINQ previously WAS foreach, as below:
        //// foreach (PropertyDescriptor prop in properties)
        //// {
        ////    if (dataTable.Columns[prop.Name] != null)
        ////    {
        ////        propertyDescriptorList.Add(prop);
        ////    }
        //// }

        // ****************************************************************
        // Get this list of properties of base object, if it's there
        // NOTE: Classes ending with "Base" is specific to this project ONLY.
        // ****************************************************************
        if (componentType.BaseType != null && componentType.BaseType.FullName.ToString(CultureInfo.InvariantCulture).EndsWith("Base"))
        {
            PropertyDescriptorCollection baseProperties = TypeDescriptor.GetProperties(componentType.BaseType);
            propertyDescriptorList.AddRange(baseProperties.Cast<PropertyDescriptor>().Where(prop => dataTable.Columns[prop.Name] != null));

            //// Note: Above LINQ previously WAS foreach, as below:
            //// foreach (PropertyDescriptor prop in baseProperties)
            //// {
            ////    if (dataTable.Columns[prop.Name] != null)
            ////    {
            ////        propertyDescriptorList.Add(prop);
            ////    }
            //// }
        }

        // *****************************************************************
        // Construct the list of properties which are in the data table we recieved.
        // *****************************************************************
        List<PropertyDescriptor> propertyDescriptorList2 = new List<PropertyDescriptor>();
        List<string> propertyDescriptorNameList2 = new List<string>();

        foreach (PropertyDescriptor prop in propertyDescriptorList)
        {
            propName = prop.Name;
            if (dataTable.Columns[propName] != null)
            {
                if (!propertyDescriptorNameList2.Contains(propName.ToUpper()))
                {
                    propertyDescriptorNameList2.Add(propName.ToUpper());
                    propertyDescriptorList2.Add(prop);
                }
            }
        }

        // *******************************************************************
        // If required, do a quick test that the column values can be mapped.
        // If they can't be mapped, add them to an "exclusion" list;
        // This is so that fields with erraneous values did not keep failing over and over (which will slow us down)
        // We only want to process the columns and values that can be converted.
        // *******************************************************************
        List<string> propertyToRemove = new List<string>();

        if (typeTestAndCorrection)
        {
            if (propertyDescriptorList2.Count > 0)
            {
                int maxTest = 10;

                if (dataTable.Rows.Count - 1 < maxTest)
                {
                    maxTest = dataTable.Rows.Count - 1;
                }

                for (int rowi = 0; rowi < maxTest; rowi++)
                {
                    DataRow row = dataTable.Rows[rowi];

                    foreach (PropertyDescriptor prop in propertyDescriptorList2)
                    {
                        object value = row[prop.Name];

                        T obj = new T();

                        try
                        {
                            // Check if it's a nullable type first.
                            Type nullableType = Nullable.GetUnderlyingType(prop.PropertyType);

                            if (nullableType != null)
                            {
                                prop.SetValue(obj, Convert.ChangeType(value, nullableType));
                            }
                            else
                            {
                                prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                            }
                        }
                        catch
                        {
                            if (!propertyToRemove.Contains(prop.Name))
                            {
                                propertyToRemove.Add(prop.Name);
                            }
                        }
                    }
                }
            }
        }

        // ************************************************************************
        // Apply the exclusion list, keeping only the properties we can handle.
        // ************************************************************************
        List<PropertyDescriptor> propertyDescriptorList3 = new List<PropertyDescriptor>();

        foreach (PropertyDescriptor prop in propertyDescriptorList2)
        {
            if (!propertyToRemove.Contains(prop.Name))
            {
                propertyDescriptorList3.Add(prop);
            }
        }

        // **********************************************************
        // Create PropertyDescriptorCollection from the list.
        // **********************************************************
        PropertyDescriptorCollection jointProperties = new PropertyDescriptorCollection(propertyDescriptorList3.ToArray());

        // **********************************************************
        // Create the object list.
        // **********************************************************
        foreach (DataRow row in dataTable.Rows)
        {
            T obj = new T();
            for (int i = 0; i < jointProperties.Count; i++)
            {
                PropertyDescriptor prop = jointProperties[i];
                object value = row[prop.Name];

                try
                {
                    if (!Convert.IsDBNull(value))
                    {
                        Type nullableType = Nullable.GetUnderlyingType(prop.PropertyType);

                        if (nullableType != null)
                        {
                            prop.SetValue(obj, Convert.ChangeType(value, nullableType));
                        }
                        else
                        {
                            prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                        }
                    }
                }
                catch
                {
                }
            }

            returnList.Add(obj);
        }

        return returnList;
    }
    */

    /// <summary>
    /// The list of generic data objects to data table.
    /// </summary>
    /// <typeparam name="T">The generic type T</typeparam>
    /// <param name="data">The data.</param>
    /// <param name="groupGuidName">Name of the external GUID.</param>
    /// <param name="groupGuid">The header GUID.</param>
    /// <returns>The <see cref="DataTable" />.</returns>
    internal static DataTable ConvertToDataTable<T>(this IEnumerable<T> data, string? groupGuidName = null, Guid? groupGuid = null)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
        DataTable table = new DataTable();
        foreach (PropertyDescriptor prop in properties)
        {
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }

        if (groupGuid != null && !string.IsNullOrEmpty(groupGuidName))
        {
            if (!table.Columns.Contains(groupGuidName))
            {
                table.Columns.Add(groupGuidName, groupGuid.GetType());
            }

            if (data != null)
            {
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }

                    row[groupGuidName] = groupGuid;
                    table.Rows.Add(row);
                }
            }
        }
        else
        {
            if (data != null)
            {
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }

                    table.Rows.Add(row);
                }
            }
        }

        return table;
    }

    /// <summary>
    /// The list of generic data objects to data table.
    /// </summary>
    /// <typeparam name="T">The generic type T.</typeparam>
    /// <param name="data">The data.</param>
    /// <param name="groupGuidName">Name of the external GUID.</param>
    /// <param name="groupGuid">The header GUID.</param>
    /// <param name="maxRecords">The max records.</param>
    /// <returns>The <see cref="DataTable" />.</returns>
    internal static DataTable ConvertToDataTable<T>(this IList<T> data, string? groupGuidName = null, Guid? groupGuid = null, int maxRecords = 0)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
        DataTable table = new DataTable();
        foreach (PropertyDescriptor prop in properties)
        {
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }

        // maxRecords limit can be set up to split the groups into TOP x,xxx sections.
        // maxRecords == 0 means there is no limit set.
        if (maxRecords == 0)
        {
            if (groupGuid != null && !string.IsNullOrEmpty(groupGuidName))
            {
                if (!table.Columns.Contains(groupGuidName))
                {
                    table.Columns.Add(groupGuidName, groupGuid.GetType());
                }

                if (data != null)
                {
                    foreach (T item in data)
                    {
                        DataRow row = table.NewRow();
                        foreach (PropertyDescriptor prop in properties)
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }

                        row[groupGuidName] = groupGuid;
                        table.Rows.Add(row);
                    }
                }
            }
            else
            {
                if (data != null)
                {
                    foreach (T item in data)
                    {
                        DataRow row = table.NewRow();
                        foreach (PropertyDescriptor prop in properties)
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }

                        table.Rows.Add(row);
                    }
                }
            }
        }
        else
        {
            if (groupGuid != null && !string.IsNullOrEmpty(groupGuidName))
            {
                if (!table.Columns.Contains(groupGuidName))
                {
                    table.Columns.Add(groupGuidName, groupGuid.GetType());
                }

                if (data != null)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (i >= maxRecords)
                        {
                            break;
                        }

                        T item = data[i];
                        DataRow row = table.NewRow();
                        foreach (PropertyDescriptor prop in properties)
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }

                        row[groupGuidName] = groupGuid;
                        table.Rows.Add(row);
                    }
                }
            }
            else
            {
                if (data != null)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (i >= maxRecords)
                        {
                            break;
                        }

                        T item = data[i];
                        DataRow row = table.NewRow();
                        foreach (PropertyDescriptor prop in properties)
                        {
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        }

                        table.Rows.Add(row);
                    }
                }
            }
        }

        return table;
    }

    /// <summary>
    /// Convert to hash set.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <typeparam name="TX">The type to convert from.</typeparam>
    /// <param name="data">The data.</param>
    /// <returns>The Hash Set.</returns>
    internal static HashSet<T> ConvertToHashSet<T, TX>(this ISet<TX> data)
        where T : new()
    {
        HashSet<T> returnHashset = new HashSet<T>();
        if (data == null || data.Count == 0)
        {
            return returnHashset;
        }

        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

        //// get this list of properties
        List<PropertyDescriptor> propertyDescriptorList = properties.Cast<PropertyDescriptor>().ToList();

        PropertyDescriptorCollection propertiesSource = TypeDescriptor.GetProperties(typeof(TX));

        //// get this list of properties
        List<PropertyDescriptor> propertyDescriptorSourceList = propertiesSource.Cast<PropertyDescriptor>().ToList();
        //// List<string> propertyDescriptorSourceNameList = new List<string>();

        //// foreach (PropertyDescriptor prop in propertyDescriptorSourceList)
        //// {
        ////    propertyDescriptorSourceNameList.Add(prop.Name.ToUpper());
        //// }

        List<KeyValuePair<int, int>> kvp = new List<KeyValuePair<int, int>>();

        for (int j = 0; j < propertyDescriptorList.Count; j++)
        {
            for (int i = 0; i < propertyDescriptorSourceList.Count; i++)
            {
                if (propertyDescriptorList[j].Name.ToUpper() == propertyDescriptorSourceList[i].Name.ToUpper())
                {
                    // if (propertyDescriptorList[j].PropertyType.UnderlyingSystemType == propertyDescriptorSourceList[i].PropertyType.UnderlyingSystemType)
                    // {
                    //    kvp.Add(new KeyValuePair<int, int>(i, j));
                    //    break;
                    // }
                    try
                    {
                        Type? nullableType = Nullable.GetUnderlyingType(propertyDescriptorList[j].PropertyType.UnderlyingSystemType);

                        if (nullableType != null)
                        {
                            if (nullableType == propertyDescriptorSourceList[i].PropertyType.UnderlyingSystemType)
                            {
                                // Might be able to do something with Convert.ChangeType() instead of this 'fail if not the same type'...
                                kvp.Add(new KeyValuePair<int, int>(i, j));
                                break;
                            }
                        }
                        else
                        {
                            if (propertyDescriptorList[j].PropertyType.UnderlyingSystemType == propertyDescriptorSourceList[i].PropertyType.UnderlyingSystemType)
                            {
                                // Might be able to do something with Convert.ChangeType() instead of this 'fail if not the same type'...
                                kvp.Add(new KeyValuePair<int, int>(i, j));
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        if (kvp.Count == 0)
        {
            return returnHashset;
        }

        foreach (TX item in data)
        {
            T obj = new T();

            foreach (KeyValuePair<int, int> t in kvp)
            {
                var value = propertiesSource[t.Key].GetValue(item) ?? DBNull.Value;

                if (value != DBNull.Value)
                {
                    properties[t.Value].SetValue(obj, value);
                }
            }

            returnHashset.Add(obj);
        }

        return returnHashset;

        /*
        // filter to only contain the properties which are in the table.
        List<PropertyDescriptor> propertyDescriptorList2 = new List<PropertyDescriptor>();
        List<string> propertyDescriptorNameList2 = new List<string>();

        string propName = string.Empty;
        bool anyPropertiesInCommon = false;
        foreach (PropertyDescriptor prop in propertyDescriptorList)
        {
            propName = prop.Name;

            if (propertyDescriptorSourceNameList.Contains(propName.ToUpper()))
            {
                if (!propertyDescriptorNameList2.Contains(propName.ToUpper()))
                {
                    propertyDescriptorNameList2.Add(propName.ToUpper());
                    propertyDescriptorList2.Add(prop);
                    anyPropertiesInCommon = true;
                }
            }
        }
        */

        /*
        if (!anyPropertiesInCommon)
        {
            return returnHashset;
        }

        //DataTable table = new DataTable();
        //foreach (PropertyDescriptor prop in properties)
        //{
        //    //table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        //}

        foreach (X item in data)
        {
            T obj = new T();

            foreach (PropertyDescriptor prop in properties)
            {

                if (propertyDescriptorNameList2.Contains(prop.Name.ToUpper()))
                {

                    var value = prop.GetValue(item) ?? DBNull.Value;

                    if (value != DBNull.Value)
                    {
                        prop.SetValue(obj, value);
                    }
                }
            }

            returnHashset.Add(obj);

            //DataRow row = table.NewRow();
            //foreach (PropertyDescriptor prop in properties)
            //{
            //    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            //}

            //table.Rows.Add(row);
        }
        */
    }
    /// <summary>
    /// Converts the data table to a generic list.
    /// </summary>
    /// <typeparam name="T">The generic type.</typeparam>
    /// <param name="dataTable">The data table.</param>
    /// <param name="typeTestAndCorrection">if set to <c>true</c> [type test and correction].</param>
    /// <returns>The list of generic type.</returns>
    internal static List<T> ToList<T>(this DataTable dataTable, bool typeTestAndCorrection = true)
        where T : new()
    {
        foreach (DataColumn dataColumn in dataTable.Columns)
        {
            if (dataColumn.ColumnName.Contains(" "))
            {
                dataColumn.ColumnName = dataColumn.ColumnName.RemoveEx(" ");
            }

            // for (int i = 0; i < processedDataTable.Columns.Count; i++)
            // {
            //    if (dataColumn.ColumnName.ToLower(CultureInfo.GetCultureInfo("en-GB")) == processedDataTable.Columns[i].ColumnName.ToLower(CultureInfo.GetCultureInfo("en-GB")) || (dataColumn.ColumnName.ToLower(CultureInfo.GetCultureInfo("en-GB")) == "landline" && processedDataTable.Columns[i].ColumnName.ToLower(CultureInfo.GetCultureInfo("en-GB")) == "contactnumber") || (dataColumn.ColumnName.ToLower(CultureInfo.GetCultureInfo("en-GB")) == "mobile" && processedDataTable.Columns[i].ColumnName.ToLower(CultureInfo.GetCultureInfo("en-GB")) == "contactnumber2"))
            //    {
            //        if (!seedToCsvColumnMapping.ContainsKey(dataColumn.ColumnName) && !seedToCsvColumnMapping.ContainsValue(i))
            //        {
            //            seedToCsvColumnMapping.Add(dataColumn.ColumnName, i);
            //        }
            //    }
            // }
        }

        List<T> returnList = new List<T>();
        if (dataTable.Rows.Count == 0)
        {
            return returnList;
        }

        Type componentType = typeof(T);
        string propName = string.Empty;

        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(componentType);

        // get this list of properties - added ! to prop.Name
        List<PropertyDescriptor> propertyDescriptorList = properties.Cast<PropertyDescriptor>().Where(prop => dataTable.Columns[prop.Name!] != null).ToList();

        //// WAS:
        //// foreach (PropertyDescriptor prop in properties)
        //// {
        ////    if (dataTable.Columns[prop.Name] != null)
        ////    {
        ////        propertyDescriptorList.Add(prop);
        ////    }
        //// }

        // get this list of properties of base object, if it's there - added ? and !
        if (componentType.BaseType != null && componentType.BaseType.FullName?.EndsWith("Base", StringComparison.Ordinal) == true)
        {
            PropertyDescriptorCollection baseProperties = TypeDescriptor.GetProperties(componentType.BaseType);
            propertyDescriptorList.AddRange(baseProperties.Cast<PropertyDescriptor>().Where(prop => dataTable.Columns[prop.Name!] != null));

            //// WAS:
            //// foreach (PropertyDescriptor prop in baseProperties)
            //// {
            ////    if (dataTable.Columns[prop.Name] != null)
            ////    {
            ////        propertyDescriptorList.Add(prop);
            ////    }
            //// }
        }

        // filter to only contain the properties which are in the table.
        List<PropertyDescriptor> propertyDescriptorList2 = new List<PropertyDescriptor>();
        List<string> propertyDescriptorNameList2 = new List<string>();

        foreach (PropertyDescriptor prop in propertyDescriptorList)
        {
            propName = prop.Name;
            if (dataTable.Columns[propName!] != null)
            {
                if (!propertyDescriptorNameList2.Contains(propName!.ToUpper(CultureInfo.GetCultureInfo("en-GB"))))
                {
                    propertyDescriptorNameList2.Add(propName!.ToUpper(CultureInfo.GetCultureInfo("en-GB")));
                    propertyDescriptorList2.Add(prop);
                }
            }
        }

        // test that properties do not error on conversion....
        List<string> propertyToRemove = new List<string>();

        if (typeTestAndCorrection)
        {
            if (propertyDescriptorList2.Count > 0)
            {
                int maxTest = 10;

                if (dataTable.Rows.Count - 1 < maxTest)
                {
                    maxTest = dataTable.Rows.Count - 1;
                }

                for (int rowi = 0; rowi < maxTest; rowi++)
                {
                    DataRow row = dataTable.Rows[rowi];

                    foreach (PropertyDescriptor prop in propertyDescriptorList2)
                    {
                        object value = row[prop.Name!];

                        T obj = new T();

                        try
                        {
                            // value? and ?? ensures stringToConvert is never null
                            string stringToConvert = value?.ToString() ?? string.Empty;

                            switch (prop.PropertyType.FullName)
                            {
                                case "System.String":
                                    prop.SetValue(obj, row[prop.Name!]);
                                    break;
                                case "System.Int16":
                                    prop.SetValue(obj, short.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case "System.Int32":
                                    prop.SetValue(obj, int.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case "System.Int64":
                                    prop.SetValue(obj, long.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case "System.Double":
                                    prop.SetValue(obj, double.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case "System.Decimal":
                                    prop.SetValue(obj, decimal.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case "System.Byte":
                                    prop.SetValue(obj, byte.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case "System.DateTime":
                                    prop.SetValue(obj, DateTime.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case "System.SByte":
                                    prop.SetValue(obj, sbyte.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case "System.Boolean":
                                    prop.SetValue(obj, bool.Parse(stringToConvert));
                                    break;
                                case "System.Char":
                                    prop.SetValue(obj, char.Parse(stringToConvert));
                                    break;
                                case "System.Guid":
                                    prop.SetValue(obj, Guid.Parse(stringToConvert));
                                    break;
                                case "System.Nullable`1[[System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                                    if (!string.IsNullOrEmpty(stringToConvert))
                                    {
                                        prop.SetValue(obj, DateTime.Parse(stringToConvert, CultureInfo.GetCultureInfo("en-GB")));
                                    }

                                    break;
                            }
                        }
                        catch
                        {
                            propertyToRemove.Add(prop.Name!);

                            // This excpetion has been handled.
                            /*
                            try
                            {
                                throw new CustomException(exception.Message, exception);
                            }
                            catch (CustomException customException)
                            {
                                // Don't care if this does not work for any reason.
                                CustomExceptionHelper.Ignore(customException);
                            }
                            */
                        }
                    }
                }
            }
        }

        ////  combine into PropertyDescriptorCollection
        PropertyDescriptorCollection jointProperties = new PropertyDescriptorCollection(propertyDescriptorList2.Where(prop => !propertyToRemove.Contains(prop.Name!)).ToArray());

        ////  converted to linq from:
        ////  foreach (PropertyDescriptor prop in propertyDescriptorList2)
        ////  {
        ////       if (!propertyToRemove.Contains(prop.Name))
        ////       {
        ////           propertyDescriptorList3.Add(prop);
        ////       }
        ////  }

        // propertyDescriptorList2

        // populate the list
        if (jointProperties.Count > 0)
        {
            //// dataTable.Rows[]

            //// var typeOfString = typeof(string);

            Dictionary<int, ObjectTypes> propertyToTypeMapping = new Dictionary<int, ObjectTypes>();

            for (int i = 0; i < jointProperties.Count; i++)
            {
                PropertyDescriptor prop = jointProperties[i]!;

                Type propertyType = prop.PropertyType;
                Type? nullableType = Nullable.GetUnderlyingType(propertyType);
                if (nullableType != null)
                {
                    propertyType = nullableType;
                }

                if (propertyType == typeof(string))
                {
                    propertyToTypeMapping.Add(i, ObjectTypes.StringType);
                    //// prop.SetValue(obj, row[prop.Name]);
                }
                else if (propertyType == typeof(short))
                {
                    //// short = Int16
                    propertyToTypeMapping.Add(i, ObjectTypes.Int16Type);
                    //// prop.SetValue(obj, Convert.ToInt16(row[prop.Name]));
                    //// prop.SetValue(obj, Int16.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(int))
                {
                    //// int = Int32
                    propertyToTypeMapping.Add(i, ObjectTypes.Int32Type);
                    //// prop.SetValue(obj, Int32.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(long))
                {
                    ////=Int64
                    propertyToTypeMapping.Add(i, ObjectTypes.Int64Type);
                    //// prop.SetValue(obj, Convert.ToInt64(row[prop.Name]));
                    //// prop.SetValue(obj, Int32.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(double))
                {
                    propertyToTypeMapping.Add(i, ObjectTypes.DoubleType);
                    //// prop.SetValue(obj, Convert.ToDouble(row[prop.Name]));
                    //// prop.SetValue(obj, Int32.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(byte))
                {
                    propertyToTypeMapping.Add(i, ObjectTypes.ByteType);
                    //// prop.SetValue(obj, Convert.ToByte(row[prop.Name]));
                    //// prop.SetValue(obj, Int32.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(DateTime))
                {
                    propertyToTypeMapping.Add(i, ObjectTypes.DateTimeType);
                    //// prop.SetValue(obj, Convert.ToDateTime(row[prop.Name]));
                    //// prop.SetValue(obj, Int32.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(sbyte))
                {
                    //// SByte = sbyte
                    propertyToTypeMapping.Add(i, ObjectTypes.SByteType);
                    //// prop.SetValue(obj, Convert.ToSByte(row[prop.Name]));
                    //// prop.SetValue(obj, Int32.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(bool))
                {
                    propertyToTypeMapping.Add(i, ObjectTypes.BooleanType);
                    //// prop.SetValue(obj, Convert.ToBoolean(row[prop.Name]));
                    //// prop.SetValue(obj, Int32.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(char))
                {
                    propertyToTypeMapping.Add(i, ObjectTypes.CharType);
                    //// prop.SetValue(obj, Convert.ToChar(row[prop.Name]));
                    //// prop.SetValue(obj, Int32.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(Guid))
                {
                    propertyToTypeMapping.Add(i, ObjectTypes.GuidType);
                    //// prop.SetValue(obj, Convert.ToGuid(row[prop.Name]));
                    //// prop.SetValue(obj, Guid.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else if (propertyType == typeof(decimal))
                {
                    propertyToTypeMapping.Add(i, ObjectTypes.DecimalType);
                    //// prop.SetValue(obj, Convert.ToGuid(row[prop.Name]));
                    //// prop.SetValue(obj, Guid.Parse(row[prop.Name].ToString(CultureInfo.GetCultureInfo("en-GB"))));
                }
                else
                {
                    //// CustomExceptionHelper.Output(prop.PropertyType.Name + " for " + prop.Name + " is not a defined type");
                    //// Error
                }

                //// propertyToTypeMapping.Add(i, propertyType);
            }

            foreach (DataRow row in dataTable.Rows)
            {
                T obj = new T();
                for (int i = 0; i < jointProperties.Count; i++)
                {
                    PropertyDescriptor prop = jointProperties[i]!;
                    object value = row[prop.Name!];

                    try
                    {
                        if (!Convert.IsDBNull(value))
                        {
                            switch (propertyToTypeMapping[i])
                            {
                                case ObjectTypes.StringType:
                                    prop.SetValue(obj, row[prop.Name!]);
                                    break;
                                case ObjectTypes.DateTimeType:
                                    prop.SetValue(obj, Convert.ToDateTime(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.Int16Type:
                                    prop.SetValue(obj, Convert.ToInt16(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.Int32Type:
                                    prop.SetValue(obj, Convert.ToInt32(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.Int64Type:
                                    prop.SetValue(obj, Convert.ToInt64(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.DoubleType:
                                    prop.SetValue(obj, Convert.ToDouble(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.ByteType:
                                    prop.SetValue(obj, Convert.ToByte(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.SByteType:
                                    prop.SetValue(obj, Convert.ToSByte(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.BooleanType:
                                    prop.SetValue(obj, Convert.ToBoolean(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.CharType:
                                    prop.SetValue(obj, Convert.ToChar(value, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                                case ObjectTypes.GuidType:
                                    // value! and ToString()! used because IsDBNull check confirms data exists
                                    prop.SetValue(obj, Guid.Parse(value!.ToString()!));
                                    break;
                                case ObjectTypes.DecimalType:
                                    prop.SetValue(obj, decimal.Parse(value!.ToString()!, CultureInfo.GetCultureInfo("en-GB")));
                                    break;
                            }
                        }
                    }
                    catch
                    {
                        /*
                        try
                        {
                            throw new CustomException(exception.Message, exception);
                        }
                        catch (CustomException customException)
                        {
                            // Don't care if this does not work for any reason.
                            CustomExceptionHelper.Ignore(customException);
                        }
                        */
                    }
                }

                returnList.Add(obj);
            }
        }

        return returnList;
    }

    /// <summary>
    /// Converts XML string to String List.
    /// </summary>
    /// <param name="xmlString">The XML string.</param>
    /// <returns>The list of strings.</returns>
    internal static List<string?>? ToStringList(this string xmlString)
    {
        List<string?> returnList = new List<string?>();
        if (!string.IsNullOrEmpty(xmlString))
        {
            using StringReader stringReader = new StringReader(xmlString);
            using XmlReader xmlReader = new XmlTextReader(stringReader);
            XmlSerializer xmlSerializer = new XmlSerializer(returnList.GetType());

            //// MemoryStream memoryStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(xmlString));
            //// XmlReader xmlReader = XmlReader.Create(memoryStream);

            try
            {
                if (xmlSerializer.Deserialize(xmlReader) is List<string> deserializedList)
                {
                    returnList = deserializedList.Select(item => (string?)item).ToList();
                }
            }
            catch
            {
                // GlobalNotificationItemList.AddNotificationItem("Error in ToStringList.", ex.Message + "\n\n" + ex.StackTrace, ToolTipIcon.Warning);
            }
        }

        return returnList;
    }

    /// <summary>
    /// Converts String List To The XML string.
    /// </summary>
    /// <param name="stringList">The string list.</param>
    /// <returns>The XML as string.</returns>
    internal static string ToXmlString(this List<string>? stringList)
    {
        string result = string.Empty;
        try
        {
            if (stringList != null && stringList.Count > 0)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(stringList.GetType());
                StringBuilder sb = new StringBuilder();
                using XmlWriter xmlWriter = XmlWriter.Create(sb);
                xmlSerializer.Serialize(xmlWriter, stringList);
                xmlWriter.Flush();
                result = sb.ToString();
            }
        }
        catch
        {
            // GlobalNotificationItemList.AddNotificationItem("Error in Transformations.ToXmlString.", ex.Message + "\n\n" + ex.StackTrace, ToolTipIcon.Warning);
        }

        return result;
    }

    #endregion Methods
}