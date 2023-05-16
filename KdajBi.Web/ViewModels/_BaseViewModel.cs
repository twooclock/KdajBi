using KdajBi.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web.ViewModels
{
    public class _BaseViewModel
    {
        public JwtToken Token;
        public long Id;
        public List<string> UserUIShow = new List<string>();
        public _BaseViewModel() { }

    }
}
