using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReModCE_ARES
{
    public static class extension
    {
        public static string replaceThis(this string input, string replaceToBlank)
        {
            try
            {
                if (input != null)
                {
                    return input.Replace("'", "");
                }
                else
                {
                    return "None";
                }
            }
            catch
            {
                return "None";
            }
        }
    }
}
