using System.Collections.Generic;

namespace Serpent.Core
{
    public class NameplateModel
    {
        public int id { get; set; }
        public string UserID { get; set; }
        public string Text { get; set; }
        public bool Active { get; set; }
    }

    public class NameplateModelList
    {
        public List<NameplateModel> records { get; set; }
    }


}
