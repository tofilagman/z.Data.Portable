using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace z.Data
{
    public class IncludeData<TFrom> : IDataExpression<TFrom>, IDataSetup<TFrom>, IDisposable
    {
        private HashSet<string> Columns { get; set; } = new HashSet<string>();

        public IncludeData(Action<IDataSetup<TFrom>> Config)
        {
            Config(this);
        }

        public void Add<TColumn>(Expression<Func<TFrom, TColumn>> Column)
        {
            var body = Column.Body as MemberExpression ?? ((UnaryExpression)Column.Body).Operand as MemberExpression;
            Columns.Add(body.Member.Name);
        }

        public bool Result(string Column)
        {
            return Columns.Any(x => x == Column);
        }

        ~IncludeData() => this.Dispose();

        public void Dispose()
        {
            Columns = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }

    public class ExcludeData<TFrom> : IDataExpression<TFrom>, IDataSetup<TFrom>, IDisposable
    {
        private HashSet<string> Columns { get; set; } = new HashSet<string>();

        public ExcludeData(Action<IDataSetup<TFrom>> Config)
        {
            Config(this);
        }

        public void Add<TColumn>(Expression<Func<TFrom, TColumn>> Column)
        {
            var body = Column.Body as MemberExpression ?? ((UnaryExpression)Column.Body).Operand as MemberExpression;
            Columns.Add(body.Member.Name);
        }

        public bool Result(string Column)
        {
            return !Columns.Any(x => x == Column);
        }

        ~ExcludeData() => this.Dispose();

        public void Dispose()
        {
            Columns = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }


    public interface IDataExpression<out TFrom>
    {
        bool Result(string Column);
    }

    public interface IDataSetup<TFrom>
    {
        void Add<TColumn>(Expression<Func<TFrom, TColumn>> Column);
    }
}
