using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi
{
      #region Datatables.net
    //Start - JSon class sent from Datatables.net

    public class DataTableAjaxPostModel
    {
        // properties are not capital due to json mapping
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public List<Column> columns { get; set; }
        public Search search { get; set; }
        public List<Order> order { get; set; }
    }

    public class Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public Search search { get; set; }
    }

    public class Search
    {
        public string value { get; set; }
        public bool regex { get; set; }
    }

    public class Order
    {
        public int column { get; set; }
        public string dir { get; set; }
    }
    /// End- JSon class sent from Datatables

    //Start - JSon class (data) sent to Datatables.net
    [Serializable]
    public class DataTableResultSet
    {
        /// <summary>value of draw parameter sent by client</summary>
        public int draw;

        /// <summary>total record count in resultset</summary>
        public int recordsTotal;

        /// <summary>filtered record count</summary>
        public int recordsFiltered;

        /// <summary>Array of records.</summary>
        public List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
    }
    //End - JSon class (data) sent to Datatables.net
    #endregion
}
