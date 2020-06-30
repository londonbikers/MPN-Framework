using System;
using System.Data;
using System.Text.RegularExpressions;

namespace MPN.Framework.Data
{
    public class Data
    {
        #region public static methods
		/// <summary>
		/// Breaks any attempted SQL-Injection attack.
		/// </summary>
		public static string FilterOutSqlInjection(string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			text = Regex.Replace(text, "'", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "\"", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "--", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "delete from", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "drop column", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "drop procedure", string.Empty, RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "drop database", string.Empty, RegexOptions.IgnoreCase);

			return text;
		}

        /// <summary>
        /// Helps with the retrival of values from MS SQL Server by converting null values to the equivilent .Net empty value.
        /// </summary>
        /// <param name="type">The Type of the source data.</param>
        /// <param name="databaseValue">The value taken from a DataReader, or the like.</param>
        /// <returns>The equivilent .Net object in either value or empty form.</returns>
        public static object GetValue(Type type, object databaseValue) 
        {
            if (type.Equals(typeof(bool)) && databaseValue == DBNull.Value)
                return false;
            else if (type.Equals(typeof(Int16)) && databaseValue == DBNull.Value ||
                type.Equals(typeof(Int32)) && databaseValue == DBNull.Value ||
                type.Equals(typeof(Int64)) && databaseValue == DBNull.Value ||
                type.Equals(typeof(decimal)) && databaseValue == DBNull.Value ||
                type.Equals(typeof(byte)) && databaseValue == DBNull.Value ||
                type.Equals(typeof(long)) && databaseValue == DBNull.Value)
                return 0;
            else if (type.Equals(typeof(string)) && databaseValue == DBNull.Value)
                return String.Empty;
            else if (type.Equals(typeof(DateTime)) && databaseValue == DBNull.Value)
                return DateTime.MinValue;
            else
                return databaseValue;
        }

        /// <summary>
        /// This method will convert SQL Server-specific null, or equivilent empty values to acceptable
        /// C# empty values. i.e. character column value type nulls to String.Empty.
        /// </summary>
        /// <param name="table">The DataTable to process</param>
        public static DataTable AssignDefaultValues(DataTable table) 
        {
            var rows = table.Rows;
            var cols = table.Columns;

            for (var i = 0; i < rows.Count; i++)
            {
                for (var y = 0; y < cols.Count; y++)
                {
                    if (rows[i][y] != DBNull.Value) continue;
                    if (cols[y].DataType.Equals(typeof(bool)) && rows[i][y] == DBNull.Value)
                        rows[i][y] = false;
                    else if (cols[y].DataType.Equals(typeof(Int16)) && rows[i][y] == DBNull.Value ||
                             cols[y].DataType.Equals(typeof(Int32)) && rows[i][y] == DBNull.Value ||
                             cols[y].DataType.Equals(typeof(Int64)) && rows[i][y] == DBNull.Value)
                        rows[i][y] = 0;
                    else if (cols[y].DataType.Equals(typeof(decimal)) && rows[i][y] == DBNull.Value)
                        rows[i][y] = 0;
                    else if (cols[y].DataType.Equals(typeof(string)) && rows[i][y] == DBNull.Value)
                        rows[i][y] = String.Empty;
                    else if (cols[y].DataType.Equals(typeof(DateTime)) && rows[i][y] == DBNull.Value)
                        rows[i][y] = DateTime.MinValue;
                }
            }

            return table;
        }

        /// <summary>
        /// Ensures empty values are converted to DBNulls and the like.
        /// </summary>
        /// <param name="parameter">The parameter/object being passed into SQL Server.</param>
        public static object ToSql(object parameter)
        {
            if (parameter is String && (parameter as String).Trim() == String.Empty)
                return DBNull.Value;

            // No change required.
            return parameter;
        }
        
        /// <summary>
        /// Ensures that any literal being used in a plain-text SQL query is properly escaped so that
        /// a SQL Server error is not thrown when the query is executed.
        /// </summary>
        /// <param name="criteria">The string representing the criteria</param>
        /// <returns>An escaped SQL criteria string.</returns>
        public static string EscapePlainTextQueryCriteria(string criteria) 
        {
            return criteria.Replace("'", "''");
        }
        
        /// <summary>
        /// Functions as EscapePlainTextQueryCriteria() does, but tailored specifically for LIKE criteria.
        /// </summary>
        /// <param name="criteria">The string representing the criteria</param>
        /// <returns>An escaped SQL criteria string.</returns>
        public static string EscapePlainTextQueryLikeCriteria(string criteria) 
        {
            criteria = EscapePlainTextQueryCriteria(criteria);
            criteria = criteria.Replace("[", "[[]");
            criteria = criteria.Replace("%", "[%]");
            criteria = criteria.Replace("_", "[_]");
            criteria = criteria.Replace("-", "[-]");

            return criteria;
        }
        #endregion
    }
}