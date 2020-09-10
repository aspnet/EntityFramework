// <auto-generated />

using System.Reflection;
using System.Resources;

namespace Microsoft.Data.Sqlite.Properties
{
    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.Data.Sqlite.Properties.Resources", typeof(Resources).Assembly);

        /// <summary>
        /// {methodName} can only be called when the connection is open.
        /// </summary>
        public static string CallRequiresOpenConnection(object methodName)
            => string.Format(
                GetString("CallRequiresOpenConnection", nameof(methodName)),
                methodName);

        /// <summary>
        /// CommandText must be set before {methodName} can be called.
        /// </summary>
        public static string CallRequiresSetCommandText(object methodName)
            => string.Format(
                GetString("CallRequiresSetCommandText", nameof(methodName)),
                methodName);

        /// <summary>
        /// ConnectionString cannot be set when the connection is open.
        /// </summary>
        public static string ConnectionStringRequiresClosedConnection
            => GetString("ConnectionStringRequiresClosedConnection");

        /// <summary>
        /// Invalid attempt to call {operation} when reader is closed.
        /// </summary>
        public static string DataReaderClosed(object operation)
            => string.Format(
                GetString("DataReaderClosed", nameof(operation)),
                operation);

        /// <summary>
        /// The CommandType '{commandType}' is not supported.
        /// </summary>
        public static string InvalidCommandType(object commandType)
            => string.Format(
                GetString("InvalidCommandType", nameof(commandType)),
                commandType);

        /// <summary>
        /// The IsolationLevel '{isolationLevel}' is not supported.
        /// </summary>
        public static string InvalidIsolationLevel(object isolationLevel)
            => string.Format(
                GetString("InvalidIsolationLevel", nameof(isolationLevel)),
                isolationLevel);

        /// <summary>
        /// The ParameterDirection '{direction}' is not supported.
        /// </summary>
        public static string InvalidParameterDirection(object direction)
            => string.Format(
                GetString("InvalidParameterDirection", nameof(direction)),
                direction);

        /// <summary>
        /// Connection string keyword '{keyword}' is not supported. For a possible alternative, see https://go.microsoft.com/fwlink/?linkid=2142181.
        /// </summary>
        public static string KeywordNotSupported(object keyword)
            => string.Format(
                GetString("KeywordNotSupported", nameof(keyword)),
                keyword);

        /// <summary>
        /// Must add values for the following parameters: {parameters}
        /// </summary>
        public static string MissingParameters(object parameters)
            => string.Format(
                GetString("MissingParameters", nameof(parameters)),
                parameters);

        /// <summary>
        /// No data exists for the row/column.
        /// </summary>
        public static string NoData
            => GetString("NoData");

        /// <summary>
        /// ConnectionString must be set before Open can be called.
        /// </summary>
        public static string OpenRequiresSetConnectionString
            => GetString("OpenRequiresSetConnectionString");

        /// <summary>
        /// SqliteConnection does not support nested transactions.
        /// </summary>
        public static string ParallelTransactionsNotSupported
            => GetString("ParallelTransactionsNotSupported");

        /// <summary>
        /// A SqliteParameter with ParameterName '{parameterName}' is not contained by this SqliteParameterCollection.
        /// </summary>
        public static string ParameterNotFound(object parameterName)
            => string.Format(
                GetString("ParameterNotFound", nameof(parameterName)),
                parameterName);

        /// <summary>
        /// {propertyName} must be set.
        /// </summary>
        public static string RequiresSet(object propertyName)
            => string.Format(
                GetString("RequiresSet", nameof(propertyName)),
                propertyName);

        /// <summary>
        /// This SqliteTransaction has completed; it is no longer usable.
        /// </summary>
        public static string TransactionCompleted
            => GetString("TransactionCompleted");

        /// <summary>
        /// The transaction object is not associated with the same connection object as this command.
        /// </summary>
        public static string TransactionConnectionMismatch
            => GetString("TransactionConnectionMismatch");

        /// <summary>
        /// Execute requires the command to have a transaction object when the connection assigned to the command is in a pending local transaction.  The Transaction property of the command has not been initialized.
        /// </summary>
        public static string TransactionRequired
            => GetString("TransactionRequired");

