using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace z.Data
{ 
    public class DataExpression<TFrom> : IDataExpression<TFrom>, IDisposable where TFrom : class
    {
        private HashSet<string> Columns = new HashSet<string>();
        public DataExpressionType ExpType { get; private set; } = DataExpressionType.None;

        public void Dispose()
        {
            Columns = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        public void Exclude<TResult>(Expression<Func<TFrom, TResult>> Column)
        {
            if (ExpType == DataExpressionType.Include)
                throw new Exception("Column must not Exclude when Include expression already set.");

            var body = Column.Body as MemberExpression ?? ((UnaryExpression)Column.Body).Operand as MemberExpression;
            Columns.Add(body.Member.Name);
            ExpType = DataExpressionType.Exclude;
        }

        public void Include<TResult>(Expression<Func<TFrom, TResult>> Column)
        {
            if (ExpType == DataExpressionType.Exclude)
                throw new Exception("Column must not Include when Exclude expression already set.");

            var body = Column.Body as MemberExpression ?? ((UnaryExpression)Column.Body).Operand as MemberExpression;
            Columns.Add(body.Member.Name);
            ExpType = DataExpressionType.Include;
        }

        public bool Result(string Column)
        {
            return Columns.Contains(Column);
        }
    }

    public enum DataExpressionType
    {
        None, Include, Exclude
    }
     
    public interface IDataExpression<TFrom>
    {
        bool Result(string Column);
        void Include<TResult>(Expression<Func<TFrom, TResult>> expression);
        void Exclude<TResult>(Expression<Func<TFrom, TResult>> expression);

        DataExpressionType ExpType { get; }
    }
     
}
