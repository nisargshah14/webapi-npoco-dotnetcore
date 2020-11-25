﻿using System;
using System.Collections.Generic;
using System.Text;
using NPoco;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Data;
using Architecture.Common.Models;
using Architecture.Common.Infrastructure.Attributes;
using Architecture.Common.Infrastructure;

namespace Architecture.Data.Repository
{
    public class BaseRepository
    {
        private readonly IDatabase _db;
        public BaseRepository()
        {
            _db = new Database(ConfigSettings.ConnectionString, DatabaseType.SqlServer2012, SqlClientFactory.Instance);
        }
        public BaseRepository(BaseRepository bd)
        {
            _db = bd._db; //new Database(ConfigSettings.ConnectionStringName);
        }

        public T SetDefaultValues<T>(T item, long loggedInUserID = 0) where T : class
        {
            if (_db.IsNew(item))
            {
                foreach (var prop in item.GetType().GetProperties().Where(q => q.IsDefined(typeof(SetValueOnAddAttribute), false)))
                {
                    if (null != prop && prop.CanWrite)
                    {
                        foreach (SetValueOnAddAttribute attr in Attribute.GetCustomAttributes(prop).OfType<SetValueOnAddAttribute>())
                        {
                            switch (attr.SetValue)
                            {
                                case (int)SetValueConstants.CurrentTime:
                                    prop.SetValue(item, DateTime.UtcNow, null);
                                    break;
                                case (int)SetValueConstants.LoggedInUserId:
                                    prop.SetValue(item, loggedInUserID, null);
                                    break;
                            }
                        }

                    }
                }
            }
            else
            {
                foreach (var prop in item.GetType().GetProperties().Where(q => q.IsDefined(typeof(SetValueOnUpdateAttribute), false)))
                {
                    if (null != prop && prop.CanWrite)
                    {
                        foreach (SetValueOnUpdateAttribute attr in Attribute.GetCustomAttributes(prop).OfType<SetValueOnUpdateAttribute>())
                        {
                            switch (attr.SetValueOnUpdate)
                            {
                                case (int)SetValueConstants.CurrentTime:
                                    prop.SetValue(item, DateTime.UtcNow, null);
                                    break;
                                case (int)SetValueConstants.LoggedInUserId:
                                    prop.SetValue(item, loggedInUserID, null);
                                    break;
                            }
                        }
                    }
                }
            }
            return item;
        }


        public T SaveEntity<T>(T item, long userID, bool IsSetDefault = true) where T : class
        {
            //userID = userID > 0 ? userID : (SessionHelper.UserId > 0) ? SessionHelper.UserId : -1;
            if (IsSetDefault)
                SetDefaultValues(item, userID);

            if (_db.IsNew(item))
            {
                _db.Insert(item);
            }
            else
            {
                _db.Update(item);
            }
            return item;
        }

        public void DeleteEntity<T>(long id) where T : class
        {
            _db.Delete<T>(id);
        }

        public object ExecQuery(string query)
        {
            return _db.Execute(query);
        }

        public object GetScalar(string spname, List<SearchValueData> searchParam = null)
        {
            return _db.ExecuteScalar<object>(GetSPString(spname, searchParam));
        }

        public object GetScalar(string sqlquery)
        {
            return _db.ExecuteScalar<object>(sqlquery);
        }


        protected string GetSortKeyName<T>()
        {
            return (typeof(T).GetCustomAttributes(typeof(SortAttribute), true)[0] as SortAttribute).KeyValue;
        }

        protected string GetStoreProcedureName<T>()
        {
            return (typeof(T).GetCustomAttributes(typeof(StoreProcedureAttribute), true)[0] as StoreProcedureAttribute).StoreProcedureName;
        }

        protected string GetSortDirection<T>()
        {
            return (typeof(T).GetCustomAttributes(typeof(SortAttribute), true)[0] as SortAttribute).DirectionValue;
        }

        protected string GetTableName<T>()
        {
            return (typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true)[0] as TableNameAttribute).Value;
        }

        protected string GetPrimaryKeyName<T>()
        {
            return (typeof(T).GetCustomAttributes(typeof(PrimaryKeyAttribute), true)[0] as PrimaryKeyAttribute).Value;
        }