        /// <summary>
        /// No mapping exists from object type {typeName} to a known managed provider native type.
        /// </summary>
        public static string UnknownDataType(object typeName)
            => string.Format(
                GetString("UnknownDataType", nameof(typeName)),
                typeName);

        /// <summary>
        /// SQLite Error {errorCode}: '{message}'.
        /// </summary>
        public static string SqliteNativeError(object errorCode, object message)
            => string.Format(
                GetString("SqliteNativeError", nameof(errorCode), nameof(message)),
                errorCode, message);

        /// <summary>
        /// For more information on this error code see https://www.sqlite.org/rescode.html
        /// </summary>
        public static string DefaultNativeError
            => GetString("DefaultNativeError");

        /// <summary>
        /// Cannot bind the value for parameter '{parameterName}' because multiple matching parameters were found in the command text. Specify the parameter name with the symbol prefix, e.g. '@{parameterName}'.
        /// </summary>
        public static string AmbiguousParameterName(object parameterName)
            => string.Format(
                GetString("AmbiguousParameterName", nameof(parameterName)),
                parameterName);

        /// <summary>
        /// The {enumType} enumeration value, {value}, is invalid.
        /// </summary>
        public static string InvalidEnumValue(object enumType, object value)
            => string.Format(
                GetString("InvalidEnumValue", nameof(enumType), nameof(value)),
                enumType, value);

        /// <summary>
        /// Cannot convert object of type '{sourceType}' to object of type '{targetType}'.
        /// </summary>
        public static string ConvertFailed(object sourceType, object targetType)
            => string.Format(
                GetString("ConvertFailed", nameof(sourceType), nameof(targetType)),
                sourceType, targetType);

        /// <summary>
        /// Cannot store 'NaN' values.
        /// </summary>
        public static string CannotStoreNaN
            => GetString("CannotStoreNaN");

        /// <summary>
        /// An open reader is already associated with this command. Close it before opening a new one.
        /// </summary>
        public static string DataReaderOpen
            => GetString("DataReaderOpen");

        /// <summary>
        /// An open reader is associated with this command. Close it before changing the {propertyName} property.
        /// </summary>
        public static string SetRequiresNoOpenReader(object propertyName)
            => string.Format(
                GetString("SetRequiresNoOpenReader", nameof(propertyName)),
                propertyName);

        /// <summary>
        /// The data is NULL at ordinal {ordinal}. This method can't be called on NULL values. Check using IsDBNull before calling.
        /// </summary>
        public static string CalledOnNullValue(object ordinal)
            => string.Format(
                GetString("CalledOnNullValue", nameof(ordinal)),
                ordinal);

        /// <summary>
        /// The SQL function '{function}' was called with a NULL argument at ordinal {ordinal}. Create the function using a Nullable parameter or rewrite your query to avoid passing NULL.
        /// </summary>
        public static string UDFCalledWithNull(object function, object ordinal)
            => string.Format(
                GetString("UDFCalledWithNull", nameof(function), nameof(ordinal)),
                function, ordinal);

        /// <summary>
        /// SqliteBlob can only be used when the connection is open.
        /// </summary>
        public static string SqlBlobRequiresOpenConnection
            => GetString("SqlBlobRequiresOpenConnection");

        /// <summary>
        /// Offset and count were out of bounds for the buffer.
        /// </summary>
        public static string InvalidOffsetAndCount
            => GetString("InvalidOffsetAndCount");

        /// <summary>
        /// The size of a blob may not be changed by the SqliteBlob API. Use an UPDATE command instead.
        /// </summary>
        public static string ResizeNotSupported
            => GetString("ResizeNotSupported");

        /// <summary>
        /// An attempt was made to move the position before the beginning of the stream.
        /// </summary>
        public static string SeekBeforeBegin
            => GetString("SeekBeforeBegin");

        /// <summary>
        /// Stream does not support writing.
        /// </summary>
        public static string WriteNotSupported
            => GetString("WriteNotSupported");

        /// <summary>
        /// You specified a password in the connection string, but the native SQLite library '{libraryName}' doesn't support encryption.
        /// </summary>
        public static string EncryptionNotSupported(object libraryName)
            => string.Format(
                GetString("EncryptionNotSupported", nameof(libraryName)),
                libraryName);

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);
            for (var i = 0; i < formatterNames.Length; i++)
            {
                value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }

            return value;
        }
    }
}
