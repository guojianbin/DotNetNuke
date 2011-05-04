#region Copyright

// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2011
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using DotNetNuke.Entities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Common.Utilities
    /// Class:      CBO
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CBO class generates objects.
    /// </summary>
    /// <history>
    ///     [cnurse]	12/01/2007	Documented
    /// </history>
    /// -----------------------------------------------------------------------------
    public class CBO
    {
        private const string defaultCacheByProperty = "ModuleID";
        private const int defaultCacheTimeOut = 20;
        private const string defaultPrimaryKey = "ItemID";
        private const string objectMapCacheKey = "ObjectMap_";
        private const string schemaCacheKey = "Schema_";

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateObjectFromReader creates an object of a specified type from the
        /// provided DataReader
        /// </summary>
        /// <param name="objType">The type of the Object</param>
        /// <param name="dr">The IDataReader to use to fill the object</param>
        /// <param name="closeReader">A flag that indicates whether the DataReader should be closed</param>
        /// <returns>The object (TObject)</returns>
        /// <history>
        /// 	[cnurse]	11/30/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static object CreateObjectFromReader(Type objType, IDataReader dr, bool closeReader)
        {
            object objObject = null;
            bool isSuccess = Null.NullBoolean;
            bool canRead = true;
            if (closeReader)
            {
                canRead = false;
                if (dr.Read())
                {
                    canRead = true;
                }
            }
            try
            {
                if (canRead)
                {
                    objObject = CreateObject(objType, false);
                    FillObjectFromReader(objObject, dr);
                }
                isSuccess = true;
            }
            finally
            {
                if ((!isSuccess))
                {
                    closeReader = true;
                }
                CloseDataReader(dr, closeReader);
            }
            return objObject;
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// FillDictionaryFromReader fills a dictionary of objects of a specified type
		/// from a DataReader.
		/// </summary>
		/// <typeparam name="TKey">The type of the key</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="keyField">The key field for the object.  This is used as
		/// the key in the Dictionary.</param>
		/// <param name="dr">The IDataReader to use to fill the objects</param>
		/// <param name="objDictionary">The Dictionary to fill.</param>
		/// <returns>A Dictionary of objects (T)</returns>
		/// <history>
		/// 	[cnurse]	11/30/2007	Created
		/// </history>
		/// -----------------------------------------------------------------------------
        private static IDictionary<TKey, TValue> FillDictionaryFromReader<TKey, TValue>(string keyField, IDataReader dr, IDictionary<TKey, TValue> objDictionary)
        {
            TValue objObject;
            TKey keyValue = default(TKey);
            try
            {
                while (dr.Read())
                {
                    objObject = (TValue) CreateObjectFromReader(typeof (TValue), dr, false);
                    if (keyField == "KeyID" && objObject is IHydratable)
                    {
                        keyValue = (TKey) Null.SetNull(((IHydratable) objObject).KeyID, keyValue);
                    }
                    else
                    {
                        if (typeof (TKey).Name == "Int32" && dr[keyField].GetType().Name == "Decimal")
                        {
                            keyValue = (TKey) Null.SetNull(dr[keyField], keyValue);
                        }
                        else if (typeof (TKey).Name.ToLower() == "string" && dr[keyField].GetType().Name.ToLower() == "dbnull")
                        {
                            keyValue = (TKey) Null.SetNull(dr[keyField], "");
                        }
                        else
                        {
                            keyValue = (TKey) Null.SetNull(dr[keyField], keyValue);
                        }
                    }
                    if (objObject != null)
                    {
                        objDictionary[keyValue] = objObject;
                    }
                }
            }
            finally
            {
                CloseDataReader(dr, true);
            }
            return objDictionary;
        }

        private static IList FillListFromReader(Type objType, IDataReader dr, IList objList, bool closeReader)
        {
            object objObject;
            bool isSuccess = Null.NullBoolean;
            try
            {
                while (dr.Read())
                {
                    objObject = CreateObjectFromReader(objType, dr, false);
                    objList.Add(objObject);
                }
                isSuccess = true;
            }
            finally
            {
                if ((!isSuccess))
                {
                    closeReader = true;
                }
                CloseDataReader(dr, closeReader);
            }
            return objList;
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// FillListFromReader fills a list of objects of a specified type
		/// from a DataReader
		/// </summary>
		/// <param name="dr">The IDataReader to use to fill the objects</param>
		/// <param name="objList">The List to Fill</param>
		/// <param name="closeReader">A flag that indicates whether the DataReader should be closed</param>
		/// <returns>A List of objects (TItem)</returns>
		/// <remarks></remarks>
		/// <history>
		/// 	[cnurse]	11/30/2007	Created
		/// </history>
		/// -----------------------------------------------------------------------------
        private static IList<TItem> FillListFromReader<TItem>(IDataReader dr, IList<TItem> objList, bool closeReader)
        {
            TItem objObject;
            bool isSuccess = Null.NullBoolean;
            try
            {
                while (dr.Read())
                {
                    objObject = (TItem) CreateObjectFromReader(typeof (TItem), dr, false);
                    objList.Add(objObject);
                }
                isSuccess = true;
            }
            finally
            {
                if ((!isSuccess))
                {
                    closeReader = true;
                }
                CloseDataReader(dr, closeReader);
            }
            return objList;
        }

        private static void FillObjectFromReader(object objObject, IDataReader dr)
        {
            try
            {
                if (objObject is IHydratable)
                {
                    var objHydratable = objObject as IHydratable;
                    if (objHydratable != null)
                    {
                        objHydratable.Fill(dr);
                    }
                }
                else
                {
                    HydrateObject(objObject, dr);
                }
            }
            catch (IndexOutOfRangeException iex)
            {
                if (Host.ThrowCBOExceptions)
                {
                    throw new ObjectHydrationException("Error Reading DataReader", iex, objObject.GetType(), dr);
                }
                else
                {
                    Exceptions.LogException(iex);
                }
            }
        }

        private static void HydrateObject(object hydratedObject, IDataReader dr)
        {
            PropertyInfo objPropertyInfo = null;
            Type propType = null;
            object coloumnValue;
            Type objDataType;
            int intIndex;
            ObjectMappingInfo objMappingInfo = GetObjectMapping(hydratedObject.GetType());
            if (hydratedObject is BaseEntityInfo && !(hydratedObject is ScheduleItem))
            {
                ((BaseEntityInfo) hydratedObject).FillBaseProperties(dr);
            }
            for (intIndex = 0; intIndex <= dr.FieldCount - 1; intIndex++)
            {
                if (objMappingInfo.Properties.TryGetValue(dr.GetName(intIndex).ToUpperInvariant(), out objPropertyInfo))
                {
                    propType = objPropertyInfo.PropertyType;
                    if (objPropertyInfo.CanWrite)
                    {
                        coloumnValue = dr.GetValue(intIndex);
                        objDataType = coloumnValue.GetType();
                        if (coloumnValue == null || coloumnValue == DBNull.Value)
                        {
                            objPropertyInfo.SetValue(hydratedObject, Null.SetNull(objPropertyInfo), null);
                        }
                        else if (propType.Equals(objDataType))
                        {
                            objPropertyInfo.SetValue(hydratedObject, coloumnValue, null);
                        }
                        else
                        {
                            if (propType.BaseType.Equals(typeof (Enum)))
                            {
                                if (Regex.IsMatch(coloumnValue.ToString(), "^\\d+$"))
                                {
                                    objPropertyInfo.SetValue(hydratedObject, Enum.ToObject(propType, Convert.ToInt32(coloumnValue)), null);
                                }
                                else
                                {
                                    objPropertyInfo.SetValue(hydratedObject, Enum.ToObject(propType, coloumnValue), null);
                                }
                            }
                            else if (propType == typeof (Guid))
                            {
                                objPropertyInfo.SetValue(hydratedObject, Convert.ChangeType(new Guid(coloumnValue.ToString()), propType), null);
                            }
                            else if (propType == typeof (Version))
                            {
                                objPropertyInfo.SetValue(hydratedObject, new Version(coloumnValue.ToString()), null);
                            }
                            else if (coloumnValue is IConvertible)
                            {
                                objPropertyInfo.SetValue(hydratedObject, Convert.ChangeType(coloumnValue, propType), null);
                            }
                            else
                            {
                                // try explicit conversion
                                objPropertyInfo.SetValue(hydratedObject, coloumnValue, null);
                            }
                        }
                    }
                }
            }
        }

        private static string GetCacheByProperty(Type objType)
        {
            string cacheProperty = defaultCacheByProperty;
            return cacheProperty;
        }

        private static int GetCacheTimeOutMultiplier(Type objType)
        {
            int cacheTimeOut = defaultCacheTimeOut;
            return cacheTimeOut;
        }

        private static string GetColumnName(PropertyInfo objProperty)
        {
            string columnName = objProperty.Name;
            return columnName;
        }

        private static ObjectMappingInfo GetObjectMapping(Type objType)
        {
            string cacheKey = objectMapCacheKey + objType.FullName;
            var objMap = (ObjectMappingInfo) DataCache.GetCache(cacheKey);
            if (objMap == null)
            {
                objMap = new ObjectMappingInfo();
                objMap.ObjectType = objType.FullName;
                objMap.PrimaryKey = GetPrimaryKey(objType);
                objMap.TableName = GetTableName(objType);
                foreach (PropertyInfo objProperty in objType.GetProperties())
                {
                    objMap.Properties.Add(objProperty.Name.ToUpperInvariant(), objProperty);
                    objMap.ColumnNames.Add(objProperty.Name.ToUpperInvariant(), GetColumnName(objProperty));
                }
                DataCache.SetCache(cacheKey, objMap);
            }
            return objMap;
        }

        private static string GetPrimaryKey(Type objType)
        {
            string primaryKey = defaultPrimaryKey;
            return primaryKey;
        }

        private static string GetTableName(Type objType)
        {
            string tableName = string.Empty;
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = objType.Name;
                if (tableName.EndsWith("Info"))
                {
                    tableName.Replace("Info", string.Empty);
                }
            }
            if (!string.IsNullOrEmpty(Config.GetSetting("ObjectQualifier")))
            {
                tableName = Config.GetSetting("ObjectQualifier") + tableName;
            }
            return tableName;
        }

        public static object CloneObject(object objObject)
        {
            try
            {
                Type objType = objObject.GetType();
                object objNewObject = Activator.CreateInstance(objType);
                ObjectMappingInfo objMappingInfo = GetObjectMapping(objType);
                foreach (KeyValuePair<string, PropertyInfo> kvp in objMappingInfo.Properties)
                {
                    PropertyInfo objProperty = kvp.Value;
                    if (objProperty.CanWrite)
                    {
                        var objPropertyClone = objProperty.GetValue(objObject, null) as ICloneable;
                        if (objPropertyClone == null)
                        {
                            objProperty.SetValue(objNewObject, objProperty.GetValue(objObject, null), null);
                        }
                        else
                        {
                            objProperty.SetValue(objNewObject, objPropertyClone.Clone(), null);
                        }
                        var enumerable = objProperty.GetValue(objObject, null) as IEnumerable;
                        if (enumerable != null)
                        {
                            var list = objProperty.GetValue(objNewObject, null) as IList;
                            if (list != null)
                            {
                                foreach (object obj in enumerable)
                                {
                                    list.Add(CloneObject(obj));
                                }
                            }
                            var dic = objProperty.GetValue(objNewObject, null) as IDictionary;
                            if (dic != null)
                            {
                                foreach (DictionaryEntry de in enumerable)
                                {
                                    dic.Add(de.Key, CloneObject(de.Value));
                                }
                            }
                        }
                    }
                }
                return objNewObject;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
                return null;
            }
        }

        public static void CloseDataReader(IDataReader dr, bool closeReader)
        {
            if (dr != null && closeReader)
            {
                dr.Close();
            }
        }

        public static TObject CreateObject<TObject>()
        {
            return (TObject) CreateObject(typeof (TObject), false);
        }

        public static TObject CreateObject<TObject>(bool initialise)
        {
            return (TObject) CreateObject(typeof (TObject), initialise);
        }

        public static object CreateObject(Type objType, bool initialise)
        {
            object objObject = Activator.CreateInstance(objType);
            if (initialise)
            {
                InitializeObject(objObject);
            }
            return objObject;
        }

        public static TObject DeserializeObject<TObject>(string fileName)
        {
            return DeserializeObject<TObject>(XmlReader.Create(new FileStream(fileName, FileMode.Open, FileAccess.Read)));
        }

        public static TObject DeserializeObject<TObject>(XmlDocument document)
        {
            return DeserializeObject<TObject>(XmlReader.Create(new StringReader(document.OuterXml)));
        }

        public static TObject DeserializeObject<TObject>(Stream stream)
        {
            return DeserializeObject<TObject>(XmlReader.Create(stream));
        }

        public static TObject DeserializeObject<TObject>(TextReader reader)
        {
            return DeserializeObject<TObject>(XmlReader.Create(reader));
        }

        public static TObject DeserializeObject<TObject>(XmlReader reader)
        {
            var objObject = CreateObject<TObject>(true);
            var xmlSerializableObject = objObject as IXmlSerializable;
            if (xmlSerializableObject == null)
            {
                var serializer = new XmlSerializer(objObject.GetType());
                objObject = (TObject) serializer.Deserialize(reader);
            }
            else
            {
                xmlSerializableObject.ReadXml(reader);
            }
            return objObject;
        }

        public static ArrayList FillCollection(IDataReader dr, Type objType)
        {
            return (ArrayList) FillListFromReader(objType, dr, new ArrayList(), true);
        }

        public static ArrayList FillCollection(IDataReader dr, Type objType, bool closeReader)
        {
            return (ArrayList) FillListFromReader(objType, dr, new ArrayList(), closeReader);
        }

        public static IList FillCollection(IDataReader dr, Type objType, ref IList objToFill)
        {
            return FillListFromReader(objType, dr, objToFill, true);
        }

        public static List<TItem> FillCollection<TItem>(IDataReader dr)
        {
            return (List<TItem>) FillListFromReader(dr, new List<TItem>(), true);
        }

        public static IList<TItem> FillCollection<TItem>(IDataReader dr, ref IList<TItem> objToFill)
        {
            return FillListFromReader(dr, objToFill, true);
        }

        public static IList<TItem> FillCollection<TItem>(IDataReader dr, IList<TItem> objToFill, bool closeReader)
        {
            return FillListFromReader(dr, objToFill, closeReader);
        }

        public static ArrayList FillCollection(IDataReader dr, ref Type objType, ref int totalRecords)
        {
            var objFillCollection = (ArrayList) FillListFromReader(objType, dr, new ArrayList(), false);
            try
            {
                if (dr.NextResult())
                {
                    totalRecords = Globals.GetTotalRecords(ref dr);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                CloseDataReader(dr, true);
            }
            return objFillCollection;
        }

        public static List<T> FillCollection<T>(IDataReader dr, ref int totalRecords)
        {
            IList<T> objFillCollection = FillCollection(dr, new List<T>(), false);
            try
            {
                if (dr.NextResult())
                {
                    totalRecords = Globals.GetTotalRecords(ref dr);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                CloseDataReader(dr, true);
            }
            return (List<T>) objFillCollection;
        }

        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr) where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, new Dictionary<int, TItem>());
        }

        public static IDictionary<int, TItem> FillDictionary<TItem>(IDataReader dr, ref IDictionary<int, TItem> objToFill) where TItem : IHydratable
        {
            return FillDictionaryFromReader("KeyID", dr, objToFill);
        }

        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr)
        {
            return (Dictionary<TKey, TValue>) FillDictionaryFromReader(keyField, dr, new Dictionary<TKey, TValue>());
        }

        public static Dictionary<TKey, TValue> FillDictionary<TKey, TValue>(string keyField, IDataReader dr, IDictionary<TKey, TValue> objDictionary)
        {
            return (Dictionary<TKey, TValue>) FillDictionaryFromReader(keyField, dr, objDictionary);
        }

        public static TObject FillObject<TObject>(IDataReader dr)
        {
            return (TObject) CreateObjectFromReader(typeof (TObject), dr, true);
        }

        public static TObject FillObject<TObject>(IDataReader dr, bool closeReader)
        {
            return (TObject) CreateObjectFromReader(typeof (TObject), dr, closeReader);
        }

        public static object FillObject(IDataReader dr, Type objType)
        {
            return CreateObjectFromReader(objType, dr, true);
        }

        public static object FillObject(IDataReader dr, Type objType, bool closeReader)
        {
            return CreateObjectFromReader(objType, dr, closeReader);
        }

        public static IQueryable<TItem> FillQueryable<TItem>(IDataReader dr)
        {
            return FillListFromReader(dr, new List<TItem>(), true).AsQueryable();
        }

        public static SortedList<TKey, TValue> FillSortedList<TKey, TValue>(string keyField, IDataReader dr)
        {
            return (SortedList<TKey, TValue>) FillDictionaryFromReader(keyField, dr, new SortedList<TKey, TValue>());
        }

        public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired);
        }

        public static TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary)
        {
            return DataCache.GetCachedData<TObject>(cacheItemArgs, cacheItemExpired, saveInDictionary);
        }

        public static Dictionary<string, PropertyInfo> GetProperties<TObject>()
        {
            return GetObjectMapping(typeof (TObject)).Properties;
        }

        public static Dictionary<string, PropertyInfo> GetProperties(Type objType)
        {
            return GetObjectMapping(objType).Properties;
        }

        public static void InitializeObject(object objObject)
        {
            foreach (PropertyInfo objPropertyInfo in GetObjectMapping(objObject.GetType()).Properties.Values)
            {
                if (objPropertyInfo.CanWrite)
                {
                    objPropertyInfo.SetValue(objObject, Null.SetNull(objPropertyInfo), null);
                }
            }
        }

        public static object InitializeObject(object objObject, Type objType)
        {
            foreach (PropertyInfo objPropertyInfo in GetObjectMapping(objType).Properties.Values)
            {
                if (objPropertyInfo.CanWrite)
                {
                    objPropertyInfo.SetValue(objObject, Null.SetNull(objPropertyInfo), null);
                }
            }
            return objObject;
        }

        public static void SerializeObject(object objObject, string fileName)
        {
            using (XmlWriter writer = XmlWriter.Create(fileName, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        public static void SerializeObject(object objObject, XmlDocument document)
        {
            var sb = new StringBuilder();
            SerializeObject(objObject, XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Document)));
            document.LoadXml(sb.ToString());
        }

        public static void SerializeObject(object objObject, Stream stream)
        {
            using (XmlWriter writer = XmlWriter.Create(stream, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        public static void SerializeObject(object objObject, TextWriter textWriter)
        {
            using (XmlWriter writer = XmlWriter.Create(textWriter, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Fragment)))
            {
                SerializeObject(objObject, writer);
                writer.Flush();
            }
        }

        public static void SerializeObject(object objObject, XmlWriter writer)
        {
            var xmlSerializableObject = objObject as IXmlSerializable;
            if (xmlSerializableObject == null)
            {
                var serializer = new XmlSerializer(objObject.GetType());
                serializer.Serialize(writer, objObject);
            }
            else
            {
                xmlSerializableObject.WriteXml(writer);
            }
        }

        public static void DeserializeSettings(IDictionary dictionary, XmlNode rootNode, string elementName)
        {
            string sKey = null;
            string sValue = null;

            foreach (XmlNode settingNode in rootNode.SelectNodes(elementName))
            {
                sKey = XmlUtils.GetNodeValue(settingNode.CreateNavigator(), "settingname");
                sValue = XmlUtils.GetNodeValue(settingNode.CreateNavigator(), "settingvalue");

                dictionary[sKey] = sValue;
            }
        }

        ///<summary>
        ///  Iterates items in a IDictionary object and generates XML nodes
        ///</summary>
        ///<param name = "dictionary">The IDictionary to iterate</param>
        ///<param name = "document">The XML document the node should be added to</param>
        ///<param name="targetPath">Path at which to serialize settings</param>
        ///<param name = "elementName">The name of the new element created</param>
        ///<remarks>
        ///</remarks>
        ///<history>
        ///  [jlucarino]	09/18/2009	created
        ///  [kbeigi] updated to IDictionary
        ///</history>
        public static void SerializeSettings(IDictionary dictionary, XmlDocument document, string targetPath, string elementName)
        {
            string sOuterElementName = elementName + "s";
            string sInnerElementName = elementName;
            XmlNode nodeSetting = default(XmlNode);
            XmlNode nodeSettings = default(XmlNode);
            XmlNode nodeSettingName = default(XmlNode);
            XmlNode nodeSettingValue = default(XmlNode);

            XmlNode targetNode = document.SelectSingleNode(targetPath);

            if (targetNode != null)
            {
                nodeSettings = targetNode.AppendChild(document.CreateElement(sOuterElementName));
                foreach (object sKey in dictionary.Keys)
                {
                    nodeSetting = nodeSettings.AppendChild(document.CreateElement(sInnerElementName));

                    nodeSettingName = nodeSetting.AppendChild(document.CreateElement("settingname"));
                    nodeSettingName.InnerText = sKey.ToString();

                    nodeSettingValue = nodeSetting.AppendChild(document.CreateElement("settingvalue"));
                    nodeSettingValue.InnerText = dictionary[sKey].ToString();
                }
            }
            else
            {
                throw new ArgumentException("Invalid Target Path");
            }
        }


        [Obsolete("Obsolete in DotNetNuke 5.0.  Replaced by GetProperties(Of TObject)() ")]
        public static ArrayList GetPropertyInfo(Type objType)
        {
            var arrProperties = new ArrayList();
            ObjectMappingInfo objMappingInfo = GetObjectMapping(objType);
            arrProperties.AddRange(objMappingInfo.Properties.Values);
            return arrProperties;
        }

        [Obsolete("Obsolete in DotNetNuke 5.0.  Replaced by SerializeObject(Object) ")]
        public static XmlDocument Serialize(object objObject)
        {
            var document = new XmlDocument();
            SerializeObject(objObject, document);
            return document;
        }
    }
}