        private string GetSPString(string spname, List<SearchValueData> searchParam)
        {
            string sp = string.Format("EXEC {0}", spname);

            if (searchParam != null)
            {
                sp = searchParam.Aggregate(sp, (current, searchValueData) => current + string.Format(" @@{0} = '{1}',", searchValueData.Name, searchValueData.Value == null ? searchValueData.Value : searchValueData.Value.Replace("@", "@@").Replace("'", "''")));
                if (searchParam.Any())
                    sp = sp.TrimEnd(',');
            }
            return sp;
        }

        public int GetEntityCount<T>(List<SearchValueData> searchParam = null, string customWhere = "") where T : class, new()
        {
            return _db.ExecuteScalar<int>(GetFilterString<T>(searchParam, "", "", customWhere, true));
        }

        private string GetFilterString<T>(List<SearchValueData> searchParam, string sortIndex = "", string sortDirection = "", string customWhere = "", bool getCount = false) where T : class, new()
        {
            string select = (getCount ? "SELECT COUNT(*) FROM " : "SELECT * FROM ");
            return GetFilterSQLString<T>(searchParam, sortIndex, sortDirection, customWhere, !getCount, select);
        }

        private string GetFilterSQLString<T>(List<SearchValueData> searchParam, string sortIndex, string sortDirection, string customWhere,
                                              bool allowSort, string select) where T : class, new()
        {
            T item = new T();
            string where = "";
            string tableName = GetTableName<T>();
            select += tableName;

            if (searchParam != null && searchParam.Count > 0)
            {
                where += " WHERE 1=1";
                foreach (SearchValueData val in searchParam)
                {
                    string value = val.Value.Replace("'", "''");
                    PropertyInfo propertyInfo = item.GetType().GetProperty(val.Name);
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        if (val.IsEqual)
                            where += " AND " + val.Name + " = '" + value + "'";
                        else if (val.IsNotEqual)
                            where += " AND " + val.Name + " != '" + value + "'";
                        else
                            where += " AND " + val.Name + " LIKE '%" + value + "%'";
                    }
                    else if (propertyInfo.PropertyType == typeof(bool) || propertyInfo.PropertyType == typeof(bool?))
                    {
                        where += " AND " + val.Name + "=" + value;
                    }
                    else if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?) ||
                             propertyInfo.PropertyType == typeof(long?) || propertyInfo.PropertyType == typeof(long))
                    {
                        if (val.IsNotEqual)
                            where += " AND " + val.Name + "!=" + value;
                        else
                            where += " AND " + val.Name + "=" + value;
                    }
                    else if (propertyInfo.PropertyType == typeof(DateTime))
                    {
                        if (searchParam.Count(a => a.Name == val.Name) > 1)
                        {
                            where += " AND ((" + val.Name + " BETWEEN " +
                                     searchParam.FirstOrDefault(a => a.Name == val.Name).Value + " AND " +
                                     searchParam.LastOrDefault(a => a.Name == val.Name).Value + ") OR (" + val.Name +
                                     " BETWEEN " + searchParam.LastOrDefault(a => a.Name == val.Name).Value +
                                     " AND " + searchParam.FirstOrDefault(a => a.Name == val.Name).Value + "))";
                        }
                        else
                        {
                            where += " AND " + val.Name + "='" + value + "'";
                        }
                    }
                }
            }

            where += customWhere != ""
                         ? string.IsNullOrEmpty(where) ? " WHERE (" + customWhere + ")" : " AND (" + customWhere + ")"
                         : "";

            if (sortIndex != "")
                where += " ORDER BY " + sortIndex + " " + sortDirection;
            else if (!string.IsNullOrEmpty(GetSortKeyName<T>()) && !string.IsNullOrEmpty(GetSortDirection<T>()) && allowSort)
            {
                where += " ORDER BY " + GetSortKeyName<T>() + " " + GetSortDirection<T>();
            }

            return select + where.Replace("1=1 AND", "").Replace("1=1", "").Replace("@", "@@");
        }

        public T GetEntity<T>(long id) where T : class, new()
        {
            T item = new T();
            string tableName = GetTableName<T>();
            string primaryKeyName = GetPrimaryKeyName<T>();
            return _db.SingleOrDefault<T>("SELECT * FROM " + tableName + " WHERE " + primaryKeyName + "=" + id) ?? item;
        }

        public T GetEntityFromQuery<T>(string query) where T : class, new()
        {
            return GetEntityListFromQuery<T>(query).SingleOrDefault();
        }

        public T GetEntity<T>(string spname, List<SearchValueData> searchParam = null) where T : class, new()
        {
            return GetEntityList<T>(spname, searchParam).SingleOrDefault();
        }

        public T GetEntity<T>(List<SearchValueData> searchParam = null, string customWhere = "", string sortIndex = "", string sortDirection = "") where T : class, new()
        {
            return _db.SingleOrDefault<T>(GetFilterString<T>(searchParam, sortIndex, sortDirection, customWhere));
        }

        public List<T> GetEntityList<T>(string spname, List<SearchValueData> searchParam = null) where T : class, new()
        {
            return _db.Fetch<T>(";" + GetSPString(spname, searchParam));
        }

        public List<T> GetEntityListFromQuery<T>(string query) where T : class, new()
        {
            return _db.Fetch<T>(";" + query);
        }

        public List<dynamic> GetDynamicList(string spname, List<SearchValueData> searchParam = null)
        {
            return _db.Fetch<dynamic>(";" + GetSPString(spname, searchParam));
        }

        public List<T> GetEntityList<T>(List<SearchValueData> searchParam = null, string customWhere = "", string sortIndex = "", string sortDirection = "") where T : class, new()
        {
            return _db.Query<T>(GetFilterString<T>(searchParam, sortIndex, sortDirection, customWhere)).ToList<T>();
        }

        public List<T> GetEntityList<T>(string sqlQuery) where T : class, new()
        {
            return _db.Query<T>(sqlQuery).ToList<T>();
        }

        public Page<T> GetEntityPageList<T>(List<SearchValueData> searchParam, int? pageSizeCount, int pageIndex, string sortIndex, string sortDirection, string customWhere = "") where T : class, new()
        {
            int pageSize = ConfigSettings.PageSize;

            if (pageSizeCount != null)
                pageSize = pageSizeCount.Value;

            if (sortIndex == "")
                sortIndex = GetSortKeyName<T>();

            if (sortDirection == "")
                sortDirection = GetSortDirection<T>();

            Page<T> pageList = _db.Page<T>(pageIndex, pageSize, GetFilterString<T>(searchParam, sortIndex, sortDirection, customWhere));

            //To get last page data if current page index is greater than last page
            if (pageList.Items.Count == 0 && pageList.TotalPages > 0 && pageList.TotalItems > 0)
                pageList = _db.Page<T>(pageList.TotalPages, pageSize, GetFilterString<T>(searchParam, sortIndex, sortDirection, customWhere));

            return pageList;
        }

        public Page<T> GetEntityPageList<T>(string spName, List<SearchValueData> searchParam, int pageSize, int pageIndex, string sortIndex, string sortDirection, string sortIndexArray = "") where T : class, new()
        {
            var searchValueData = new SearchValueData { Name = "SortExpression", Value = Convert.ToString(sortIndex) };
            searchParam.Add(searchValueData);

            searchValueData = new SearchValueData { Name = "SortType", Value = Convert.ToString(sortDirection) };
            searchParam.Add(searchValueData);

            searchValueData = new SearchValueData { Name = "FromIndex", Value = Convert.ToString(((pageIndex - 1) * pageSize) + 1) };
            searchParam.Add(searchValueData);

            searchValueData = new SearchValueData { Name = "ToIndex", Value = Convert.ToString(pageIndex * pageSize) };
            searchParam.Add(searchValueData);

            if (!string.IsNullOrEmpty(sortIndexArray))//For Multi line sorting
            {
                searchValueData = new SearchValueData { Name = "SortIndexArray", Value = Convert.ToString(sortIndexArray) };
                searchParam.Add(searchValueData);
            }

            List<T> records = GetEntityList<T>(spName, searchParam);

            int count = 0;
            if (records != null && records.Count > 0)
            {
                object obj = records.First();
                PropertyInfo prop = obj.GetType().GetProperty("Count");
                count = Convert.ToInt32(prop.GetValue(obj));
            }

            Page<T> pageRecords = GetPageInStoredProcResultSet(pageIndex, pageSize, count, records);
            return pageRecords;
        }

        private string GetFilterString<T>(List<SearchValueData> searchParam, string sortIndex = "", string sortDirection = "", string customWhere = "") where T : class, new()
        {
            T item = new T();
            string tableName = GetTableName<T>();
            string select = "SELECT * FROM " + tableName;
            string where = "";

            if (searchParam != null && searchParam.Count > 0)
            {
                where += " WHERE 1=1";
                foreach (SearchValueData val in searchParam)
                {
                    string value = val.Value.Replace("'", "''");
                    PropertyInfo propertyInfo = item.GetType().GetProperty(val.Name);
                    var propertyType = propertyInfo.PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propertyType = propertyType.GetGenericArguments()[0];
                    }

                    if (propertyType == typeof(string) || propertyType == typeof(char))
                    {
                        if (val.IsEqual)
                            where += " AND " + val.Name + " = '" + value + "'";
                        else if (val.IsNotEqual)
                            where += " AND " + val.Name + " != '" + value + "'";
                        else if (val.IsNull)
                            where += " AND " + val.Name + " IS NULL ";
                        else
                            where += " AND " + val.Name + " LIKE '%" + value + "%'";
                    }
                    else if (propertyType == typeof(bool))
                    {
                        where += " AND " + val.Name + "=" + value;
                    }
                    else if (propertyType == typeof(int) || propertyType == typeof(long) || propertyType == typeof(decimal) || propertyType == typeof(float))
                    {
                        if (val.IsNotEqual)
                            where += " AND " + val.Name + "!=" + value;
                        else if (val.IsNull)
                            where += " AND " + val.Name + " IS NULL ";
                        else
                            where += " AND " + val.Name + "=" + value;
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        if (searchParam.Count(a => a.Name == val.Name) > 1)
                        {
                            where += " AND ((" + val.Name + " BETWEEN " + searchParam.First(a => a.Name == val.Name).Value + " AND " + searchParam.Last(a => a.Name == val.Name).Value + ") OR (" + val.Name + " BETWEEN " + searchParam.Last(a => a.Name == val.Name).Value + " AND " + searchParam.First(a => a.Name == val.Name).Value + "))";
                        }
                        else
                        {
                            where += " AND " + val.Name + "='" + value + "'";
                        }
                    }
                }
            }

            where += customWhere != "" ? string.IsNullOrEmpty(where) ? " WHERE (" + customWhere + ")" : " AND (" + customWhere + ")" : "";

            if (sortIndex != "")
                where += " ORDER BY " + sortIndex + " " + sortDirection;

            return select + where.Replace("1=1 AND", "").Replace("1=1", "").Replace("@", "@@");
        }

        public Page<T> GetPageInStoredProcResultSet<T>(int pageIndex, int pageSize, int count, List<T> itemsList) where T : class, new()
        {
            var result = new Page<T>
            {
                CurrentPage = pageIndex,
                ItemsPerPage = pageSize,
                TotalItems = count
            };
            result.TotalPages = result.TotalItems / pageSize;

            if ((result.TotalItems % pageSize) != 0)
                result.TotalPages++;

            result.Items = itemsList;
            return result;
        }

        #region Add For Generic List and Search

        public ServiceResponse GetPageRecords<T>(int pageSize, int pageIndex, string sortIndex, string sortDirection, SearchModel searchModel = null) where T : class, new()
        {
            ServiceResponse response = new ServiceResponse();
            if (searchModel != null && !string.IsNullOrEmpty(searchModel.Value))
                searchModel.Value = searchModel.Value.Replace("'", "''");

            string spName = GetStoreProcedureName<T>();

            List<SearchValueData> searchParam = new List<SearchValueData>();

            if (pageSize != Constants.AllRecordsConstant && pageIndex != Constants.AllRecordsConstant)
            {
                searchParam.Add(new SearchValueData
                {
                    Name = "FromIndex",
                    Value = Convert.ToString(((pageIndex - 1) * pageSize) + 1)
                });
                searchParam.Add(new SearchValueData { Name = "ToIndex", Value = Convert.ToString(pageIndex * pageSize) });
            }
            else
            {
                searchParam.Add(new SearchValueData
                {
                    Name = "FromIndex",
                    Value = Convert.ToString(Constants.AllRecordsConstant)
                });
                searchParam.Add(new SearchValueData
                {
                    Name = "ToIndex",
                    Value = Convert.ToString(Constants.AllRecordsConstant)
                });
            }

            searchParam.Add(new SearchValueData { Name = "SortExpression", Value = string.IsNullOrEmpty(sortIndex) ? GetSortKeyName<T>() : Convert.ToString(sortIndex) });

            searchParam.Add(new SearchValueData { Name = "SortType", Value = string.IsNullOrEmpty(sortDirection) ? GetSortDirection<T>() : Convert.ToString(sortDirection) });

            string customWhere = (searchModel != null && !string.IsNullOrEmpty(searchModel.Value)) ? "Where (1=1)" + GetCustomWhere<T>(searchModel) : "";

            searchParam.Add(new SearchValueData { Name = "CustomWhere", Value = customWhere });

            List<T> records = GetEntityList<T>(spName, searchParam);

            int count = 0;
            if (records != null && records.Count > 0)
            {
                object obj = records.First();
                PropertyInfo prop = obj.GetType().GetProperty("Count");
                count = Convert.ToInt32(prop.GetValue(obj, new Object[] { 0 }));
            }

            Page<T> pageRecords = GetPageInStoredProcResultSet(pageIndex, pageSize, count, records);
            response.Data = pageRecords;
            response.IsSuccess = true;
            return response;
        }


        public ServiceResponse GetPageRecords<T>(int pageSize, int pageIndex, string sortIndex, string sortDirection, List<SearchModel> searchModelList) where T : class, new()
        {
            ServiceResponse response = new ServiceResponse();
            //if (searchModel != null && !string.IsNullOrEmpty(searchModel.Value))
            //    searchModel.Value = searchModel.Value.Replace("'", "''");

            string spName = GetStoreProcedureName<T>();

            List<SearchValueData> searchParam = new List<SearchValueData>();

            if (pageSize != Constants.AllRecordsConstant && pageIndex != Constants.AllRecordsConstant)
            {
                searchParam.Add(new SearchValueData
                {
                    Name = "FromIndex",
                    Value = Convert.ToString(((pageIndex - 1) * pageSize) + 1)
                });
                searchParam.Add(new SearchValueData { Name = "ToIndex", Value = Convert.ToString(pageIndex * pageSize) });
            }
            else
            {
                searchParam.Add(new SearchValueData
                {
                    Name = "FromIndex",
                    Value = Convert.ToString(Constants.AllRecordsConstant)
                });
                searchParam.Add(new SearchValueData
                {
                    Name = "ToIndex",
                    Value = Convert.ToString(Constants.AllRecordsConstant)
                });
            }

            searchParam.Add(new SearchValueData { Name = "SortExpression", Value = string.IsNullOrEmpty(sortIndex) ? GetSortKeyName<T>() : Convert.ToString(sortIndex) });

            searchParam.Add(new SearchValueData { Name = "SortType", Value = string.IsNullOrEmpty(sortDirection) ? GetSortDirection<T>() : Convert.ToString(sortDirection) });

            string customWhere = "Where (1=1)";
            foreach (var searchModel in searchModelList)
            {
                customWhere += (searchModel != null && !string.IsNullOrEmpty(searchModel.Value)) ? GetCustomWhere<T>(searchModel) : "";
            }

            searchParam.Add(new SearchValueData { Name = "CustomWhere", Value = customWhere });

            List<T> records = GetEntityList<T>(spName, searchParam);

            int count = 0;
            if (records != null && records.Count > 0)
            {
                object obj = records.First();
                PropertyInfo prop = obj.GetType().GetProperty("Count");
                count = Convert.ToInt32(prop.GetValue(obj, new Object[] { 0 }));
            }

            Page<T> pageRecords = GetPageInStoredProcResultSet(pageIndex, pageSize, count, records);
            response.Data = pageRecords;
            response.IsSuccess = true;
            return response;
        }

        public string GetCustomWhere<T>(SearchModel searchModel) where T : class, new()
        {
            string customWhere = "";

            if (string.IsNullOrEmpty(searchModel.Name))
            {
                foreach (PropertyInfo prop in typeof(T).GetProperties().Where(p => p.IsDefined(typeof(SearchAttribute), false)))
                {
                    searchModel.Name = prop.Name;
                    if (string.IsNullOrEmpty(customWhere))
                        customWhere = GetCustomWhereStringWithPassedOperator(searchModel);
                    else
                        customWhere += " OR " + GetCustomWhereStringWithPassedOperator(searchModel);
                }
                if (!string.IsNullOrEmpty(customWhere))
                    customWhere = " (" + customWhere + ")";
            }
            else
                customWhere = GetCustomWhereStringWithPassedOperator(searchModel);
            return string.IsNullOrEmpty(customWhere) ? customWhere : " AND " + customWhere;
        }

        public string GetCustomWhereStringWithPassedOperator(SearchModel searchModel)
        {
            string customWhere = "";
            switch (searchModel.OperatorId)
            {
                case (int)SearchModel.SearchOperator.EqualTo:
                    customWhere = searchModel.Name + " = '" + searchModel.Value + "'";
                    break;
                case (int)SearchModel.SearchOperator.NotEqualTo:
                    customWhere = searchModel.Name + " != '" + searchModel.Value + "'";
                    break;
                case (int)SearchModel.SearchOperator.BeginsWith:
                    customWhere = searchModel.Name + " LIKE '" + searchModel.Value + "%'";
                    break;
                case (int)SearchModel.SearchOperator.EndsWith:
                    customWhere = searchModel.Name + " LIKE '%" + searchModel.Value + "'";
                    break;
                case (int)SearchModel.SearchOperator.Contains:
                    customWhere = searchModel.Name + " LIKE '%" + searchModel.Value.Replace(" ", "%") + "%'";
                    break;
                case (int)SearchModel.SearchOperator.DoesNotContains:
                    customWhere = searchModel.Name + " NOT LIKE '%" + searchModel.Value + "%'";
                    break;
                case (int)SearchModel.SearchOperator.GreaterThan:
                    customWhere = searchModel.Name + " > '" + searchModel.Value + "'";
                    break;
                case (int)SearchModel.SearchOperator.LessThan:
                    customWhere = searchModel.Name + " < '" + searchModel.Value + "'";
                    break;
            }
            return customWhere;
        }

        public ServiceResponse DeleteRecord<T>(long id, string successMessage = "Record deleted successfully") where T : class, new()
        {
            ServiceResponse response = new ServiceResponse();
            List<SearchValueData> searchList = new List<SearchValueData>();

            searchList.Add(new SearchValueData { Name = "PrimaryKeyID", Value = Convert.ToString(id) });
            searchList.Add(new SearchValueData { Name = "PrimaryKeyName", Value = GetPrimaryKeyName<T>() });
            searchList.Add(new SearchValueData { Name = "TableName", Value = GetTableName<T>() });

            int status = Convert.ToInt32(GetScalar("DeleteRecord", searchList));
            if (status == 1)
            {
                response.Message = successMessage;
                response.IsSuccess = true;
            }

            if (status == 0)
            {
                response.Message = "Sorry, record is used in the system. You can't delete the record.";
                response.IsSuccess = false;
            }
            return response;
        }
        public ServiceResponse DeleteRecord<T>(string customWhere, string successMessage = "Record deleted successfully") where T : class, new()
        {
            ServiceResponse response = new ServiceResponse();
            List<SearchValueData> searchList = new List<SearchValueData>();
            //searchList.Add(new SearchValueData { Name = "PrimaryKeyID", Value = Convert.ToString(id) });
            //searchList.Add(new SearchValueData { Name = "PrimaryKeyName", Value = GetPrimaryKeyName<T>() });
            searchList.Add(new SearchValueData { Name = "CustomWhere", Value = customWhere });
            searchList.Add(new SearchValueData { Name = "TableName", Value = GetTableName<T>() });

            int status = Convert.ToInt32(GetScalar("DeleteRecord", searchList));
            if (status == 1)
            {
                response.Message = successMessage;
                response.IsSuccess = true;
            }

            if (status == 0)
            {
                response.Message = "Sorry, record is used in the system. You can't delete the record.";
                response.IsSuccess = false;
            }
            return response;
        }

        #endregion Add For Generic List and Search

        #region For Multiple ResutlSet

        /// <summary>
        /// Perform a multi-results set query
        /// </summary>
        /// <param name="query">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <typeparam name="T">The Type representing Model in which Result Set Data will be Bind</typeparam>
        /// <returns>Return Object of Generic Type T</returns>
        //public T GetMultipleEntity<T>(string query, params object[] args) where T : new()
        //{
        //    GridReader grd = _db.QueryMultiple(query, args);
        //    T obj = new T();
        //    Type returnType = typeof(T);
        //    PropertyInfo[] propertyInfo = returnType.GetProperties();
        //    foreach (var info in propertyInfo)
        //    {
        //        bool isFirstorDefault = false;
        //        Type t;
        //        if (info.PropertyType.GetGenericArguments().Any())
        //        {
        //            t = info.PropertyType.GetGenericArguments()[0];
        //        }
        //        else
        //        {
        //            isFirstorDefault = true;
        //            t = info.PropertyType;
        //        }
        //        var value =
        //        (typeof(BaseRepository).GetMethod("GetValueOf")
        //        .MakeGenericMethod(t)
        //        .Invoke(null, new object[] { grd, isFirstorDefault }));
        //        info.SetValue(obj, value, null);
        //    }
        //    _db.CloseSharedConnection();
        //    return obj;
        //}

        //public T GetMultipleEntity<T>(string spname, List<SearchValueData> searchParam) where T : new()
        //{            
        //    GridReader grd = _db.QueryMultiple(GetSPString(spname, searchParam));
        //    T obj = new T();
        //    Type returnType = typeof(T);
        //    PropertyInfo[] propertyInfo = returnType.GetProperties();
        //    foreach (var info in propertyInfo)
        //    {

        //        if (!info.CanWrite)
        //        {
        //            continue;
        //        }
        //        bool isFirstorDefault = false;
        //        Type t;

        //        if (info.GetCustomAttributes(typeof(IgnoreAttribute), true).Length != 0)
        //        {
        //            continue;
        //        }
        //        if (info.PropertyType.GetGenericArguments().Any())
        //        {
        //            t = info.PropertyType.GetGenericArguments()[0];
        //        }
        //        else
        //        {
        //            isFirstorDefault = true;
        //            t = info.PropertyType;
        //        }

        //        var value =
        //    (typeof(BaseRepository).GetMethod("GetValueOf")
        //    .MakeGenericMethod(t)
        //    .Invoke(null, new object[] { grd, isFirstorDefault }));
        //        info.SetValue(obj, value, null);


        //    }
        //    _db.CloseSharedConnection();
        //    return obj;
        //}

        /// <summary>
        /// Reads from a GridReader, Either List of Type T or Object if Type T
        /// </summary>
        /// <param name="reader">GridReader from which result set will be read.</param>
        /// <param name="isFirstorDefault">If true function returns first row or default value as object of type T else return List of T"></typeparam></param>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <returns>An enumerable collection of result records</returns>
        //public static object GetValueOf<T>(GridReader reader, bool isFirstorDefault)
        //{
        //    IEnumerable<T> lst = reader.Read<T>();
        //    if (isFirstorDefault)
        //        return lst.FirstOrDefault();
        //    return lst.ToList();
        //}

        /// <summary>
        /// Execute Query And Return DataSet (This Support Only for single Table ResultSet)
        /// </summary>
        /// <param name="sql">The SQL query to be executed</param>
        /// <param name="args">Arguments to any embedded parameters in the SQL</param>
        /// <returns>Dataset With Single Table Only</returns>
        //public DataSet GetDataSet(string query, params object[] args)
        //{
        //    return _db.QueryForDataSet(query, args);
        //}

        //public DataSet GetDataSet(string spname, List<SearchValueData> searchParam)
        //{
        //    return GetDataSet(GetSPString(spname, searchParam));
        //}
        //public DataTable GetDataTable(string query, params object[] args)
        //{
        //    return _db.QueryForDataSet(query, args).Tables[0];
        //}

        //public static DataTable ToDataTable<T>(List<T> items)
        //{
        //    DataTable dataTable = new DataTable(typeof(T).Name);

        //    //Get all the properties
        //    PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    foreach (PropertyInfo prop in Props)
        //    {
        //        //Defining type of data column gives proper data table 
        //        var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
        //        //Setting column names as Property names
        //        dataTable.Columns.Add(prop.Name, type);
        //    }
        //    foreach (T item in items)
        //    {
        //        var values = new object[Props.Length];
        //        for (int i = 0; i < Props.Length; i++)
        //        {
        //            //inserting property values to datatable rows
        //            values[i] = Props[i].GetValue(item, null);
        //        }
        //        dataTable.Rows.Add(values);
        //    }
        //    //put a breakpoint here and check datatable
        //    return dataTable;
        //}

        #endregion For Multiple ResutlSet

        #region Get Common Table List

        //public T SetTableParamForCustomEntity<T>(string tableName1, string table1SelectFields, string joinFieldName1,
        //    string tableName2 = "", string table2SelectFields = "", string joinFieldName2 = "",
        //    int pageIndex = 0, int pageSize = 0, string sortIndex = "", string sortDirection = "",
        //    string searchCriteria1 = "", string searchCriteria2 = "") where T : class, new()
        //{
        //    return SetTableParamForCustomList<T>(tableName1, table1SelectFields, joinFieldName1, tableName2, table2SelectFields, joinFieldName2,
        //                        pageIndex, pageSize, sortIndex, sortDirection, searchCriteria1, searchCriteria2).FirstOrDefault();
        //}

        //public List<T> SetTableParamForCustomList<T>(string tableName1, string table1SelectFields, string joinFieldName1,
        //    string tableName2 = "", string table2SelectFields = "", string joinFieldName2 = "",
        //    int pageIndex = 0, int pageSize = 0, string sortIndex = "", string sortDirection = "",
        //    string searchCriteria1 = "", string searchCriteria2 = "") where T : class, new()
        //{
        //    List<SearchValueData> searchData = new List<SearchValueData>();
        //    searchData.Add(new SearchValueData { Name = "TableName1", Value = tableName1 });
        //    searchData.Add(new SearchValueData { Name = "TableName1Fields", Value = SetTableFieldsForCustomList(table1SelectFields) });
        //    searchData.Add(new SearchValueData { Name = "JoinFieldName1", Value = joinFieldName1 });
        //    searchData.Add(new SearchValueData { Name = "TableName2", Value = tableName2 });
        //    searchData.Add(new SearchValueData { Name = "TableName2Fields", Value = SetTableFieldsForCustomList(table2SelectFields, true) });
        //    searchData.Add(new SearchValueData { Name = "JoinFieldName2", Value = joinFieldName2 });
        //    searchData.Add(new SearchValueData { Name = "FromIndex", Value = Convert.ToString(((pageIndex - 1) * pageSize) + 1) });
        //    searchData.Add(new SearchValueData { Name = "ToIndex", Value = Convert.ToString(pageIndex * pageSize) });
        //    searchData.Add(new SearchValueData { Name = "SortExpression", Value = sortIndex });
        //    searchData.Add(new SearchValueData { Name = "SortType", Value = sortDirection });
        //    searchData.Add(new SearchValueData { Name = "SearchCriteria1", Value = searchCriteria1 });
        //    searchData.Add(new SearchValueData { Name = "SearchCriteria2", Value = searchCriteria2 });

        //    return GetEntityList<T>(StoredProcedures.SP_GetRecordsFromTable, searchData);
        //}

        //public string SetTableFieldsForCustomList(string fields, bool isTable2 = false)
        //{
        //    string newFields = string.Empty;
        //    if (!string.IsNullOrEmpty(fields))
        //    {
        //        if (isTable2)
        //        {
        //            List<string> splitFields = Extention.ToStringList(fields);
        //            newFields = "tbl2." + string.Join(",tbl2.", splitFields.ToArray());
        //        }
        //        else
        //        {
        //            List<string> splitFields = Extention.ToStringList(fields);
        //            newFields = "tbl1." + string.Join(",tbl1.", splitFields.ToArray());
        //        }
        //    }
        //    return newFields;
        //}

        //public string SetQueryForMultipleFields(string tableFields, string searchText = "", bool isTable2 = false, int searchOperator = 5)
        //{
        //    string query = string.Empty;
        //    string tableAlias = isTable2 ? Constants.tbl2 : Constants.tbl1;

        //    List<string> table1Fields = Extention.ToStringList(tableFields);
        //    if (table1Fields.Count > 0)
        //    {
        //        int count = 1;
        //        foreach (var item in table1Fields)
        //        {
        //            if (searchOperator == (int)Common.SearchOperator.EqualTo)
        //                query += tableAlias + item + Constants.QueryEqualTo + "'" + searchText + "'";
        //            else if (searchOperator == (int)Common.SearchOperator.NotEqualTo)
        //                query += tableAlias + item + Constants.QueryNotEqualTo + "'" + searchText + "'";
        //            else
        //                query += tableAlias + item + Constants.QueryLike + "'%" + searchText + "%'";

        //            if (table1Fields.Count != count)
        //                query += Constants.QuerySeparator;
        //            count++;
        //        }
        //    }
        //    return string.Format("({0})", query);
        //}

        #endregion

        #region Transaction Management

        public void BeginTransaction()
        {
            _db.BeginTransaction();
        }
        public void AbortTransaction()
        {
            _db.AbortTransaction();
        }

        public void CompleteTransaction()
        {
            _db.CompleteTransaction();
        }


        #endregion
    }
}
