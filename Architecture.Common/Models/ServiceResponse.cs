using System;
using System.Collections.Generic;
using System.Text;

namespace Architecture.Common.Models
{
    public class ServiceResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string ErrorCode { get; set; }
    }

    public class DashboardUpdateResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class SearchModel
    {
        public enum SearchOperator
        {
            EqualTo = 1, NotEqualTo, BeginsWith, EndsWith, Contains, DoesNotContains, GreaterThan, LessThan
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public int OperatorId { get; set; }
    }
}
